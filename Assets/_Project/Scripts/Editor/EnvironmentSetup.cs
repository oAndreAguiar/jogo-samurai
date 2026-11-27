using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Monta o cenário da vila (Floresta de Bambu / vila Edo) na cena Prototype usando só a folha de
/// assets do desenvolvedor (recortes em Sprites/Environment). Não recria a cena: só troca o chão,
/// tira os blocos cinza provisórios e coloca construções e objetos.
/// Menu: JogoJapones > Setup > Montar vila (cenário)
/// </summary>
public static class EnvironmentSetup
{
    private const string Root = "Assets/_Project";
    private const string EnvDir = Root + "/Sprites/Environment";
    private const string ScenePath = Root + "/Scenes/Prototype.unity";

    // Pixels por metro: construções e árvores maiores, objetos pequenos um pouco menores na tela
    private const float BuildingPpu = 34f;
    private const float PropPpu = 48f;

    [MenuItem("JogoJapones/Setup/Montar vila (cenário)")]
    public static void BuildVillage()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        ImportGroundTextures();
        ImportSprites();

        // ---- Chão: grama ----
        Material ground = LoadOrCreateMaterial("M_Ground");
        ground.SetTexture("_BaseMap", Load<Texture2D>("Env_Ground_Grass"));
        ground.SetTextureScale("_BaseMap", new Vector2(25f, 25f)); // um "ladrilho" a cada 4 m
        ground.SetColor("_BaseColor", Color.white);
        EditorUtility.SetDirty(ground);

        // Tira os blocos cinza provisórios (degrau, rampa, torii de cubos)
        GameObject oldBlocks = GameObject.Find("Blocks");
        if (oldBlocks != null)
        {
            Object.DestroyImmediate(oldBlocks);
        }
        GameObject oldVillage = GameObject.Find("Village");
        if (oldVillage != null)
        {
            Object.DestroyImmediate(oldVillage);
        }

        Transform village = new GameObject("Village").transform;
        Quaternion faceCamera = Quaternion.Euler(50f, 0f, 0f); // mesma inclinação da câmera do jogo

        // ---- Ruas ----
        Transform streets = Child(village, "Streets");
        Material stone = StreetMaterial("M_Street_Stone", "Env_Ground_StonePlain");
        Material dirt = StreetMaterial("M_Street_Dirt", "Env_Ground_DirtPlain");
        Street(streets, "MainStreet", new Vector3(0f, 0.02f, 0f), new Vector2(6f, 92f), stone);
        Street(streets, "CrossStreet", new Vector3(0f, 0.015f, 0f), new Vector2(92f, 5f), dirt);
        Street(streets, "ShrinePath", new Vector3(0f, 0.025f, 26f), new Vector2(10f, 8f), stone);

        // ---- Construções (com colisão na base) ----
        Transform buildings = Child(village, "Buildings");
        Building(buildings, "House_Blue1", new Vector3(-10f, 0f, 8f), faceCamera, 3.5f);
        Building(buildings, "Shop_Wood1", new Vector3(11f, 0f, 8f), faceCamera, 3.5f);
        Building(buildings, "House_Blue2", new Vector3(-23f, 0f, 10f), faceCamera, 3.5f);
        Building(buildings, "Shop_Wood2", new Vector3(24f, 0f, 9f), faceCamera, 3.5f);
        Building(buildings, "Storehouse_White", new Vector3(-36f, 0f, 13f), faceCamera, 3.5f);
        Building(buildings, "Longhouse", new Vector3(30f, 0f, 27f), faceCamera, 3.5f);
        Building(buildings, "Shrine_Torii", new Vector3(0f, 0f, 32f), faceCamera, 3f);
        Building(buildings, "House_Blue2", new Vector3(-12f, 0f, -16f), faceCamera, 3.5f);
        Building(buildings, "Shop_Wood2", new Vector3(13f, 0f, -17f), faceCamera, 3.5f);
        Building(buildings, "Storehouse_White", new Vector3(30f, 0f, -21f), faceCamera, 3.5f);
        Building(buildings, "Longhouse", new Vector3(-28f, 0f, -27f), faceCamera, 3.5f);
        Building(buildings, "House_Blue1", new Vector3(-30f, 0f, 30f), faceCamera, 3.5f);

        // Muros e cercas
        Building(buildings, "Wall_StoneCorner", new Vector3(40f, 0f, 36f), faceCamera, 2f);
        Building(buildings, "Wall_StoneSmall", new Vector3(-41f, 0f, -8f), faceCamera, 1.5f);
        Building(buildings, "Wall_StoneSmall", new Vector3(41f, 0f, -8f), faceCamera, 1.5f);
        Building(buildings, "Fence_Wood1", new Vector3(-17f, 0f, 5f), faceCamera, 1.2f);
        Building(buildings, "Fence_Wood2", new Vector3(18f, 0f, 5f), faceCamera, 1.2f);
        Building(buildings, "Fence_Wood3", new Vector3(-19f, 0f, -11f), faceCamera, 1.2f);
        Building(buildings, "Fence_Wood1", new Vector3(20f, 0f, -12f), faceCamera, 1.2f);

        // ---- Natureza e objetos ----
        Transform props = Child(village, "Props");
        // Pinheiros e bordos (tronco com colisão pequena)
        Vector3[] pines = { new Vector3(-18f, 0f, 22f), new Vector3(18f, 0f, 20f), new Vector3(-42f, 0f, 26f), new Vector3(42f, 0f, 44f),
                            new Vector3(-31f, 0f, -40f), new Vector3(24f, 0f, -36f), new Vector3(6f, 0f, -42f), new Vector3(-9f, 0f, 42f) };
        foreach (Vector3 p in pines) Prop(props, "Tree_Pine", p, faceCamera, BuildingPpu, 0.6f);
        Vector3[] maples = { new Vector3(-26f, 0f, -6f), new Vector3(34f, 0f, 4f), new Vector3(9f, 0f, 21f), new Vector3(-14f, 0f, 30f),
                             new Vector3(-40f, 0f, 40f), new Vector3(40f, 0f, -38f) };
        foreach (Vector3 p in maples) Prop(props, "Tree_Maple", p, faceCamera, BuildingPpu, 0.5f);
        // Moitas de bambu
        Vector3[] bamboo = { new Vector3(-38f, 0f, -16f), new Vector3(-36f, 0f, -13f), new Vector3(-40f, 0f, -12f),
                             new Vector3(38f, 0f, 15f), new Vector3(40f, 0f, 18f), new Vector3(36f, 0f, 19f),
                             new Vector3(-20f, 0f, 40f), new Vector3(-17f, 0f, 42f) };
        foreach (Vector3 p in bamboo) Prop(props, "Bamboo", p, faceCamera, BuildingPpu, 0.4f);
        // Lanternas de pedra na rua principal e no santuário
        Prop(props, "Lantern_Stone1", new Vector3(-4f, 0f, 16f), faceCamera, PropPpu, 0.4f);
        Prop(props, "Lantern_Stone2", new Vector3(4f, 0f, 16f), faceCamera, PropPpu, 0.4f);
        Prop(props, "Lantern_Stone1", new Vector3(-5f, 0f, 27f), faceCamera, PropPpu, 0.4f);
        Prop(props, "Lantern_Stone2", new Vector3(5f, 0f, 27f), faceCamera, PropPpu, 0.4f);
        Prop(props, "Lantern_Stone1", new Vector3(-4f, 0f, -11f), faceCamera, PropPpu, 0.4f);
        Prop(props, "Lantern_Stone2", new Vector3(4f, 0f, -11f), faceCamera, PropPpu, 0.4f);
        // Carroça, bancos, vasos e pedras
        Prop(props, "Cart", new Vector3(7f, 0f, -13f), faceCamera, PropPpu, 1f);
        Prop(props, "Bench1", new Vector3(-8f, 0f, -6f), faceCamera, PropPpu, 0f);
        Prop(props, "Bench2", new Vector3(8f, 0f, -6f), faceCamera, PropPpu, 0f);
        Prop(props, "Bench1", new Vector3(-7f, 0f, 22f), faceCamera, PropPpu, 0f);
        Prop(props, "Plant_Pot1", new Vector3(-5f, 0f, 6f), faceCamera, PropPpu, 0f);
        Prop(props, "Plant_Pot2", new Vector3(15f, 0f, 5f), faceCamera, PropPpu, 0f);
        Prop(props, "Plant_PotSmall", new Vector3(-17f, 0f, -12f), faceCamera, PropPpu, 0f);
        Prop(props, "Plant_Pot1", new Vector3(26f, 0f, -18f), faceCamera, PropPpu, 0f);
        Prop(props, "Rocks", new Vector3(20f, 0f, -7f), faceCamera, PropPpu, 0.6f);
        Prop(props, "Rocks", new Vector3(-24f, 0f, 17f), faceCamera, PropPpu, 0.6f);
        Prop(props, "Rocks", new Vector3(31f, 0f, -31f), faceCamera, PropPpu, 0.6f);

        // ---- Ordem de desenho pelo pé (quem está atrás de uma casa fica escondido) ----
        GraphicsSettings.transparencySortMode = TransparencySortMode.CustomAxis;
        GraphicsSettings.transparencySortAxis = new Vector3(0f, 0f, 1f);
        Camera main = Camera.main;
        if (main != null)
        {
            main.transparencySortMode = TransparencySortMode.CustomAxis;
            main.transparencySortAxis = new Vector3(0f, 0f, 1f);
        }
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            foreach (SpriteRenderer sr in player.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sr.spriteSortPoint = SpriteSortPoint.Pivot;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[EnvironmentSetup] Vila montada.");
    }

    // Tira um print da câmera do jogo (para conferir sem abrir o editor)
    public static void CaptureScreenshot()
    {
        EditorSceneManager.OpenScene(ScenePath);
        Camera cam = Camera.main;
        RenderTexture rt = new RenderTexture(1600, 900, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        Texture2D shot = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        shot.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
        shot.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        string path = System.Environment.GetEnvironmentVariable("SCREENSHOT_PATH") ?? "Temp/village.png";
        File.WriteAllBytes(path, shot.EncodeToPNG());
    }

    // ---------------- montagem ----------------

    private static Transform Child(Transform parent, string name)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(parent, false);
        return t;
    }

    private static void Street(Transform parent, string name, Vector3 center, Vector2 size, Material material)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        Object.DestroyImmediate(quad.GetComponent<Collider>()); // a rua é só visual; o chão já tem colisão
        quad.transform.SetParent(parent, false);
        quad.transform.position = center;
        quad.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        quad.transform.localScale = new Vector3(size.x, size.y, 1f);
        MeshRenderer mr = quad.GetComponent<MeshRenderer>();
        // Cada rua com o próprio material para o ladrilho acompanhar o tamanho (um a cada 3 m)
        Material instance = new Material(material) { name = material.name + "_" + name };
        instance.SetTextureScale("_BaseMap", new Vector2(size.x / 3f, size.y / 3f));
        string path = Root + "/Materials/" + instance.name + ".mat";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(instance, path);
        mr.sharedMaterial = instance;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        quad.isStatic = true;
    }

    // Construção: sprite em pé virado para a câmera + caixa de colisão na base
    private static void Building(Transform parent, string sprite, Vector3 position, Quaternion faceCamera, float colliderHeight)
    {
        GameObject root = Place(parent, sprite, position, faceCamera, BuildingPpu);
        Sprite s = Load<Sprite>("Env_" + sprite);
        float width = s.rect.width / BuildingPpu;
        // A base desenhada (3/4) ocupa mais ou menos metade da altura do desenho em profundidade
        float depth = Mathf.Max(1f, width * 0.45f);
        BoxCollider box = root.AddComponent<BoxCollider>();
        box.size = new Vector3(width * 0.82f, colliderHeight, depth);
        box.center = new Vector3(0f, colliderHeight * 0.5f, depth * 0.5f);
        root.isStatic = true;
    }

    // Objeto: sprite em pé; colisão pequena opcional (tronco, lanterna, pedra)
    private static void Prop(Transform parent, string sprite, Vector3 position, Quaternion faceCamera, float ppu, float colliderRadius)
    {
        GameObject root = Place(parent, sprite, position, faceCamera, ppu);
        if (colliderRadius > 0f)
        {
            CapsuleCollider capsule = root.AddComponent<CapsuleCollider>();
            capsule.radius = colliderRadius;
            capsule.height = 2f;
            capsule.center = new Vector3(0f, 1f, colliderRadius);
        }
        root.isStatic = true;
    }

    private static GameObject Place(Transform parent, string sprite, Vector3 position, Quaternion faceCamera, float ppu)
    {
        GameObject root = new GameObject(sprite);
        root.transform.SetParent(parent, false);
        root.transform.position = position;
        GameObject visual = new GameObject("Sprite");
        visual.transform.SetParent(root.transform, false);
        visual.transform.rotation = faceCamera;
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = Load<Sprite>("Env_" + sprite);
        sr.spriteSortPoint = SpriteSortPoint.Pivot;
        // Recortes estão em BuildingPpu ou PropPpu; ajusta a escala se este objeto usar outro valor
        float importedPpu = sr.sprite.pixelsPerUnit;
        visual.transform.localScale = Vector3.one * (importedPpu / ppu);
        return root;
    }

    // ---------------- importação ----------------

    private static void ImportGroundTextures()
    {
        foreach (string name in new[] { "Env_Ground_Grass", "Env_Ground_DirtPlain", "Env_Ground_StonePlain",
                                         "Env_Ground_Dirt", "Env_Ground_Stone", "Env_Ground_GrassDirt", "Env_Ground_Canal" })
        {
            string path = EnvDir + "/" + name + ".png";
            AssetDatabase.ImportAsset(path);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Default;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Mirror; // espelhar esconde as emendas do ladrilho
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static void ImportSprites()
    {
        foreach (string file in Directory.GetFiles(EnvDir, "Env_*.png"))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            if (name.StartsWith("Env_Ground_")) continue;
            string path = EnvDir + "/" + name + ".png";
            AssetDatabase.ImportAsset(path);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            bool small = name.Contains("Lantern") || name.Contains("Bench") || name.Contains("Plant") || name.Contains("Rocks") || name.Contains("Cart");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = small ? PropPpu : BuildingPpu;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter; // pivô no pé
            importer.SetTextureSettings(settings);
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static Material StreetMaterial(string name, string texture)
    {
        Material m = LoadOrCreateMaterial(name);
        m.SetTexture("_BaseMap", Load<Texture2D>(texture));
        m.SetColor("_BaseColor", Color.white);
        m.SetFloat("_Smoothness", 0.05f);
        EditorUtility.SetDirty(m);
        return m;
    }

    private static Material LoadOrCreateMaterial(string name)
    {
        string path = Root + "/Materials/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }

    private static T Load<T>(string name) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(EnvDir + "/" + name + ".png");
    }
}
