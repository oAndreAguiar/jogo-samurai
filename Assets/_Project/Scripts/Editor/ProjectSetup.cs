using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;

/// <summary>
/// Script de Editor: configura o projeto para WebGL e monta a cena Prototype (etapas 0, 1 e 2).
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
        "Sprites", "Prefabs", "Materials", "ScriptableObjects", "ScriptableObjects/Player", "ScriptableObjects/Enemies", "ScriptableObjects/World", "Audio"
    };

    // Chamado pela linha de comando: roda tudo de uma vez
    public static void RunAll()
    {
        CreateFolders();
        ConfigurePlayerSettings();
        CreatePrototypeScene();
        RemoveSampleScene();
        AssetDatabase.SaveAssets();
        Debug.Log("[ProjectSetup] Setup concluído.");
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

        // Degrau baixo e rampa para testar subida (etapa 1)
        GameObject step = CreateBlock(blocks.transform, new Vector3(-3f, 0f, 3f), new Vector3(2f, 0.3f, 2f), blockMat);
        step.name = "LowStep";
        GameObject ramp = CreateBlock(blocks.transform, new Vector3(2f, 0f, 4f), new Vector3(4.12f, 0.2f, 3f), blockMat);
        ramp.name = "Ramp";
        ramp.transform.position = new Vector3(2f, 0.45f, 4f);
        ramp.transform.rotation = Quaternion.Euler(0f, 0f, 14f); // sobe até o bloco de 1 m

        // Jogador: CharacterController + movimento + sprite com Billboard
        Sprite playerSprite = CreatePlayerSprite();
        GameObject player = new GameObject("Player");
        player.transform.position = Vector3.zero;
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.6f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0f, 0.8f, 0f);
        cc.skinWidth = 0.05f;
        cc.minMoveDistance = 0f; // evita ignorar passos pequenos em FPS alto

        GameObject visual = new GameObject("Sprite");
        visual.transform.SetParent(player.transform, false);
        visual.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = playerSprite;
        visual.AddComponent<Billboard>();

        PlayerMovement movement = player.AddComponent<PlayerMovement>();
        SerializedObject so = new SerializedObject(movement);
        so.FindProperty("settings").objectReferenceValue = GetOrCreateMovementSettings();
        so.FindProperty("spriteRenderer").objectReferenceValue = sr;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Câmera principal (quem move é o Cinemachine)
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 40f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color32(20, 18, 24, 255);
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<CinemachineBrain>();
        camGo.transform.position = new Vector3(0f, 14f, -12f);
        camGo.transform.rotation = Quaternion.Euler(50f, 0f, 0f);

        // Câmera virtual do Cinemachine: segue o jogador de cima, inclinada, sem girar
        GameObject cmGo = new GameObject("CM FollowCamera");
        cmGo.transform.position = new Vector3(0f, 14f, -12f);
        cmGo.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
        CinemachineCamera cmCam = cmGo.AddComponent<CinemachineCamera>();
        cmCam.Follow = player.transform;
        cmCam.Lens.FieldOfView = 40f;
        CinemachineFollow follow = cmGo.AddComponent<CinemachineFollow>();
        follow.FollowOffset = new Vector3(0f, 14f, -12f);
        follow.TrackerSettings.BindingMode = BindingMode.WorldSpace;
        follow.TrackerSettings.PositionDamping = new Vector3(0.4f, 0.4f, 0.4f);

        GameObject canvasGo = CreateMobileJoystick();
        CreateHorde(player.transform, canvasGo);

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

    private static PlayerMovementSettings GetOrCreateMovementSettings()
    {
        string path = Root + "/ScriptableObjects/Player/PlayerMovementSettings.asset";
        PlayerMovementSettings settings = AssetDatabase.LoadAssetAtPath<PlayerMovementSettings>(path);
        if (settings == null)
        {
            Directory.CreateDirectory(Root + "/ScriptableObjects/Player");
            settings = ScriptableObject.CreateInstance<PlayerMovementSettings>();
            AssetDatabase.CreateAsset(settings, path);
        }
        return settings;
    }

    // Canvas com joystick virtual (Input System OnScreenStick) no canto inferior esquerdo
    private static GameObject CreateMobileJoystick()
    {
        Sprite circle = CreateCircleSprite();

        GameObject canvasGo = new GameObject("HUD Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();

        GameObject joyRoot = new GameObject("MobileJoystick", typeof(RectTransform));
        joyRoot.transform.SetParent(canvasGo.transform, false);
        RectTransform rootRt = (RectTransform)joyRoot.transform;
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.zero;
        rootRt.pivot = new Vector2(0.5f, 0.5f);
        rootRt.anchoredPosition = new Vector2(240f, 240f);
        rootRt.sizeDelta = new Vector2(280f, 280f);
        Image bg = joyRoot.AddComponent<Image>();
        bg.sprite = circle;
        bg.color = new Color(1f, 1f, 1f, 0.25f);
        bg.raycastTarget = false;
        joyRoot.AddComponent<MobileJoystickVisibility>();

        GameObject knob = new GameObject("Knob", typeof(RectTransform));
        knob.transform.SetParent(joyRoot.transform, false);
        RectTransform knobRt = (RectTransform)knob.transform;
        knobRt.sizeDelta = new Vector2(120f, 120f);
        Image knobImg = knob.AddComponent<Image>();
        knobImg.sprite = circle;
        knobImg.color = new Color(1f, 1f, 1f, 0.7f);
        OnScreenStick stick = knob.AddComponent<OnScreenStick>();
        stick.controlPath = "<Gamepad>/leftStick";
        stick.movementRange = 100f;
        return canvasGo;
    }

    // Horda (etapa 2): dados dos inimigos, spawner, movimento e texto de debug
    private static void CreateHorde(Transform player, GameObject canvasGo)
    {
        Directory.CreateDirectory(Root + "/ScriptableObjects/Enemies");
        Directory.CreateDirectory(Root + "/ScriptableObjects/World");

        Sprite oniSprite = CreateEnemySprite("Enemy_Oni", new Color32(150, 60, 170, 255), false);
        Sprite flameSprite = CreateEnemySprite("Enemy_Hitodama", new Color32(80, 170, 255, 255), true);
        EnemyData oni = GetOrCreateEnemy("Enemy_Oni", "Oni", oniSprite, 2.5f, 3f, 0.6f);
        EnemyData flame = GetOrCreateEnemy("Enemy_Hitodama", "Hitodama", flameSprite, 4f, 1f, 0.7f);

        string hordePath = Root + "/ScriptableObjects/World/HordeSettings.asset";
        HordeSettings horde = AssetDatabase.LoadAssetAtPath<HordeSettings>(hordePath);
        if (horde == null)
        {
            horde = ScriptableObject.CreateInstance<HordeSettings>();
            horde.enemyTypes.Add(oni);
            horde.enemyTypes.Add(flame);
            AssetDatabase.CreateAsset(horde, hordePath);
        }

        GameObject hordeGo = new GameObject("Horde");
        HordeSpawner spawner = hordeGo.AddComponent<HordeSpawner>();
        SerializedObject spawnerSo = new SerializedObject(spawner);
        spawnerSo.FindProperty("settings").objectReferenceValue = horde;
        spawnerSo.FindProperty("player").objectReferenceValue = player;
        spawnerSo.ApplyModifiedPropertiesWithoutUndo();

        EnemyMovementSystem movement = hordeGo.AddComponent<EnemyMovementSystem>();
        SerializedObject movementSo = new SerializedObject(movement);
        movementSo.FindProperty("spawner").objectReferenceValue = spawner;
        movementSo.ApplyModifiedPropertiesWithoutUndo();

        // Texto de debug no topo esquerdo
        GameObject textGo = new GameObject("HordeDebugText", typeof(RectTransform));
        textGo.transform.SetParent(canvasGo.transform, false);
        RectTransform rt = (RectTransform)textGo.transform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(24f, -20f);
        rt.sizeDelta = new Vector2(1200f, 60f);
        Text text = textGo.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 36;
        text.color = Color.white;
        text.raycastTarget = false;
        text.text = "00:00";
        Outline outline = textGo.AddComponent<Outline>();
        outline.effectColor = Color.black;

        HordeDebugHUD hud = textGo.AddComponent<HordeDebugHUD>();
        SerializedObject hudSo = new SerializedObject(hud);
        hudSo.FindProperty("spawner").objectReferenceValue = spawner;
        hudSo.FindProperty("label").objectReferenceValue = text;
        hudSo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static EnemyData GetOrCreateEnemy(string assetName, string displayName, Sprite sprite, float speed, float weight, float height)
    {
        string path = Root + "/ScriptableObjects/Enemies/" + assetName + ".asset";
        EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<EnemyData>();
            data.displayName = displayName;
            data.moveSpeed = speed;
            data.spawnWeight = weight;
            data.spriteHeight = height;
            AssetDatabase.CreateAsset(data, path);
        }
        data.sprite = sprite;
        EditorUtility.SetDirty(data);
        return data;
    }

    // Sprite provisório 24x24 de inimigo: fantasma (oni) ou chama flutuante (hitodama)
    private static Sprite CreateEnemySprite(string name, Color32 body, bool flame)
    {
        string path = Root + "/Sprites/" + name + ".png";
        const int size = 24;
        Color32 clear = new Color32(0, 0, 0, 0);
        Color32 outline = new Color32(16, 12, 16, 255);
        Color32 eye = new Color32(255, 230, 80, 255);
        Color32 light = new Color32(
            (byte)Mathf.Min(255, body.r + 60), (byte)Mathf.Min(255, body.g + 60), (byte)Mathf.Min(255, body.b + 60), 255);

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Color32 c = clear;
                float dx = x - 11.5f;
                if (flame)
                {
                    // Gota de fogo: larga embaixo, ponta em cima
                    float halfWidth = y < 10 ? Mathf.Sqrt(Mathf.Max(0f, 49f - (y - 8) * (y - 8))) : 7f * (1f - (y - 10) / 12f);
                    if (y >= 2 && y <= 21 && Mathf.Abs(dx) <= halfWidth) c = Mathf.Abs(dx) < halfWidth * 0.45f && y < 14 ? light : body;
                    if ((x == 9 || x == 14) && (y == 8 || y == 9)) c = outline;
                }
                else
                {
                    // Fantasma com chifres: cabeça redonda, barra ondulada embaixo
                    bool head = dx * dx + (y - 13) * (y - 13) <= 64;
                    bool skirt = y >= 3 && y < 13 && Mathf.Abs(dx) <= 8 && !(y < 5 && (x % 4 == 0));
                    bool horn = (y >= 20 && y <= 22) && (x == 7 + (y - 20) / 2 || x == 16 - (y - 20) / 2);
                    if (head || skirt) c = body;
                    if (horn) c = eye;
                    if ((x == 9 || x == 14) && (y == 13 || y == 14)) c = eye;
                    if (y == 9 && x >= 10 && x <= 13) c = outline;
                }
                tex.SetPixel(x, y, c);
            }
        }

        // Contorno escuro para destacar na horda
        Color32[] src = tex.GetPixels32();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (src[y * size + x].a != 0) continue;
                bool near = false;
                for (int oy = -1; oy <= 1 && !near; oy++)
                {
                    for (int ox = -1; ox <= 1; ox++)
                    {
                        int nx = x + ox, ny = y + oy;
                        if (nx < 0 || ny < 0 || nx >= size || ny >= size) continue;
                        if (src[ny * size + nx].a != 0) { near = true; break; }
                    }
                }
                if (near) tex.SetPixel(x, y, outline);
            }
        }

        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 20f;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // Círculo branco 128x128 para o joystick
    private static Sprite CreateCircleSprite()
    {
        string path = Root + "/Sprites/UI_Circle.png";
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size * 0.5f - 1f;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float a = Mathf.Clamp01(r - d + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
