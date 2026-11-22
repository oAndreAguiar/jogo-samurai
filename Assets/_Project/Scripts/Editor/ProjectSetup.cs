using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Script de Editor da etapa 0: configura o projeto para WebGL e monta a cena Prototype.
/// Pode ser rodado de novo pelo menu "JogoJapones > Setup > ..." sem quebrar nada.
/// </summary>
public static class ProjectSetup
{
    private const string Root = "Assets/_Project";
    private const string ScenePath = Root + "/Scenes/Prototype.unity";

    private static readonly string[] Folders =
    {
        "Scenes", "Scripts/Core", "Scripts/Player", "Scripts/Enemies", "Scripts/Combat",
        "Scripts/Progression", "Scripts/World", "Scripts/UI", "Scripts/Utils",
        "Sprites", "Prefabs", "Materials", "ScriptableObjects", "Audio"
    };

    // Chamado pela linha de comando: roda tudo de uma vez
    public static void RunAll()
    {
        CreateFolders();
        ConfigurePlayerSettings();
        CreatePrototypeScene();
        RemoveSampleScene();
        AssetDatabase.SaveAssets();
        Debug.Log("[ProjectSetup] Etapa 0 concluída.");
    }

    [MenuItem("JogoJapones/Setup/Criar pastas")]
    public static void CreateFolders()
    {
        foreach (string folder in Folders)
        {
            Directory.CreateDirectory(Path.Combine(Root, folder));
            // .gitkeep para o Git guardar pastas vazias
            string keep = Path.Combine(Root, folder, ".gitkeep");
            if (!File.Exists(keep))
            {
                File.WriteAllText(keep, "");
            }
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("JogoJapones/Setup/Configurar WebGL")]
    public static void ConfigurePlayerSettings()
    {
        PlayerSettings.companyName = "JogoJapones";
        PlayerSettings.productName = "JogoJapones";

        // Gzip com fallback funciona em qualquer servidor (itch.io incluso) e no celular
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        }
    }

    [MenuItem("JogoJapones/Setup/Recriar cena Prototype")]
    public static void CreatePrototypeScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Texturas pixel art provisórias (filtro Point)
        Texture2D grassTex = CreatePixelTexture("Ground_Grass", new Color32(74, 122, 58, 255), new Color32(60, 102, 47, 255), 16, 11);
        Texture2D stoneTex = CreatePixelTexture("Block_Stone", new Color32(120, 112, 104, 255), new Color32(96, 90, 84, 255), 16, 23);

        Material groundMat = CreateMaterial("M_Ground", grassTex, new Vector2(25f, 25f));
        Material blockMat = CreateMaterial("M_Block", stoneTex, Vector2.one);
        Material toriiMat = CreateMaterial("M_Torii", null, Vector2.one, new Color32(196, 40, 32, 255));

        // Chão
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(10f, 1f, 10f); // 100 x 100 metros
        ground.GetComponent<MeshRenderer>().sharedMaterial = groundMat;

        // Blocos de alturas diferentes para testar relevo e colisão
        GameObject blocks = new GameObject("Blocks");
        CreateBlock(blocks.transform, new Vector3(6f, 0f, 4f), new Vector3(4f, 1f, 4f), blockMat);
        CreateBlock(blocks.transform, new Vector3(-7f, 0f, 6f), new Vector3(3f, 2f, 6f), blockMat);
        CreateBlock(blocks.transform, new Vector3(10f, 0f, -8f), new Vector3(6f, 3f, 2f), blockMat);
        CreateBlock(blocks.transform, new Vector3(-12f, 0f, -10f), new Vector3(5f, 4f, 5f), blockMat);
        CreateBlock(blocks.transform, new Vector3(0f, 0f, 14f), new Vector3(12f, 1.5f, 2f), blockMat);
        // Pilares vermelhos tipo torii, só para dar referência visual
        CreateBlock(blocks.transform, new Vector3(-2f, 0f, -6f), new Vector3(0.6f, 3f, 0.6f), toriiMat);
        CreateBlock(blocks.transform, new Vector3(2f, 0f, -6f), new Vector3(0.6f, 3f, 0.6f), toriiMat);
        GameObject beam = CreateBlock(blocks.transform, new Vector3(0f, 3f, -6f), new Vector3(5.5f, 0.5f, 0.6f), toriiMat);
        beam.name = "ToriiBeam";

        // Luz direcional
        GameObject lightGo = new GameObject("Directional Light");
        Light light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.95f, 0.85f);
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.45f, 0.47f, 0.55f);

        // Personagem provisório (sprite com Billboard)
        Sprite playerSprite = CreatePlayerSprite();
        GameObject player = new GameObject("PlayerPlaceholder");
        player.transform.position = Vector3.zero;
        GameObject visual = new GameObject("Sprite");
        visual.transform.SetParent(player.transform, false);
        visual.transform.localPosition = new Vector3(0f, 0.75f, 0f);
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = playerSprite;
        visual.AddComponent<Billboard>();

        // Câmera de cima, inclinada, em perspectiva
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 40f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color32(20, 18, 24, 255);
        camGo.AddComponent<AudioListener>();
        camGo.transform.position = new Vector3(0f, 14f, -12f);
        camGo.transform.rotation = Quaternion.Euler(50f, 0f, 0f);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }

    // Tira um print da câmera principal da cena aberta (para conferir a cena sem abrir o editor)
    public static void CaptureScreenshot()
    {
        EditorSceneManager.OpenScene(ScenePath);
        Camera cam = Camera.main;
        RenderTexture rt = new RenderTexture(1280, 720, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        Texture2D shot = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        shot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        shot.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        string path = System.Environment.GetEnvironmentVariable("SCREENSHOT_PATH") ?? "Temp/prototype.png";
        File.WriteAllBytes(path, shot.EncodeToPNG());
        Debug.Log("[ProjectSetup] Screenshot salvo em " + path);
    }

    private static void RemoveSampleScene()
    {
        AssetDatabase.DeleteAsset("Assets/Scenes/SampleScene.unity");
        AssetDatabase.DeleteAsset("Assets/Settings/SampleSceneProfile.asset");
        if (AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            string[] left = AssetDatabase.FindAssets("", new[] { "Assets/Scenes" });
            if (left.Length == 0)
            {
                AssetDatabase.DeleteAsset("Assets/Scenes");
            }
        }
    }

    private static GameObject CreateBlock(Transform parent, Vector3 basePos, Vector3 size, Material mat)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = "Block";
        block.transform.SetParent(parent, false);
        block.transform.localScale = size;
        // basePos é o pé do bloco, então sobe metade da altura
        block.transform.position = basePos + new Vector3(0f, size.y * 0.5f, 0f);
        block.GetComponent<MeshRenderer>().sharedMaterial = mat;
        block.isStatic = true;
        return block;
    }

    private static Material CreateMaterial(string name, Texture2D tex, Vector2 tiling, Color? color = null)
    {
        string path = Root + "/Materials/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        if (tex != null)
        {
            mat.SetTexture("_BaseMap", tex);
            mat.SetTextureScale("_BaseMap", tiling);
        }
        mat.SetColor("_BaseColor", color ?? Color.white);
        mat.SetFloat("_Smoothness", 0.1f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // Textura 16x16 com ruído simples de duas cores, salva como PNG
    private static Texture2D CreatePixelTexture(string name, Color32 a, Color32 b, int size, int seed)
    {
        string path = Root + "/Sprites/" + name + ".png";
        Random.State old = Random.state;
        Random.InitState(seed);
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, Random.value < 0.3f ? b : a);
            }
        }
        Random.state = old;
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Default;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // Sprite provisório 32x32 de um "samurai": corpo vermelho, faixa dourada e cabeça
    private static Sprite CreatePlayerSprite()
    {
        string path = Root + "/Sprites/Player_Placeholder.png";
        const int size = 32;
        Color32 clear = new Color32(0, 0, 0, 0);
        Color32 outline = new Color32(16, 12, 16, 255);
        Color32 body = new Color32(196, 40, 32, 255);
        Color32 gold = new Color32(222, 180, 64, 255);
        Color32 skin = new Color32(240, 205, 170, 255);
        Color32 hair = new Color32(30, 24, 30, 255);

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Color32 c = clear;
                bool bodyArea = x >= 10 && x <= 21 && y >= 2 && y <= 18;
                bool headArea = (x - 16) * (x - 16) + (y - 24) * (y - 24) <= 25;
                if (bodyArea) c = (y == 10 || y == 11) ? gold : body;
                if (headArea) c = y >= 26 ? hair : skin;
                if ((x == 14 || x == 18) && y == 24) c = outline; // olhos
                tex.SetPixel(x, y, c);
            }
        }
        // Contorno escuro para a silhueta destacar na horda
        Color32[] src = tex.GetPixels32();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (src[y * size + x].a != 0) continue;
                bool nearFilled = false;
                for (int dy = -1; dy <= 1 && !nearFilled; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= size || ny >= size) continue;
                        if (src[ny * size + nx].a != 0) { nearFilled = true; break; }
                    }
                }
                if (nearFilled) tex.SetPixel(x, y, outline);
            }
        }
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 20f; // 32 px = 1,6 m de altura
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
