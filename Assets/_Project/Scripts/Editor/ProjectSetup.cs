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
/// Script de Editor: configura o projeto para WebGL e monta a cena Prototype (etapas 0 a 7).
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
        "Sprites", "Prefabs", "Materials", "ScriptableObjects", "ScriptableObjects/Player", "ScriptableObjects/Enemies", "ScriptableObjects/World", "ScriptableObjects/Combat", "ScriptableObjects/Upgrades", "ScriptableObjects/Progression", "Audio"
    };

    // Chamado pela linha de comando: roda tudo de uma vez
    public static void RunAll()
    {
        CreateFolders();
        ConfigurePlayerSettings();
        CreatePrototypeScene();
        CreateMainMenuScene();
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
        HordeSpawner spawner = CreateHorde(player.transform, canvasGo);
        CreateCombat(player, sr, spawner, canvasGo);

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
    private static HordeSpawner CreateHorde(Transform player, GameObject canvasGo)
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
        rt.anchoredPosition = new Vector2(24f, -40f);
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
        return spawner;
    }

    // Combate (etapa 3): shuriken automática, números de dano, vida do jogador e tela de derrota
    private static void CreateCombat(GameObject player, SpriteRenderer playerSprite, HordeSpawner spawner, GameObject canvasGo)
    {
        Directory.CreateDirectory(Root + "/ScriptableObjects/Combat");

        // Dados
        DamageNumberSettings numberSettings = GetOrCreateAsset<DamageNumberSettings>(Root + "/ScriptableObjects/Combat/DamageNumberSettings.asset");
        numberSettings.digitsTexture = CreateDigitsTexture();
        EditorUtility.SetDirty(numberSettings);

        PlayerStats stats = GetOrCreateAsset<PlayerStats>(Root + "/ScriptableObjects/Player/PlayerStats.asset");

        // Sistemas de combate
        GameObject combatGo = new GameObject("Combat");
        ProjectileSystem projectiles = combatGo.AddComponent<ProjectileSystem>();
        SetRefs(projectiles, ("spawner", spawner));
        DamageNumberSystem numbers = combatGo.AddComponent<DamageNumberSystem>();
        SetRefs(numbers, ("settings", numberSettings), ("spawner", spawner));

        // Jogador: vida (as habilidades vêm do PlayerLoadout, etapa 4)
        PlayerHealth health = player.AddComponent<PlayerHealth>();
        SetRefs(health, ("stats", stats), ("spawner", spawner), ("spriteRenderer", playerSprite));

        // Barra de vida embaixo do personagem
        Sprite pixel = CreatePixelSprite();
        GameObject bar = new GameObject("HealthBar");
        bar.transform.SetParent(player.transform, false);
        bar.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        SpriteRenderer barBg = new GameObject("Background").AddComponent<SpriteRenderer>();
        barBg.transform.SetParent(bar.transform, false);
        barBg.transform.localScale = new Vector3(1.1f, 0.16f, 1f);
        barBg.sprite = pixel;
        barBg.color = new Color(0.08f, 0.06f, 0.08f, 0.9f);
        barBg.sortingOrder = 60;
        SpriteRenderer barFill = new GameObject("Fill").AddComponent<SpriteRenderer>();
        barFill.transform.SetParent(bar.transform, false);
        barFill.transform.localScale = new Vector3(1.02f, 0.09f, 1f);
        barFill.sprite = pixel;
        barFill.sortingOrder = 61;
        PlayerHealthBar healthBar = bar.AddComponent<PlayerHealthBar>();
        SetRefs(healthBar, ("health", health), ("fill", barFill.transform), ("fillRenderer", barFill));

        // Tela de derrota
        GameObject panel = new GameObject("GameOverPanel", typeof(RectTransform));
        panel.transform.SetParent(canvasGo.transform, false);
        RectTransform panelRt = (RectTransform)panel.transform;
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        Image dim = panel.AddComponent<Image>();
        dim.color = new Color(0.05f, 0.02f, 0.03f, 0.75f);

        Text title = CreateText(panel.transform, "Title", "Você caiu", 110, new Color(0.85f, 0.15f, 0.12f), new Vector2(0f, 90f));
        Text summary = CreateText(panel.transform, "Summary", "", 44, Color.white, new Vector2(0f, -70f));
        title.fontStyle = FontStyle.Bold;

        GameObject gameOverGo = new GameObject("GameOver");
        GameOverScreen gameOver = gameOverGo.AddComponent<GameOverScreen>();
        SetRefs(gameOver, ("playerHealth", health), ("spawner", spawner), ("panel", panel), ("summaryText", summary));

        ExperienceGemSystem gems = CreateProgression(player, spawner, projectiles, health, canvasGo);
        CreateZones(spawner, gems, health, canvasGo);
        CreateFullHud(player, playerSprite, spawner, health, canvasGo);
        // A derrota mostra até onde o jogador chegou e o tempo total
        SetRefs(gameOver, ("zones", Object.FindFirstObjectByType<ZoneController>()),
            ("sanctuary", Object.FindFirstObjectByType<SanctuaryController>()), ("clock", Object.FindFirstObjectByType<RunClock>()));
        // Tela de derrota por cima de tudo
        panel.transform.SetAsLastSibling();
    }

    // Progressão (etapa 4): XP, joias, habilidades, passivas e tela de level-up
    private static ExperienceGemSystem CreateProgression(GameObject player, HordeSpawner spawner, ProjectileSystem projectiles, PlayerHealth health, GameObject canvasGo)
    {
        string upgradesDir = Root + "/ScriptableObjects/Upgrades";
        Directory.CreateDirectory(upgradesDir);

        // ---- Sprites provisórios ----
        Sprite shurikenSprite = CreateShurikenSprite();
        Sprite ringSprite = CreateRingSprite();
        Sprite ofudaSprite = PaintSprite("Ofuda", 8, 14, 16f, (x, y) =>
            (y == 9 || y == 10) && x >= 2 && x <= 5 ? new Color32(200, 30, 30, 255)
            : (x == 3 || x == 4) && y >= 3 && y <= 7 ? new Color32(200, 30, 30, 255)
            : new Color32(245, 235, 200, 255));
        Sprite smallGem = PaintSprite("Gem_Small", 7, 9, 22f, (x, y) =>
            Mathf.Abs(x - 3) + Mathf.Abs(y - 4) * 0.75f <= 3.2f ? (x < 3 ? new Color32(150, 255, 160, 255) : new Color32(60, 200, 90, 255)) : (Color32?)null);
        Sprite bigGem = PaintSprite("Gem_Big", 9, 11, 20f, (x, y) =>
            Mathf.Abs(x - 4) + Mathf.Abs(y - 5) * 0.8f <= 4.2f ? (x < 4 ? new Color32(150, 210, 255, 255) : new Color32(50, 120, 230, 255)) : (Color32?)null);
        Sprite katanaIcon = PaintSprite("Icon_Katana", 16, 16, 16f, (x, y) =>
            Mathf.Abs(x - y) <= 1 && x >= 4 ? new Color32(220, 225, 235, 255)
            : Mathf.Abs(x - y) <= 1 && x < 4 ? new Color32(120, 40, 40, 255)
            : (x + y == 7 || x + y == 8) && x >= 2 && x <= 5 ? new Color32(222, 180, 64, 255) : (Color32?)null);
        Sprite iconSpeed = PaintSprite("Icon_Speed", 16, 16, 16f, (x, y) =>
            x >= 3 && x <= 12 && y >= 3 && y <= 5 ? new Color32(120, 80, 40, 255)
            : (x == 5 || x == 10) && y >= 6 && y <= 8 ? new Color32(120, 80, 40, 255)
            : x >= 4 && x <= 11 && y >= 9 && y <= 10 ? new Color32(160, 110, 60, 255) : (Color32?)null);
        Sprite iconHealth = PaintSprite("Icon_Health", 16, 16, 16f, (x, y) =>
            y <= 12 && Mathf.Abs(x - 7.5f) <= (12 - y) * 0.6f + 0.5f && y >= 2
                ? (y <= 4 ? new Color32(30, 30, 40, 255) : new Color32(250, 250, 245, 255)) : (Color32?)null);
        Sprite iconDamage = PaintSprite("Icon_Damage", 16, 16, 16f, (x, y) =>
            x >= 2 && x <= 13 && y >= 5 && y <= 9 ? (y == 9 ? new Color32(170, 170, 180, 255) : new Color32(110, 110, 125, 255)) : (Color32?)null);
        Sprite iconCooldown = PaintSprite("Icon_Cooldown", 16, 16, 16f, (x, y) =>
            (x == 7 || x == 8) && y >= 2 && y <= 9 ? new Color32(150, 90, 50, 255)
            : (x == 7 || x == 8) && y >= 10 && y <= 13 && (y % 2 == 0) ? new Color32(200, 200, 210, 180)
            : x >= 4 && x <= 11 && y >= 1 && y <= 2 ? new Color32(90, 60, 40, 255) : (Color32?)null);
        Sprite iconMagnet = PaintSprite("Icon_Magnet", 16, 16, 16f, (x, y) =>
            (x - 7.5f) * (x - 7.5f) + (y - 6.5f) * (y - 6.5f) <= 30f ? new Color32(250, 245, 235, 255)
            : (x - 4.5f) * (x - 4.5f) + (y - 12f) * (y - 12f) <= 4f || (x - 10.5f) * (x - 10.5f) + (y - 12f) * (y - 12f) <= 4f ? new Color32(250, 245, 235, 255)
            : (Color32?)null);
        Sprite iconHeal = PaintSprite("Icon_Heal", 16, 16, 16f, (x, y) =>
            x >= 4 && x <= 11 && y >= 2 && y <= 10 ? (y >= 8 ? new Color32(120, 200, 90, 255) : new Color32(90, 150, 70, 255)) : (Color32?)null);

        // ---- Habilidades ----
        var shuriken = GetOrCreateAsset<ProjectileAbilityDefinition>(upgradesDir + "/Ability_Shuriken.asset");
        shuriken.displayName = "Shuriken";
        shuriken.icon = shurikenSprite;
        shuriken.projectileSprite = shurikenSprite;
        if (shuriken.levels == null || shuriken.levels.Length == 0)
        {
            shuriken.levels = new[]
            {
                new ProjectileAbilityDefinition.Level { description = "Lança uma shuriken no inimigo mais próximo.", damage = 10, cooldown = 0.6f, projectiles = 1, pierce = 1 },
                new ProjectileAbilityDefinition.Level { description = "+1 shuriken por disparo.", damage = 12, cooldown = 0.6f, projectiles = 2, pierce = 1 },
                new ProjectileAbilityDefinition.Level { description = "+25% de dano. Atravessa +1 inimigo.", damage = 15, cooldown = 0.55f, projectiles = 2, pierce = 2 },
                new ProjectileAbilityDefinition.Level { description = "+1 shuriken. Recarga menor.", damage = 15, cooldown = 0.5f, projectiles = 3, pierce = 2 },
                new ProjectileAbilityDefinition.Level { description = "+1 shuriken, +dano e atravessa +1.", damage = 20, cooldown = 0.45f, projectiles = 4, pierce = 3 },
            };
        }
        EditorUtility.SetDirty(shuriken);

        var katana = GetOrCreateAsset<AuraAbilityDefinition>(upgradesDir + "/Ability_KatanaSlash.asset");
        katana.displayName = "Corte de Katana";
        katana.icon = katanaIcon;
        katana.effectSprite = ringSprite;
        if (katana.levels == null || katana.levels.Length == 0)
        {
            katana.levels = new[]
            {
                new AuraAbilityDefinition.Level { description = "Corta todos os inimigos ao seu redor.", damage = 12, cooldown = 1.6f, radius = 2.2f },
                new AuraAbilityDefinition.Level { description = "+dano e +alcance.", damage = 15, cooldown = 1.5f, radius = 2.4f },
                new AuraAbilityDefinition.Level { description = "+dano e corta mais rápido.", damage = 18, cooldown = 1.35f, radius = 2.6f },
                new AuraAbilityDefinition.Level { description = "+dano e +alcance.", damage = 22, cooldown = 1.2f, radius = 2.9f },
                new AuraAbilityDefinition.Level { description = "Corte máximo: muito dano e alcance.", damage = 30, cooldown = 1.0f, radius = 3.3f },
            };
        }
        EditorUtility.SetDirty(katana);

        var ofuda = GetOrCreateAsset<OrbitAbilityDefinition>(upgradesDir + "/Ability_Ofuda.asset");
        ofuda.displayName = "Ofuda";
        ofuda.icon = ofudaSprite;
        ofuda.orbiterSprite = ofudaSprite;
        if (ofuda.levels == null || ofuda.levels.Length == 0)
        {
            ofuda.levels = new[]
            {
                new OrbitAbilityDefinition.Level { description = "Dois talismãs giram ao seu redor.", damage = 8, count = 2, radius = 1.8f, rotationSpeed = 180 },
                new OrbitAbilityDefinition.Level { description = "+1 talismã.", damage = 8, count = 3, radius = 1.8f, rotationSpeed = 180 },
                new OrbitAbilityDefinition.Level { description = "+dano e +raio.", damage = 11, count = 3, radius = 2.1f, rotationSpeed = 200 },
                new OrbitAbilityDefinition.Level { description = "+1 talismã. Giram mais rápido.", damage = 11, count = 4, radius = 2.1f, rotationSpeed = 220 },
                new OrbitAbilityDefinition.Level { description = "+1 talismã, +dano e +raio.", damage = 15, count = 5, radius = 2.4f, rotationSpeed = 240 },
            };
        }
        EditorUtility.SetDirty(ofuda);

        // ---- Passivas ----
        var speed = CreatePassive(upgradesDir, "Passive_Geta", "Sandálias Geta", iconSpeed, PassiveStat.MoveSpeed, 0.08f, "+{0} de velocidade.");
        var vitality = CreatePassive(upgradesDir, "Passive_Onigiri", "Onigiri", iconHealth, PassiveStat.MaxHealth, 20f, "+{0} de vida máxima.");
        var might = CreatePassive(upgradesDir, "Passive_Whetstone", "Pedra de Amolar", iconDamage, PassiveStat.Damage, 0.1f, "+{0} de dano.");
        var haste = CreatePassive(upgradesDir, "Passive_Incense", "Incenso", iconCooldown, PassiveStat.Cooldown, 0.08f, "{0} a menos de recarga.");
        var magnet = CreatePassive(upgradesDir, "Passive_ManekiNeko", "Maneki-neko", iconMagnet, PassiveStat.PickupRadius, 0.25f, "+{0} de alcance de coleta.");

        // ---- Configuração da progressão ----
        var settings = GetOrCreateAsset<ProgressionSettings>(Root + "/ScriptableObjects/Progression/ProgressionSettings.asset");
        settings.startingAbility = shuriken;
        settings.upgradePool = new System.Collections.Generic.List<UpgradeDefinition> { shuriken, katana, ofuda, speed, vitality, might, haste, magnet };
        settings.smallGemSprite = smallGem;
        settings.bigGemSprite = bigGem;
        EditorUtility.SetDirty(settings);

        // XP dos inimigos: o Oni (mais forte) vale 2
        EnemyData oni = AssetDatabase.LoadAssetAtPath<EnemyData>(Root + "/ScriptableObjects/Enemies/Enemy_Oni.asset");
        if (oni != null && oni.xpValue == 1)
        {
            oni.xpValue = 2;
            EditorUtility.SetDirty(oni);
        }

        // ---- Componentes no jogador ----
        PlayerLoadout loadout = player.AddComponent<PlayerLoadout>();
        SetRefs(loadout, ("settings", settings), ("spawner", spawner), ("projectiles", projectiles), ("playerHealth", health));
        PlayerExperience experience = player.AddComponent<PlayerExperience>();
        SetRefs(experience, ("settings", settings));
        SetRefs(player.GetComponent<PlayerMovement>(), ("loadout", loadout));

        GameObject progressionGo = new GameObject("Progression");
        ExperienceGemSystem gemSystem = progressionGo.AddComponent<ExperienceGemSystem>();
        SetRefs(gemSystem, ("settings", settings), ("spawner", spawner), ("experience", experience), ("loadout", loadout));

        // ---- Barra de XP no topo ----
        Sprite pixel = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/UI_Pixel.png");
        GameObject xpBar = new GameObject("ExperienceBar", typeof(RectTransform));
        xpBar.transform.SetParent(canvasGo.transform, false);
        RectTransform xpRt = (RectTransform)xpBar.transform;
        xpRt.anchorMin = new Vector2(0f, 1f);
        xpRt.anchorMax = new Vector2(1f, 1f);
        xpRt.pivot = new Vector2(0.5f, 1f);
        xpRt.offsetMin = new Vector2(0f, -26f);
        xpRt.offsetMax = new Vector2(0f, 0f);
        Image xpBg = xpBar.AddComponent<Image>();
        xpBg.color = new Color(0.05f, 0.04f, 0.08f, 0.85f);
        xpBg.raycastTarget = false;

        GameObject xpFillGo = new GameObject("Fill", typeof(RectTransform));
        xpFillGo.transform.SetParent(xpBar.transform, false);
        RectTransform fillRt = (RectTransform)xpFillGo.transform;
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = new Vector2(3f, 3f);
        fillRt.offsetMax = new Vector2(-3f, -3f);
        Image xpFill = xpFillGo.AddComponent<Image>();
        xpFill.sprite = pixel;
        xpFill.type = Image.Type.Filled;
        xpFill.fillMethod = Image.FillMethod.Horizontal;
        xpFill.fillAmount = 0f;
        xpFill.color = new Color(0.35f, 0.75f, 1f);
        xpFill.raycastTarget = false;

        Text levelText = CreateText(xpBar.transform, "Level", "Nv 1", 34, Color.white, Vector2.zero);
        RectTransform levelRt = (RectTransform)levelText.transform;
        levelRt.anchorMin = new Vector2(1f, 0f);
        levelRt.anchorMax = new Vector2(1f, 0f);
        levelRt.pivot = new Vector2(1f, 1f);
        levelRt.anchoredPosition = new Vector2(-24f, -4f);
        levelRt.sizeDelta = new Vector2(240f, 50f);
        levelText.alignment = TextAnchor.UpperRight;

        ExperienceBar xpBarComp = xpBar.AddComponent<ExperienceBar>();
        SetRefs(xpBarComp, ("experience", experience), ("fill", xpFill), ("levelText", levelText));

        // ---- Tela de level-up ----
        GameObject panel = new GameObject("LevelUpPanel", typeof(RectTransform));
        panel.transform.SetParent(canvasGo.transform, false);
        RectTransform panelRt = (RectTransform)panel.transform;
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        panel.AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f, 0.7f);

        Text title = CreateText(panel.transform, "Title", "Subiu de nível!", 80, new Color(1f, 0.85f, 0.3f), new Vector2(0f, 380f));
        title.fontStyle = FontStyle.Bold;
        CreateText(panel.transform, "Hint", "Toque numa carta ou aperte 1, 2 ou 3", 32, new Color(0.85f, 0.85f, 0.9f), new Vector2(0f, -400f));

        GameObject levelUpGo = new GameObject("LevelUp");
        LevelUpScreen screen = levelUpGo.AddComponent<LevelUpScreen>();
        SerializedObject screenSo = new SerializedObject(screen);
        screenSo.FindProperty("experience").objectReferenceValue = experience;
        screenSo.FindProperty("loadout").objectReferenceValue = loadout;
        screenSo.FindProperty("playerHealth").objectReferenceValue = health;
        screenSo.FindProperty("panel").objectReferenceValue = panel;
        screenSo.FindProperty("healIcon").objectReferenceValue = iconHeal;
        SerializedProperty cardsProp = screenSo.FindProperty("cards");
        cardsProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            GameObject card = new GameObject("Card" + (i + 1), typeof(RectTransform));
            card.transform.SetParent(panel.transform, false);
            RectTransform cardRt = (RectTransform)card.transform;
            cardRt.anchoredPosition = new Vector2((i - 1) * 470f, -10f);
            cardRt.sizeDelta = new Vector2(430f, 580f);
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.12f, 0.09f, 0.14f, 0.97f);
            Outline border = card.AddComponent<Outline>();
            border.effectColor = new Color(0.87f, 0.7f, 0.25f);
            border.effectDistance = new Vector2(4f, -4f);
            Button button = card.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.95f, 0.8f);
            colors.pressedColor = new Color(1f, 0.85f, 0.5f);
            button.colors = colors;

            GameObject iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(card.transform, false);
            RectTransform iconRt = (RectTransform)iconGo.transform;
            iconRt.anchoredPosition = new Vector2(0f, 170f);
            iconRt.sizeDelta = new Vector2(150f, 150f);
            Image icon = iconGo.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            Text key = CreateText(card.transform, "Key", (i + 1).ToString(), 34, new Color(0.87f, 0.7f, 0.25f), new Vector2(-175f, 255f));
            ((RectTransform)key.transform).sizeDelta = new Vector2(60f, 60f);
            Text nameText = CreateText(card.transform, "Name", "Nome", 44, Color.white, new Vector2(0f, 40f));
            ((RectTransform)nameText.transform).sizeDelta = new Vector2(410f, 70f);
            nameText.fontStyle = FontStyle.Bold;
            Text lvlText = CreateText(card.transform, "Level", "Nova!", 30, new Color(1f, 0.82f, 0.3f), new Vector2(0f, -20f));
            ((RectTransform)lvlText.transform).sizeDelta = new Vector2(410f, 50f);
            Text descText = CreateText(card.transform, "Description", "Descrição", 32, new Color(0.9f, 0.9f, 0.92f), new Vector2(0f, -150f));
            ((RectTransform)descText.transform).sizeDelta = new Vector2(380f, 200f);
            descText.alignment = TextAnchor.UpperCenter;

            SerializedProperty cardProp = cardsProp.GetArrayElementAtIndex(i);
            cardProp.FindPropertyRelative("button").objectReferenceValue = button;
            cardProp.FindPropertyRelative("icon").objectReferenceValue = icon;
            cardProp.FindPropertyRelative("nameText").objectReferenceValue = nameText;
            cardProp.FindPropertyRelative("levelText").objectReferenceValue = lvlText;
            cardProp.FindPropertyRelative("descriptionText").objectReferenceValue = descText;
        }
        screenSo.ApplyModifiedPropertiesWithoutUndo();
        return gemSystem;
    }

    // Zonas (etapa 5): elites, guardiões, 3 zonas e HUD da zona
    private static void CreateZones(HordeSpawner spawner, ExperienceGemSystem gems, PlayerHealth health, GameObject canvasGo)
    {
        string enemiesDir = Root + "/ScriptableObjects/Enemies";
        string worldDir = Root + "/ScriptableObjects/World";
        EnemyData oni = AssetDatabase.LoadAssetAtPath<EnemyData>(enemiesDir + "/Enemy_Oni.asset");
        EnemyData flame = AssetDatabase.LoadAssetAtPath<EnemyData>(enemiesDir + "/Enemy_Hitodama.asset");

        // Elites e guardiões: mesmos sprites provisórios, maiores e com outra cor
        EnemyData redOni = CreateSpecialEnemy(enemiesDir + "/Elite_RedOni.asset", "Oni Vermelho", oni.sprite, EnemyRank.Elite,
            1.8f, new Color(1f, 0.45f, 0.35f), 2.3f, 220f, 18f, 15, 0.6f);
        // Sprite próprio dourado (pintar o sprite azul de amarelo deixaria verde)
        Sprite goldFlameSprite = CreateEnemySprite("Enemy_HitodamaGold", new Color32(235, 180, 40, 255), true);
        EnemyData goldFlame = CreateSpecialEnemy(enemiesDir + "/Elite_GoldHitodama.asset", "Hitodama Dourada", goldFlameSprite, EnemyRank.Elite,
            1.6f, Color.white, 3.4f, 140f, 14f, 12, 0.7f);
        goldFlame.tint = Color.white;
        EnemyData bigOni = CreateSpecialEnemy(enemiesDir + "/Guardian_OOni.asset", "Ō-Oni", oni.sprite, EnemyRank.Guardian,
            3.2f, new Color(0.75f, 0.25f, 0.3f), 2.1f, 2400f, 30f, 60, 0.6f);
        EnemyData flameKing = CreateSpecialEnemy(enemiesDir + "/Guardian_OnibiO.asset", "Onibi-Ō", flame.sprite, EnemyRank.Guardian,
            2.9f, new Color(0.75f, 0.45f, 1f), 2.8f, 1800f, 25f, 60, 0.7f);

        // Hordas das zonas 2 e 3: cópias da zona 1 com mais inimigos por segundo
        HordeSettings horde1 = AssetDatabase.LoadAssetAtPath<HordeSettings>(worldDir + "/HordeSettings.asset");
        HordeSettings horde2 = CreateScaledHorde(horde1, worldDir + "/HordeSettings_Zone2.asset", 1.5f, 10);
        HordeSettings horde3 = CreateScaledHorde(horde1, worldDir + "/HordeSettings_Zone3.asset", 2f, 14);

        var zoneList = new System.Collections.Generic.List<ZoneData>
        {
            CreateZone(worldDir + "/Zone_1.asset", "Bosque de Bambu", horde1, 1f, new[] { redOni, goldFlame }, new[] { bigOni, flameKing }),
            CreateZone(worldDir + "/Zone_2.asset", "Trilha dos Lampiões", horde2, 1.6f, new[] { redOni, goldFlame }, new[] { bigOni, flameKing }),
            CreateZone(worldDir + "/Zone_3.asset", "Portão do Templo", horde3, 2.4f, new[] { redOni, goldFlame }, new[] { bigOni, flameKing }),
        };

        GameObject zoneGo = new GameObject("Zones");
        ZoneController controller = zoneGo.AddComponent<ZoneController>();
        SerializedObject so = new SerializedObject(controller);
        RegionData region = CreateRegion(worldDir, zoneList);
        so.FindProperty("region").objectReferenceValue = region;
        so.FindProperty("spawner").objectReferenceValue = spawner;
        so.FindProperty("gems").objectReferenceValue = gems;
        so.FindProperty("playerHealth").objectReferenceValue = health;
        so.ApplyModifiedPropertiesWithoutUndo();

        // ---- HUD da zona ----
        Sprite pixel = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/UI_Pixel.png");
        Text timer = CreateText(canvasGo.transform, "ZoneTimer", "Zona 1/3   10:00", 46, Color.white, Vector2.zero);
        RectTransform timerRt = (RectTransform)timer.transform;
        timerRt.anchorMin = timerRt.anchorMax = new Vector2(0.5f, 1f);
        timerRt.pivot = new Vector2(0.5f, 1f);
        timerRt.anchoredPosition = new Vector2(0f, -34f);
        timerRt.sizeDelta = new Vector2(900f, 70f);
        timer.fontStyle = FontStyle.Bold;

        // Botão "Chamar guardião" no canto superior direito
        GameObject buttonGo = new GameObject("CallGuardianButton", typeof(RectTransform));
        buttonGo.transform.SetParent(canvasGo.transform, false);
        RectTransform buttonRt = (RectTransform)buttonGo.transform;
        buttonRt.anchorMin = buttonRt.anchorMax = new Vector2(1f, 1f);
        buttonRt.pivot = new Vector2(1f, 1f);
        buttonRt.anchoredPosition = new Vector2(-24f, -90f);
        buttonRt.sizeDelta = new Vector2(360f, 80f);
        Image buttonBg = buttonGo.AddComponent<Image>();
        buttonBg.color = new Color(0.55f, 0.1f, 0.1f, 0.9f);
        Outline buttonBorder = buttonGo.AddComponent<Outline>();
        buttonBorder.effectColor = new Color(0.87f, 0.7f, 0.25f);
        buttonBorder.effectDistance = new Vector2(3f, -3f);
        Button button = buttonGo.AddComponent<Button>();
        Text buttonText = CreateText(buttonGo.transform, "Label", "Chamar guardião (G)", 32, Color.white, Vector2.zero);
        ((RectTransform)buttonText.transform).sizeDelta = new Vector2(360f, 80f);

        // Aviso no meio da tela
        Text banner = CreateText(canvasGo.transform, "ZoneBanner", "", 64, new Color(1f, 0.88f, 0.4f), new Vector2(0f, 220f));
        ((RectTransform)banner.transform).sizeDelta = new Vector2(1700f, 400f);
        banner.fontStyle = FontStyle.Bold;
        banner.supportRichText = true;

        // Barra de vida do guardião (embaixo do timer)
        GameObject gBar = new GameObject("GuardianBar", typeof(RectTransform));
        gBar.transform.SetParent(canvasGo.transform, false);
        RectTransform gRt = (RectTransform)gBar.transform;
        gRt.anchorMin = gRt.anchorMax = new Vector2(0.5f, 1f);
        gRt.pivot = new Vector2(0.5f, 1f);
        gRt.anchoredPosition = new Vector2(0f, -150f);
        gRt.sizeDelta = new Vector2(900f, 34f);
        Image gBg = gBar.AddComponent<Image>();
        gBg.color = new Color(0.05f, 0.03f, 0.05f, 0.9f);
        GameObject gFillGo = new GameObject("Fill", typeof(RectTransform));
        gFillGo.transform.SetParent(gBar.transform, false);
        RectTransform gFillRt = (RectTransform)gFillGo.transform;
        gFillRt.anchorMin = Vector2.zero;
        gFillRt.anchorMax = Vector2.one;
        gFillRt.offsetMin = new Vector2(4f, 4f);
        gFillRt.offsetMax = new Vector2(-4f, -4f);
        Image gFill = gFillGo.AddComponent<Image>();
        gFill.sprite = pixel;
        gFill.type = Image.Type.Filled;
        gFill.fillMethod = Image.FillMethod.Horizontal;
        gFill.color = new Color(0.85f, 0.15f, 0.15f);
        Text gName = CreateText(gBar.transform, "Name", "Guardião", 34, Color.white, new Vector2(0f, 40f));
        ((RectTransform)gName.transform).sizeDelta = new Vector2(900f, 50f);

        SanctuaryController sanctuary = CreateSanctuary(controller, spawner, health, canvasGo);

        ZoneHUD hud = zoneGo.AddComponent<ZoneHUD>();
        SetRefs(hud, ("zones", controller), ("sanctuary", sanctuary), ("timerText", timer), ("callGuardianButton", button), ("bannerText", banner),
            ("guardianBar", gBar), ("guardianFill", gFill), ("guardianName", gName));
    }

    // HUD completo (etapa 7): perfil, moedas, ativa/ultimate, minimapa e layout da seção 6
    private static void CreateFullHud(GameObject player, SpriteRenderer playerSprite, HordeSpawner spawner, PlayerHealth health, GameObject canvasGo)
    {
        const int minimapLayer = 8;
        EnsureLayer(minimapLayer, "Minimap");

        Transform canvas = canvasGo.transform;
        ZoneController zones = Object.FindFirstObjectByType<ZoneController>();
        SanctuaryController sanctuary = Object.FindFirstObjectByType<SanctuaryController>();
        RunClock clock = Object.FindFirstObjectByType<RunClock>();
        PlayerLoadout loadout = player.GetComponent<PlayerLoadout>();
        PlayerExperience experience = player.GetComponent<PlayerExperience>();
        Sprite circle = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/UI_Circle.png");
        Sprite ring = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/FX_Ring.png");
        Color gold = new Color(0.87f, 0.7f, 0.25f);
        Color panelColor = new Color(0.07f, 0.05f, 0.09f, 1f); // opaco: a borda (Outline) não vaza por baixo

        // ---- Moedas: valores das elites, guardiões e boss ----
        foreach (string guid in AssetDatabase.FindAssets("t:EnemyData", new[] { Root + "/ScriptableObjects/Enemies" }))
        {
            EnemyData e = AssetDatabase.LoadAssetAtPath<EnemyData>(AssetDatabase.GUIDToAssetPath(guid));
            if (e.rank == EnemyRank.Common || !Mathf.Approximately(e.coinChance, 0.05f))
            {
                continue; // comuns e valores já ajustados ficam como estão
            }
            e.coinChance = 1f;
            e.coinValue = e.rank == EnemyRank.Elite ? 5 : e.rank == EnemyRank.Guardian ? 25 : 100;
            EditorUtility.SetDirty(e);
        }

        GameObject runGo = new GameObject("RunResults");
        RunResults results = runGo.AddComponent<RunResults>();
        SetRefs(results, ("spawner", spawner), ("zones", zones), ("sanctuary", sanctuary), ("playerHealth", health), ("clock", clock));

        // Coleções do menu: registra criaturas e habilidades encontradas
        DiscoveryTracker tracker = runGo.AddComponent<DiscoveryTracker>();
        SetRefs(tracker, ("spawner", spawner), ("loadout", loadout), ("playerHealth", health), ("sanctuary", sanctuary));

        // Botões "Jogar de novo" e "Menu" nas telas de derrota e vitória
        GameOverScreen gameOverScreen = Object.FindFirstObjectByType<GameOverScreen>();
        VictoryScreen victoryScreen = Object.FindFirstObjectByType<VictoryScreen>();
        CreateResultButtons(canvas.Find("GameOverPanel"), gameOverScreen.Restart);
        CreateResultButtons(canvas.Find("VictoryPanel"), victoryScreen.Restart);
        Transform oldHint = canvas.Find("VictoryPanel/Hint");
        if (oldHint != null) Object.DestroyImmediate(oldHint.gameObject);

        // ---- Ativa (R) e ultimate (C) ----
        Directory.CreateDirectory(Root + "/ScriptableObjects/Skills");
        Sprite dashIcon = PaintSprite("Icon_Dash", 16, 16, 16f, (x, y) =>
            (y >= 6 && y <= 9 && x >= 1 && x <= 9) ? new Color32(200, 220, 255, 255)
            : (Mathf.Abs(y - 7.5f) <= 14 - x - 0.5f && x >= 9) ? new Color32(120, 170, 255, 255) : (Color32?)null);
        Sprite ultIcon = PaintSprite("Icon_Ultimate", 16, 16, 16f, (x, y) =>
        {
            float dx = x - 7.5f, dy = y - 7.5f;
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            float a = Mathf.Atan2(dy, dx);
            float petal = 5.5f + 2.5f * Mathf.Cos(a * 5f);
            if (r <= 2f) return new Color32(255, 220, 90, 255);
            return r <= petal ? new Color32(255, 150, 190, 255) : (Color32?)null;
        });
        SkillData dashData = GetOrCreateAsset<SkillData>(Root + "/ScriptableObjects/Skills/Skill_Dash.asset");
        if (dashData.displayName == "Habilidade") dashData.displayName = "Passo das Sombras";
        dashData.icon = dashIcon;
        EditorUtility.SetDirty(dashData);
        SkillData ultData = GetOrCreateAsset<SkillData>(Root + "/ScriptableObjects/Skills/Skill_Ultimate.asset");
        if (ultData.displayName == "Habilidade") ultData.displayName = "Tempestade de Pétalas";
        ultData.icon = ultIcon;
        EditorUtility.SetDirty(ultData);

        // Clarão de tela cheia da ultimate (atrás do resto do HUD)
        GameObject flashGo = new GameObject("UltimateFlash", typeof(RectTransform));
        flashGo.transform.SetParent(canvas, false);
        flashGo.transform.SetSiblingIndex(0);
        RectTransform flashRt = (RectTransform)flashGo.transform;
        flashRt.anchorMin = Vector2.zero;
        flashRt.anchorMax = Vector2.one;
        flashRt.offsetMin = Vector2.zero;
        flashRt.offsetMax = Vector2.zero;
        Image flash = flashGo.AddComponent<Image>();
        flash.color = new Color(1f, 0.85f, 0.95f, 0f);
        flash.raycastTarget = false;
        flash.enabled = false;

        DashSkill dash = player.AddComponent<DashSkill>();
        SetRefs(dash, ("data", dashData), ("controller", player.GetComponent<CharacterController>()), ("health", health), ("playerSprite", playerSprite));
        UltimateSkill ult = player.AddComponent<UltimateSkill>();
        SetRefs(ult, ("data", ultData), ("spawner", spawner), ("health", health), ("loadout", loadout), ("flash", flash));

        // ---- Topo esquerdo: retrato, nome, conta, nível ----
        GameObject infoPanel = CreatePanel(canvas, "PlayerPanel", new Vector2(0f, 1f), new Vector2(24f, -40f), new Vector2(440f, 150f), panelColor, gold);
        GameObject portraitFrame = CreatePanel(infoPanel.transform, "PortraitFrame", new Vector2(0f, 1f), new Vector2(14f, -14f), new Vector2(122f, 122f), new Color(0.2f, 0.12f, 0.12f, 1f), gold);
        Image portrait = CreateImage(portraitFrame.transform, "Portrait", null, new Vector2(100f, 110f));
        portrait.preserveAspect = true;
        Text nameText = CreateText(infoPanel.transform, "Name", "Samurai", 38, Color.white, Vector2.zero);
        Place(nameText, new Vector2(0f, 1f), new Vector2(150f, -18f), new Vector2(280f, 50f), TextAnchor.UpperLeft);
        nameText.fontStyle = FontStyle.Bold;
        Text accountText = CreateText(infoPanel.transform, "Account", "Conta Nv 1", 26, new Color(0.75f, 0.75f, 0.8f), Vector2.zero);
        Place(accountText, new Vector2(0f, 1f), new Vector2(150f, -66f), new Vector2(280f, 36f), TextAnchor.UpperLeft);
        Text runLevel = CreateText(infoPanel.transform, "RunLevel", "Nv 1", 40, new Color(1f, 0.82f, 0.3f), Vector2.zero);
        Place(runLevel, new Vector2(0f, 1f), new Vector2(150f, -98f), new Vector2(280f, 50f), TextAnchor.UpperLeft);
        runLevel.fontStyle = FontStyle.Bold;
        PlayerInfoHUD info = infoPanel.AddComponent<PlayerInfoHUD>();
        SetRefs(info, ("experience", experience), ("portrait", portrait), ("nameText", nameText), ("accountLevelText", accountText),
            ("runLevelText", runLevel), ("portraitSprite", playerSprite.sprite));

        // O nível agora fica no painel; a barra de XP não precisa mais do texto
        Transform xpLevel = canvas.Find("ExperienceBar/Level");
        if (xpLevel != null) xpLevel.gameObject.SetActive(false);

        // Texto de debug (FPS etc.) vai para baixo do painel, menor
        Text debugText = canvas.Find("HordeDebugText").GetComponent<Text>();
        debugText.fontSize = 22;
        ((RectTransform)debugText.transform).anchoredPosition = new Vector2(28f, -200f);
        debugText.color = new Color(1f, 1f, 1f, 0.6f);

        // ---- Topo central: região/zona/dificuldade, timer, números ----
        Text zoneLine = CreateText(canvas, "ZoneLine", "", 32, new Color(0.9f, 0.88f, 0.8f), Vector2.zero);
        Place(zoneLine, new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(1000f, 44f), TextAnchor.UpperCenter);
        Text timer = canvas.Find("ZoneTimer").GetComponent<Text>();
        Place(timer, new Vector2(0.5f, 1f), new Vector2(0f, -76f), new Vector2(600f, 80f), TextAnchor.UpperCenter);
        timer.fontSize = 64;
        Text stats = CreateText(canvas, "RunStats", "", 28, Color.white, Vector2.zero);
        Place(stats, new Vector2(0.5f, 1f), new Vector2(0f, -152f), new Vector2(1100f, 40f), TextAnchor.UpperCenter);
        RunStatsHUD statsHud = runGo.AddComponent<RunStatsHUD>();
        SetRefs(statsHud, ("clock", clock), ("results", results), ("spawner", spawner), ("zones", zones), ("statsText", stats));
        ((RectTransform)canvas.Find("GuardianBar")).anchoredPosition = new Vector2(0f, -236f);
        SetRefs(Object.FindFirstObjectByType<ZoneHUD>(), ("zoneLineText", zoneLine));

        // ---- Topo direito: minimapa redondo ----
        RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(Root + "/Materials/RT_Minimap.renderTexture");
        if (rt == null)
        {
            rt = new RenderTexture(256, 256, 16) { name = "RT_Minimap" };
            AssetDatabase.CreateAsset(rt, Root + "/Materials/RT_Minimap.renderTexture");
        }
        GameObject camGo = new GameObject("MinimapCamera");
        Camera mapCam = camGo.AddComponent<Camera>();
        mapCam.orthographic = true;
        mapCam.orthographicSize = 22f;
        mapCam.clearFlags = CameraClearFlags.SolidColor;
        mapCam.backgroundColor = new Color(0.08f, 0.1f, 0.08f);
        mapCam.cullingMask = (1 << 0) | (1 << minimapLayer);
        mapCam.targetTexture = rt;
        mapCam.nearClipPlane = 1f;
        mapCam.farClipPlane = 100f;
        Camera.main.cullingMask &= ~(1 << minimapLayer); // a câmera do jogo não vê os marcadores

        GameObject playerMarker = new GameObject("MinimapPlayerMarker");
        playerMarker.layer = minimapLayer;
        playerMarker.transform.SetParent(player.transform, false);
        playerMarker.transform.localPosition = new Vector3(0f, 20f, 0f);
        playerMarker.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        playerMarker.transform.localScale = Vector3.one * 2f;
        SpriteRenderer pm = playerMarker.AddComponent<SpriteRenderer>();
        pm.sprite = circle;
        pm.color = Color.white;
        pm.sortingOrder = 210;

        MinimapController minimap = camGo.AddComponent<MinimapController>();
        SetRefs(minimap, ("minimapCamera", mapCam), ("player", player.transform), ("spawner", spawner), ("markerSprite", circle));

        GameObject mapRoot = new GameObject("Minimap", typeof(RectTransform));
        mapRoot.transform.SetParent(canvas, false);
        RectTransform mapRt = (RectTransform)mapRoot.transform;
        mapRt.anchorMin = mapRt.anchorMax = new Vector2(1f, 1f);
        mapRt.pivot = new Vector2(1f, 1f);
        mapRt.anchoredPosition = new Vector2(-24f, -40f);
        mapRt.sizeDelta = new Vector2(270f, 270f);
        Image border = mapRoot.AddComponent<Image>();
        border.sprite = circle;
        border.color = gold;
        border.raycastTarget = false;
        GameObject maskGo = new GameObject("Mask", typeof(RectTransform));
        maskGo.transform.SetParent(mapRoot.transform, false);
        ((RectTransform)maskGo.transform).sizeDelta = new Vector2(254f, 254f);
        Image maskImage = maskGo.AddComponent<Image>();
        maskImage.sprite = circle;
        maskImage.raycastTarget = false;
        maskGo.AddComponent<Mask>().showMaskGraphic = false;
        GameObject rawGo = new GameObject("Map", typeof(RectTransform));
        rawGo.transform.SetParent(maskGo.transform, false);
        ((RectTransform)rawGo.transform).sizeDelta = new Vector2(254f, 254f);
        RawImage raw = rawGo.AddComponent<RawImage>();
        raw.texture = rt;
        raw.raycastTarget = false;

        ((RectTransform)canvas.Find("CallGuardianButton")).anchoredPosition = new Vector2(-24f, -330f);

        // ---- Embaixo: espaços de habilidades (4) e passivas (3) ----
        int abilityCount = 4, passiveCount = 3;
        float slotSize = 100f, spacing = 112f, gap = 44f;
        float totalWidth = (abilityCount + passiveCount) * spacing + gap;
        GameObject slotsRoot = new GameObject("LoadoutSlots", typeof(RectTransform));
        slotsRoot.transform.SetParent(canvas, false);
        RectTransform slotsRt = (RectTransform)slotsRoot.transform;
        slotsRt.anchorMin = slotsRt.anchorMax = new Vector2(0.5f, 0f);
        slotsRt.pivot = new Vector2(0.5f, 0f);
        slotsRt.anchoredPosition = new Vector2(-40f, 24f);
        slotsRt.sizeDelta = new Vector2(totalWidth, slotSize + 30f);

        LoadoutHUD loadoutHud = slotsRoot.AddComponent<LoadoutHUD>();
        SerializedObject lso = new SerializedObject(loadoutHud);
        lso.FindProperty("loadout").objectReferenceValue = loadout;
        SerializedProperty abilityProp = lso.FindProperty("abilitySlots");
        SerializedProperty passiveProp = lso.FindProperty("passiveSlots");
        abilityProp.arraySize = abilityCount;
        passiveProp.arraySize = passiveCount;
        for (int i = 0; i < abilityCount + passiveCount; i++)
        {
            bool isAbility = i < abilityCount;
            float x = -totalWidth * 0.5f + spacing * 0.5f + i * spacing + (isAbility ? 0f : gap);
            Color frame = isAbility ? gold : new Color(0.45f, 0.65f, 0.95f);
            GameObject slot = CreatePanel(slotsRoot.transform, (isAbility ? "Ability" : "Passive") + "Slot" + i,
                new Vector2(0.5f, 0f), new Vector2(x, 28f), new Vector2(slotSize, slotSize), panelColor, frame);
            RectTransform slotRt = (RectTransform)slot.transform;
            slotRt.pivot = new Vector2(0.5f, 0f);
            Image icon = CreateImage(slot.transform, "Icon", null, new Vector2(78f, 78f));
            icon.preserveAspect = true;
            Text lvl = CreateText(slot.transform, "Level", "", 22, Color.white, new Vector2(0f, -64f));
            ((RectTransform)lvl.transform).sizeDelta = new Vector2(110f, 30f);
            SerializedProperty el = (isAbility ? abilityProp : passiveProp).GetArrayElementAtIndex(isAbility ? i : i - abilityCount);
            el.FindPropertyRelative("icon").objectReferenceValue = icon;
            el.FindPropertyRelative("levelText").objectReferenceValue = lvl;
        }
        lso.ApplyModifiedPropertiesWithoutUndo();

        // ---- Embaixo à direita: botões da ativa (R) e da ultimate (C) ----
        CreateSkillButton(canvas, "UltimateButton", "C", ult, circle, ring, new Vector2(-40f, 40f), 170f, new Color(1f, 0.6f, 0.8f));
        CreateSkillButton(canvas, "ActiveButton", "R", dash, circle, ring, new Vector2(-240f, 40f), 140f, new Color(0.6f, 0.8f, 1f));

        CreateFusionAndMap(player, health, zones, sanctuary, minimap, canvas, mapRoot, border);

        // Painéis de level-up, derrota e vitória ficam por cima do HUD novo
        canvas.Find("LevelUpPanel").SetAsLastSibling();
        canvas.Find("VictoryPanel").SetAsLastSibling();
    }

    private static void CreateResultButtons(Transform panel, UnityEngine.Events.UnityAction restart)
    {
        Button again = CreateMenuButton(panel, "PlayAgainButton", "Jogar de novo (R)", new Vector2(-250f, -340f), new Vector2(440f, 100f),
            new Color(0.55f, 0.1f, 0.1f, 1f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(again.onClick, restart);
        Button menu = CreateMenuButton(panel, "MenuButton", "Menu (Esc)", new Vector2(250f, -340f), new Vector2(440f, 100f),
            new Color(0.12f, 0.09f, 0.14f, 1f));
        ReturnToMenu back = menu.gameObject.AddComponent<ReturnToMenu>();
        SetRefs(back, ("button", menu));
    }

    // Botão retangular com texto e borda dourada
    private static Button CreateMenuButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        Image bg = go.AddComponent<Image>();
        bg.color = color;
        Outline border = go.AddComponent<Outline>();
        border.effectColor = new Color(0.87f, 0.7f, 0.25f);
        border.effectDistance = new Vector2(3f, -3f);
        Button button = go.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.9f, 0.75f);
        colors.pressedColor = new Color(1f, 0.8f, 0.5f);
        button.colors = colors;
        Text text = CreateText(go.transform, "Label", label, 40, Color.white, Vector2.zero);
        ((RectTransform)text.transform).sizeDelta = size;
        text.fontStyle = FontStyle.Bold;
        return button;
    }

    // ---------------- Etapa 8: fusão no altar e mapa (M) ----------------

    private static void CreateFusionAndMap(GameObject player, PlayerHealth health, ZoneController zones, SanctuaryController sanctuary,
        MinimapController minimap, Transform canvas, GameObject minimapRoot, Image minimapBorder)
    {
        const int minimapLayer = 8;
        Color cyan = new Color(0.4f, 0.95f, 1f);
        Color gold = new Color(0.87f, 0.7f, 0.25f);
        string upgradesDir = Root + "/ScriptableObjects/Upgrades";

        // ---- Receita: Shuriken + Ofuda = Shuriken Selada ----
        Sprite sealedSprite = PaintSprite("Shuriken_Sealed", 15, 15, 18f, (x, y) =>
        {
            int dx = Mathf.Abs(x - 7), dy = Mathf.Abs(y - 7);
            bool blade = (dx <= 1 && dy <= 7 - dx * 3) || (dy <= 1 && dx <= 7 - dy * 3) || (dx + dy <= 4);
            if (!blade) return null;
            if (dx + dy <= 2) return new Color32(210, 30, 30, 255); // selo vermelho no centro
            return dx + dy <= 4 ? new Color32(245, 235, 200, 255) : new Color32(200, 206, 220, 255);
        });
        var result = GetOrCreateAsset<ProjectileAbilityDefinition>(upgradesDir + "/Ability_SealedShuriken.asset");
        result.displayName = "Shuriken Selada";
        result.icon = sealedSprite;
        result.projectileSprite = sealedSprite;
        if (result.levels == null || result.levels.Length == 0)
        {
            result.targetRange = 16f;
            result.spreadAngle = 14f;
            result.projectileSpeed = 16f;
            result.levels = new[]
            {
                new ProjectileAbilityDefinition.Level { description = "Seis shurikens seladas por disparo: fortes e atravessam quase tudo.",
                    damage = 32, cooldown = 0.4f, projectiles = 6, pierce = 6 },
            };
        }
        EditorUtility.SetDirty(result);

        var shuriken = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(upgradesDir + "/Ability_Shuriken.asset");
        var ofuda = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(upgradesDir + "/Ability_Ofuda.asset");
        var recipe = GetOrCreateAsset<FusionRecipe>(Root + "/ScriptableObjects/Upgrades/Fusion_ShurikenOfuda.asset");
        recipe.ingredientA = shuriken;
        recipe.ingredientB = ofuda;
        recipe.result = result;
        if (string.IsNullOrEmpty(recipe.description))
        {
            recipe.description = "O selo de uma ofuda torna a shuriken implacável.";
        }
        EditorUtility.SetDirty(recipe);

        PlayerLoadout loadout = player.GetComponent<PlayerLoadout>();
        ProgressionSettings progression = AssetDatabase.LoadAssetAtPath<ProgressionSettings>(Root + "/ScriptableObjects/Progression/ProgressionSettings.asset");
        if (!progression.fusionRecipes.Contains(recipe))
        {
            progression.fusionRecipes.Add(recipe);
            EditorUtility.SetDirty(progression);
        }

        // ---- Altar de Fusão (um objeto, reposicionado a cada zona) ----
        GameObject altarGo = new GameObject("FusionAltar");
        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pedestal.name = "Pedestal";
        pedestal.transform.SetParent(altarGo.transform, false);
        pedestal.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        pedestal.transform.localScale = new Vector3(1.6f, 0.8f, 1.6f);
        pedestal.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/M_Block.mat");

        Sprite flame = CreateEnemySprite("FX_AltarFlame", new Color32(90, 230, 255, 255), true);
        SpriteRenderer glow = new GameObject("Glow").AddComponent<SpriteRenderer>();
        glow.transform.SetParent(altarGo.transform, false);
        glow.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        glow.transform.localScale = Vector3.one * 1.4f;
        glow.sprite = flame;
        glow.sortingOrder = 20;
        glow.gameObject.AddComponent<Billboard>();

        SpriteRenderer ring = new GameObject("Ring").AddComponent<SpriteRenderer>();
        ring.transform.SetParent(altarGo.transform, false);
        ring.transform.localPosition = new Vector3(0f, 0.06f, 0f);
        ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        ring.transform.localScale = Vector3.one * 2.4f; // anel de ~4,8 m
        ring.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/FX_Ring.png");
        ring.sortingOrder = 15;

        FusionAltar altar = altarGo.AddComponent<FusionAltar>();
        SetRefs(altar, ("player", player.transform), ("zones", zones), ("sanctuary", sanctuary), ("minimap", minimap),
            ("glow", glow), ("ring", ring));

        // ---- Tela de fusão ----
        GameObject panel = new GameObject("FusionPanel", typeof(RectTransform));
        panel.transform.SetParent(canvas, false);
        Stretch(panel, canvas);
        panel.AddComponent<Image>().color = new Color(0.02f, 0.04f, 0.06f, 0.95f);
        Text title = CreateText(panel.transform, "Title", "Altar de Fusão", 84, cyan, new Vector2(0f, 360f));
        title.fontStyle = FontStyle.Bold;

        GameObject group = new GameObject("Recipe", typeof(RectTransform));
        group.transform.SetParent(panel.transform, false);
        Image iconA = CreateIconBox(group.transform, "IconA", new Vector2(-420f, 140f), gold);
        CreateText(group.transform, "Plus", "+", 110, Color.white, new Vector2(-210f, 140f));
        Image iconB = CreateIconBox(group.transform, "IconB", new Vector2(0f, 140f), gold);
        CreateText(group.transform, "Equals", "=", 110, Color.white, new Vector2(210f, 140f));
        Image iconC = CreateIconBox(group.transform, "IconResult", new Vector2(420f, 140f), cyan);
        Text recipeText = CreateText(group.transform, "RecipeText", "", 44, Color.white, new Vector2(0f, -10f));
        recipeText.fontStyle = FontStyle.Bold;
        Text message = CreateText(panel.transform, "Message", "", 34, new Color(0.88f, 0.92f, 0.95f), new Vector2(0f, -150f));
        ((RectTransform)message.transform).sizeDelta = new Vector2(1300f, 200f);

        Button fuse = CreateMenuButton(panel.transform, "FuseButton", "Fundir (F)", new Vector2(-250f, -350f), new Vector2(440f, 100f),
            new Color(0.05f, 0.4f, 0.5f, 1f));
        Button close = CreateMenuButton(panel.transform, "CloseButton", "Agora não (Esc)", new Vector2(250f, -350f), new Vector2(440f, 100f),
            new Color(0.12f, 0.09f, 0.14f, 1f));

        GameObject fusionGo = new GameObject("FusionScreen");
        FusionScreen fusion = fusionGo.AddComponent<FusionScreen>();
        SetRefs(fusion, ("altar", altar), ("loadout", loadout), ("playerHealth", health), ("panel", panel), ("recipeGroup", group),
            ("iconA", iconA), ("iconB", iconB), ("iconResult", iconC), ("recipeText", recipeText), ("messageText", message),
            ("fuseButton", fuse), ("closeButton", close));

        // ---- Mapa da fase (M) ----
        RenderTexture mapRt = AssetDatabase.LoadAssetAtPath<RenderTexture>(Root + "/Materials/RT_Map.renderTexture");
        if (mapRt == null)
        {
            mapRt = new RenderTexture(768, 768, 16) { name = "RT_Map" };
            AssetDatabase.CreateAsset(mapRt, Root + "/Materials/RT_Map.renderTexture");
        }
        GameObject mapCamGo = new GameObject("MapCamera");
        Camera mapCam = mapCamGo.AddComponent<Camera>();
        mapCam.orthographic = true;
        mapCam.orthographicSize = 51f;
        mapCam.clearFlags = CameraClearFlags.SolidColor;
        mapCam.backgroundColor = new Color(0.05f, 0.06f, 0.05f);
        mapCam.cullingMask = (1 << 0) | (1 << minimapLayer);
        mapCam.targetTexture = mapRt;
        mapCam.nearClipPlane = 1f;
        mapCam.farClipPlane = 120f;
        mapCam.enabled = false;

        GameObject mapPanel = new GameObject("MapPanel", typeof(RectTransform));
        mapPanel.transform.SetParent(canvas, false);
        Stretch(mapPanel, canvas);
        mapPanel.AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.04f, 0.96f);
        Text mapTitle = CreateText(mapPanel.transform, "Title", "Mapa  ·  " + AssetDatabase.LoadAssetAtPath<RegionData>(Root + "/ScriptableObjects/World/Region_BambooForest.asset").displayName,
            54, gold, new Vector2(0f, 480f));
        mapTitle.fontStyle = FontStyle.Bold;
        GameObject frame = CreatePanel(mapPanel.transform, "Frame", new Vector2(0.5f, 0.5f), new Vector2(-150f, -20f), new Vector2(860f, 860f),
            new Color(0.07f, 0.05f, 0.09f, 1f), gold);
        ((RectTransform)frame.transform).pivot = new Vector2(0.5f, 0.5f);
        GameObject rawGo = new GameObject("Map", typeof(RectTransform));
        rawGo.transform.SetParent(frame.transform, false);
        ((RectTransform)rawGo.transform).sizeDelta = new Vector2(840f, 840f);
        RawImage raw = rawGo.AddComponent<RawImage>();
        raw.texture = mapRt;
        raw.raycastTarget = false;

        // Legenda
        (string label, Color color)[] legend =
        {
            ("Você", Color.white),
            ("Altar de Fusão", cyan),
            ("Elite", new Color(1f, 0.75f, 0.2f)),
            ("Guardião", new Color(1f, 0.25f, 0.25f)),
            ("Chefe", new Color(0.8f, 0.2f, 1f)),
        };
        Sprite circle = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/UI_Circle.png");
        for (int i = 0; i < legend.Length; i++)
        {
            float y = 200f - i * 70f;
            Image dot = CreateImage(mapPanel.transform, "Dot" + i, circle, new Vector2(34f, 34f));
            dot.color = legend[i].color;
            ((RectTransform)dot.transform).anchoredPosition = new Vector2(370f, y);
            Text t = CreateText(mapPanel.transform, "Legend" + i, legend[i].label, 34, Color.white, new Vector2(560f, y));
            ((RectTransform)t.transform).sizeDelta = new Vector2(320f, 50f);
            t.alignment = TextAnchor.MiddleLeft;
        }
        Button mapClose = CreateMenuButton(mapPanel.transform, "CloseButton", "Fechar (M)", new Vector2(560f, -380f), new Vector2(360f, 90f),
            new Color(0.12f, 0.09f, 0.14f, 1f));

        // Tocar no minimapa abre o mapa (celular)
        minimapBorder.raycastTarget = true;
        Button minimapButton = minimapRoot.AddComponent<Button>();

        MapScreen mapScreen = mapCamGo.AddComponent<MapScreen>();
        SetRefs(mapScreen, ("mapCamera", mapCam), ("panel", mapPanel), ("minimapButton", minimapButton), ("closeButton", mapClose),
            ("playerHealth", health));
    }

    private static Image CreateIconBox(Transform parent, string name, Vector2 position, Color border)
    {
        GameObject box = CreatePanel(parent, name, new Vector2(0.5f, 0.5f), position, new Vector2(170f, 170f),
            new Color(0.08f, 0.06f, 0.1f, 1f), border);
        ((RectTransform)box.transform).pivot = new Vector2(0.5f, 0.5f);
        Image icon = CreateImage(box.transform, "Icon", null, new Vector2(140f, 140f));
        icon.preserveAspect = true;
        return icon;
    }

    // ---------------- Menu principal (antes da etapa 8) ----------------

    private const string MenuScenePath = Root + "/Scenes/MainMenu.unity";

    [MenuItem("JogoJapones/Setup/Recriar menu principal")]
    public static void CreateMainMenuScene()
    {
        GameCatalog catalog = CreateCatalog();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.04f, 0.07f);

        GameObject canvasGo = new GameObject("Menu Canvas");
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
        Transform root = canvasGo.transform;

        Color gold = new Color(0.87f, 0.7f, 0.25f);
        Color red = new Color(0.72f, 0.12f, 0.1f);

        // ---- Painel principal ----
        GameObject main = Stretch(new GameObject("MainPanel", typeof(RectTransform)), root);
        // Torii decorativo (retângulos vermelhos) atrás do título
        AddRect(main.transform, "ToriiTop", new Vector2(0f, 420f), new Vector2(1100f, 46f), red);
        AddRect(main.transform, "ToriiBeam", new Vector2(0f, 340f), new Vector2(900f, 28f), red);
        AddRect(main.transform, "ToriiLeft", new Vector2(-360f, 120f), new Vector2(46f, 600f), red);
        AddRect(main.transform, "ToriiRight", new Vector2(360f, 120f), new Vector2(46f, 600f), red);

        Text title = CreateText(main.transform, "Title", "JOGO SAMURAI", 120, gold, new Vector2(0f, 200f));
        title.fontStyle = FontStyle.Bold;
        CreateText(main.transform, "Subtitle", "protótipo", 34, new Color(0.8f, 0.78f, 0.75f), new Vector2(0f, 110f));

        Button play = CreateMenuButton(main.transform, "PlayButton", "Jogar", new Vector2(0f, -10f), new Vector2(560f, 104f), red);
        Button creatures = CreateMenuButton(main.transform, "CreaturesButton", "Manual de Criaturas", new Vector2(0f, -125f), new Vector2(560f, 92f),
            new Color(0.12f, 0.09f, 0.14f, 1f));
        Button upgrades = CreateMenuButton(main.transform, "UpgradesButton", "Habilidades", new Vector2(0f, -232f), new Vector2(560f, 92f),
            new Color(0.12f, 0.09f, 0.14f, 1f));
        Button recipes = CreateMenuButton(main.transform, "RecipesButton", "Fusões", new Vector2(0f, -339f), new Vector2(560f, 92f),
            new Color(0.12f, 0.09f, 0.14f, 1f));
        Text profile = CreateText(main.transform, "Profile", "", 30, new Color(0.75f, 0.75f, 0.8f), new Vector2(0f, -460f));

        // Sprites decorativos dos lados
        AddSprite(main.transform, "DecoPlayer", AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/Player_Placeholder.png"), new Vector2(-640f, -230f), 260f);
        AddSprite(main.transform, "DecoOni", AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/Enemy_Oni.png"), new Vector2(640f, -250f), 200f);
        AddSprite(main.transform, "DecoFlame", AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/Enemy_Hitodama.png"), new Vector2(800f, -120f), 130f);

        // ---- Páginas de coleção ----
        CollectionPage creaturesPage = CreateCollectionPage(root, "CreaturesPage", "Manual de Criaturas", CollectionPage.Mode.Creatures, catalog);
        CollectionPage upgradesPage = CreateCollectionPage(root, "UpgradesPage", "Habilidades", CollectionPage.Mode.Upgrades, catalog);
        CollectionPage recipesPage = CreateCollectionPage(root, "RecipesPage", "Fusões", CollectionPage.Mode.Recipes, catalog);

        MainMenu menu = canvasGo.AddComponent<MainMenu>();
        SetRefs(menu, ("mainPanel", main), ("creaturesPage", creaturesPage), ("upgradesPage", upgradesPage),
            ("playButton", play), ("creaturesButton", creatures), ("upgradesButton", upgrades), ("profileText", profile),
            ("recipesPage", recipesPage), ("recipesButton", recipes));

        EditorSceneManager.SaveScene(scene, MenuScenePath);
        // Menu é a primeira cena do build; o jogo vem depois
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(ScenePath, true)
        };
    }

    private static CollectionPage CreateCollectionPage(Transform root, string name, string titleText, CollectionPage.Mode mode, GameCatalog catalog)
    {
        Color gold = new Color(0.87f, 0.7f, 0.25f);
        Color panel = new Color(0.1f, 0.07f, 0.12f, 1f);
        GameObject page = Stretch(new GameObject(name, typeof(RectTransform)), root);

        Text title = CreateText(page.transform, "Title", titleText, 64, gold, new Vector2(0f, 470f));
        title.fontStyle = FontStyle.Bold;
        Text progress = CreateText(page.transform, "Progress", "", 34, Color.white, new Vector2(620f, 470f));
        ((RectTransform)progress.transform).sizeDelta = new Vector2(500f, 60f);
        Button back = CreateMenuButton(page.transform, "BackButton", "Voltar (Esc)", new Vector2(-720f, 470f), new Vector2(340f, 80f),
            new Color(0.12f, 0.09f, 0.14f, 1f));

        // Grade com rolagem (esquerda)
        GameObject scroll = new GameObject("Grid", typeof(RectTransform));
        scroll.transform.SetParent(page.transform, false);
        RectTransform scrollRt = (RectTransform)scroll.transform;
        scrollRt.anchoredPosition = new Vector2(-380f, -40f);
        scrollRt.sizeDelta = new Vector2(1060f, 860f);
        scroll.AddComponent<Image>().color = panel;
        scroll.AddComponent<RectMask2D>();
        ScrollRect scrollRect = scroll.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 40f;

        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(scroll.transform, false);
        RectTransform contentRt = (RectTransform)content.transform;
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.sizeDelta = new Vector2(0f, 0f);
        GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(190f, 220f);
        grid.spacing = new Vector2(16f, 16f);
        grid.padding = new RectOffset(24, 24, 24, 24);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRt;
        scrollRect.viewport = scrollRt;

        // Modelo de item da grade
        GameObject entry = new GameObject("EntryTemplate", typeof(RectTransform));
        entry.transform.SetParent(content.transform, false);
        Image entryBg = entry.AddComponent<Image>();
        entryBg.color = new Color(0.16f, 0.12f, 0.18f, 1f);
        Outline entryBorder = entry.AddComponent<Outline>();
        entryBorder.effectColor = gold;
        entryBorder.effectDistance = new Vector2(2f, -2f);
        entry.AddComponent<Button>();
        GameObject iconGo = new GameObject("Icon", typeof(RectTransform));
        iconGo.transform.SetParent(entry.transform, false);
        ((RectTransform)iconGo.transform).anchoredPosition = new Vector2(0f, 22f);
        ((RectTransform)iconGo.transform).sizeDelta = new Vector2(130f, 130f);
        Image icon = iconGo.AddComponent<Image>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        Text entryName = CreateText(entry.transform, "Name", "???", 24, Color.white, new Vector2(0f, -82f));
        ((RectTransform)entryName.transform).sizeDelta = new Vector2(180f, 56f);
        // Nomes longos ("Hitodama Dourada") diminuem a fonte em vez de cortar
        entryName.resizeTextForBestFit = true;
        entryName.resizeTextMinSize = 14;
        entryName.resizeTextMaxSize = 24;

        // Detalhes (direita)
        GameObject detail = new GameObject("Details", typeof(RectTransform));
        detail.transform.SetParent(page.transform, false);
        RectTransform detailRt = (RectTransform)detail.transform;
        detailRt.anchoredPosition = new Vector2(560f, -40f);
        detailRt.sizeDelta = new Vector2(720f, 860f);
        detail.AddComponent<Image>().color = panel;
        detail.AddComponent<Outline>().effectColor = gold;
        GameObject dIconGo = new GameObject("Icon", typeof(RectTransform));
        dIconGo.transform.SetParent(detail.transform, false);
        ((RectTransform)dIconGo.transform).anchoredPosition = new Vector2(0f, 270f);
        ((RectTransform)dIconGo.transform).sizeDelta = new Vector2(240f, 240f);
        Image dIcon = dIconGo.AddComponent<Image>();
        dIcon.preserveAspect = true;
        dIcon.raycastTarget = false;
        Text dName = CreateText(detail.transform, "Name", "", 54, Color.white, new Vector2(0f, 110f));
        ((RectTransform)dName.transform).sizeDelta = new Vector2(680f, 70f);
        dName.fontStyle = FontStyle.Bold;
        Text dSub = CreateText(detail.transform, "Subtitle", "", 32, gold, new Vector2(0f, 55f));
        ((RectTransform)dSub.transform).sizeDelta = new Vector2(680f, 50f);
        Text dBody = CreateText(detail.transform, "Body", "", 30, new Color(0.9f, 0.9f, 0.92f), new Vector2(0f, -190f));
        ((RectTransform)dBody.transform).sizeDelta = new Vector2(640f, 440f);
        dBody.alignment = TextAnchor.UpperLeft;

        CollectionPage collection = page.AddComponent<CollectionPage>();
        SerializedObject so = new SerializedObject(collection);
        so.FindProperty("mode").enumValueIndex = (int)mode;
        so.ApplyModifiedPropertiesWithoutUndo();
        SetRefs(collection, ("catalog", catalog), ("gridContent", content.transform), ("entryTemplate", entry),
            ("progressText", progress), ("backButton", back), ("detailIcon", dIcon), ("detailName", dName),
            ("detailSubtitle", dSub), ("detailBody", dBody));
        return collection;
    }

    // Catálogo com todas as criaturas e habilidades (ordem das coleções) + descrições do Manual
    private static GameCatalog CreateCatalog()
    {
        string enemies = Root + "/ScriptableObjects/Enemies/";
        (string file, string text)[] creatures =
        {
            ("Enemy_Oni", "Ogro youkai das montanhas. Lento, mas aguenta pancada."),
            ("Enemy_Hitodama", "Chama errante de uma alma perdida. Rápida e frágil."),
            ("Elite_RedOni", "Oni veterano coberto de cicatrizes. Bate forte e demora a cair."),
            ("Elite_GoldHitodama", "Chama rara que brilha como ouro. Corre muito e vale muita experiência."),
            ("Guardian_OOni", "O grande oni que guarda as trilhas da floresta. Um dos guardiões possíveis das zonas."),
            ("Guardian_OnibiO", "Rei das chamas fantasmas. Arde sem parar. Um dos guardiões possíveis das zonas."),
            ("Boss_KageOni", "Senhor das sombras do templo e chefe da Floresta de Bambu. Solta ondas de choque e fica furioso quando ferido."),
        };

        GameCatalog catalog = GetOrCreateAsset<GameCatalog>(Root + "/ScriptableObjects/GameCatalog.asset");
        catalog.creatures = new System.Collections.Generic.List<EnemyData>();
        foreach (var c in creatures)
        {
            EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(enemies + c.file + ".asset");
            if (data == null) continue;
            if (string.IsNullOrEmpty(data.description))
            {
                data.description = c.text;
                EditorUtility.SetDirty(data);
            }
            catalog.creatures.Add(data);
        }

        ProgressionSettings progression = AssetDatabase.LoadAssetAtPath<ProgressionSettings>(Root + "/ScriptableObjects/Progression/ProgressionSettings.asset");
        catalog.upgrades = new System.Collections.Generic.List<UpgradeDefinition>(progression.upgradePool);
        catalog.recipes = new System.Collections.Generic.List<FusionRecipe>(progression.fusionRecipes);
        foreach (FusionRecipe r in progression.fusionRecipes)
        {
            if (r != null && r.result != null && !catalog.upgrades.Contains(r.result))
            {
                catalog.upgrades.Add(r.result); // habilidades de fusão também aparecem em Habilidades
            }
        }
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    private static GameObject Stretch(GameObject go, Transform parent)
    {
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    private static void AddRect(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        Image image = go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private static void AddSprite(Transform parent, string name, Sprite sprite, Vector2 position, float size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(size, size);
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private static void CreateSkillButton(Transform canvas, string name, string key, PlayerSkill skill, Sprite circle, Sprite ring,
        Vector2 position, float size, Color accent)
    {
        GameObject root = new GameObject(name, typeof(RectTransform));
        root.transform.SetParent(canvas, false);
        RectTransform rt = (RectTransform)root.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(size, size);
        Image bg = root.AddComponent<Image>();
        bg.sprite = circle;
        bg.color = new Color(0.07f, 0.05f, 0.09f, 0.85f);
        Button button = root.AddComponent<Button>();

        Image icon = CreateImage(root.transform, "Icon", null, new Vector2(size * 0.62f, size * 0.62f));
        icon.preserveAspect = true;

        Image overlay = CreateImage(root.transform, "Cooldown", circle, new Vector2(size, size));
        overlay.color = new Color(0f, 0f, 0f, 0.65f);
        overlay.type = Image.Type.Filled;
        overlay.fillMethod = Image.FillMethod.Radial360;
        overlay.fillOrigin = (int)Image.Origin360.Top;
        overlay.fillClockwise = false;

        Image glow = CreateImage(root.transform, "Ready", ring, new Vector2(size + 8f, size + 8f));
        glow.color = accent;

        GameObject keyGo = CreatePanel(root.transform, "Key", new Vector2(1f, 0f), new Vector2(4f, -4f), new Vector2(48f, 48f),
            new Color(0.07f, 0.05f, 0.09f, 1f), accent);
        ((RectTransform)keyGo.transform).pivot = new Vector2(1f, 0f);
        Text keyText = CreateText(keyGo.transform, "Label", key, 30, Color.white, Vector2.zero);
        ((RectTransform)keyText.transform).sizeDelta = new Vector2(48f, 48f);
        keyText.fontStyle = FontStyle.Bold;

        SkillButtonHUD hud = root.AddComponent<SkillButtonHUD>();
        SetRefs(hud, ("skill", skill), ("button", button), ("icon", icon), ("cooldownOverlay", overlay), ("readyGlow", glow));
    }

    // Painel retangular com borda (Outline)
    private static GameObject CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, Color color, Color border)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(anchor.x, anchor.y);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        Image image = go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = border;
        outline.effectDistance = new Vector2(3f, -3f);
        return go;
    }

    private static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        ((RectTransform)go.transform).sizeDelta = size;
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }

    private static void Place(Text text, Vector2 anchor, Vector2 position, Vector2 size, TextAnchor alignment)
    {
        RectTransform rt = (RectTransform)text.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        text.alignment = alignment;
    }

    // Garante que a camada (layer) exista no projeto, com o nome dado
    private static void EnsureLayer(int index, string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        SerializedProperty layer = layers.GetArrayElementAtIndex(index);
        if (layer.stringValue != layerName)
        {
            layer.stringValue = layerName;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    // Região (etapa 6): boss do Santuário e os dados da região
    private static RegionData CreateRegion(string worldDir, System.Collections.Generic.List<ZoneData> zoneList)
    {
        // Sprite provisório do boss: oni escuro com máscara vermelha e chifres dourados (32x32)
        Sprite bossSprite = PaintSprite("Boss_KageOni", 32, 32, 20f, (x, y) =>
        {
            float dx = x - 15.5f;
            bool horn = y >= 26 && y <= 31 && (Mathf.Abs(x - (8 - (y - 26) / 2)) <= 1 || Mathf.Abs(x - (23 + (y - 26) / 2)) <= 1);
            bool head = dx * dx + (y - 19) * (y - 19) <= 64;
            bool body = y >= 2 && y < 18 && Mathf.Abs(dx) <= 10 - (17 - y) * 0.15f && !(y < 4 && x % 5 == 0);
            if (horn) return new Color32(222, 180, 64, 255);
            if (head)
            {
                bool eye = (x == 12 || x == 13 || x == 18 || x == 19) && (y == 20 || y == 21);
                bool mouth = y == 15 && x >= 11 && x <= 20 && x % 2 == 0;
                if (eye) return new Color32(255, 240, 120, 255);
                if (mouth) return new Color32(250, 250, 250, 255);
                return new Color32(170, 30, 35, 255); // máscara vermelha
            }
            if (body) return (Mathf.Abs(dx) < 3 ? new Color32(222, 180, 64, 255) : new Color32(35, 25, 40, 255));
            return null;
        });

        EnemyData boss = CreateSpecialEnemy(Root + "/ScriptableObjects/Enemies/Boss_KageOni.asset", "Kage-Oni", bossSprite, EnemyRank.Boss,
            3.6f, Color.white, 2.4f, 5000f, 35f, 0, 0.8f);

        string path = worldDir + "/Region_BambooForest.asset";
        bool isNew = AssetDatabase.LoadAssetAtPath<RegionData>(path) == null;
        RegionData region = GetOrCreateAsset<RegionData>(path);
        if (isNew)
        {
            region.displayName = "Floresta de Bambu";
            region.zones = zoneList;
            region.boss = boss;
        }
        EditorUtility.SetDirty(region);
        return region;
    }

    // Santuário (etapa 6): arena, ataque do boss, relógio da partida e tela de vitória
    private static SanctuaryController CreateSanctuary(ZoneController zones, HordeSpawner spawner, PlayerHealth health, GameObject canvasGo)
    {
        Sprite ring = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/FX_Ring.png");

        GameObject sanctuaryGo = new GameObject("Sanctuary");
        RunClock clock = sanctuaryGo.AddComponent<RunClock>();

        SpriteRenderer arenaRing = new GameObject("ArenaRing").AddComponent<SpriteRenderer>();
        arenaRing.transform.SetParent(sanctuaryGo.transform, false);
        arenaRing.sprite = ring;
        arenaRing.sortingOrder = 30;
        ArenaBoundary arena = sanctuaryGo.AddComponent<ArenaBoundary>();
        SetRefs(arena, ("player", health.GetComponent<CharacterController>()), ("ring", arenaRing));

        SpriteRenderer warning = new GameObject("BossWarningRing").AddComponent<SpriteRenderer>();
        warning.transform.SetParent(sanctuaryGo.transform, false);
        warning.sprite = ring;
        warning.sortingOrder = 35;
        BossAttack attack = sanctuaryGo.AddComponent<BossAttack>();
        SetRefs(attack, ("playerHealth", health), ("warningRing", warning));

        SanctuaryController sanctuary = sanctuaryGo.AddComponent<SanctuaryController>();
        Light light = GameObject.Find("Directional Light").GetComponent<Light>();
        SetRefs(sanctuary, ("zones", zones), ("spawner", spawner), ("arena", arena), ("bossAttack", attack),
            ("player", health.transform), ("mainLight", light));

        // Tela de vitória
        GameObject panel = new GameObject("VictoryPanel", typeof(RectTransform));
        panel.transform.SetParent(canvasGo.transform, false);
        RectTransform panelRt = (RectTransform)panel.transform;
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        panel.AddComponent<Image>().color = new Color(0.03f, 0.03f, 0.06f, 0.8f);
        Text title = CreateText(panel.transform, "Title", "Vitória!", 120, new Color(1f, 0.85f, 0.3f), new Vector2(0f, 200f));
        title.fontStyle = FontStyle.Bold;
        Text summary = CreateText(panel.transform, "Summary", "", 46, Color.white, new Vector2(0f, 10f));
        Text medal = CreateText(panel.transform, "Medal", "", 60, Color.white, new Vector2(0f, -160f));
        medal.fontStyle = FontStyle.Bold;
        CreateText(panel.transform, "Hint", "R ou toque para jogar de novo", 36, new Color(0.85f, 0.85f, 0.9f), new Vector2(0f, -320f));

        VictoryScreen victory = sanctuaryGo.AddComponent<VictoryScreen>();
        SetRefs(victory, ("sanctuary", sanctuary), ("zones", zones), ("spawner", spawner),
            ("experience", health.GetComponent<PlayerExperience>()), ("clock", clock), ("panel", panel),
            ("summaryText", summary), ("medalText", medal));
        return sanctuary;
    }

    private static EnemyData CreateSpecialEnemy(string path, string displayName, Sprite sprite, EnemyRank rank, float scale, Color tint,
        float speed, float health, float contactDamage, int xp, float spriteHeight)
    {
        bool isNew = AssetDatabase.LoadAssetAtPath<EnemyData>(path) == null;
        EnemyData data = GetOrCreateAsset<EnemyData>(path);
        data.sprite = sprite;
        if (isNew)
        {
            data.displayName = displayName;
            data.rank = rank;
            data.scale = scale;
            data.tint = tint;
            data.moveSpeed = speed;
            data.maxHealth = health;
            data.contactDamage = contactDamage;
            data.xpValue = xp;
            data.spriteHeight = spriteHeight;
            data.spawnWeight = 0f;
        }
        EditorUtility.SetDirty(data);
        return data;
    }

    private static HordeSettings CreateScaledHorde(HordeSettings source, string path, float rateMultiplier, int waveBase)
    {
        HordeSettings horde = AssetDatabase.LoadAssetAtPath<HordeSettings>(path);
        if (horde != null)
        {
            return horde;
        }
        horde = Object.Instantiate(source);
        Keyframe[] keys = horde.spawnsPerSecond.keys;
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].value *= rateMultiplier;
        }
        horde.spawnsPerSecond = new AnimationCurve(keys);
        horde.waveBaseCount = waveBase;
        AssetDatabase.CreateAsset(horde, path);
        return horde;
    }

    private static ZoneData CreateZone(string path, string displayName, HordeSettings horde, float healthMultiplier,
        EnemyData[] elites, EnemyData[] guardians)
    {
        bool isNew = AssetDatabase.LoadAssetAtPath<ZoneData>(path) == null;
        ZoneData zone = GetOrCreateAsset<ZoneData>(path);
        if (isNew)
        {
            zone.displayName = displayName;
            zone.horde = horde;
            zone.enemyHealthMultiplier = healthMultiplier;
            zone.elites = new System.Collections.Generic.List<EnemyData>(elites);
            zone.guardianOptions = new System.Collections.Generic.List<EnemyData>(guardians);
        }
        EditorUtility.SetDirty(zone);
        return zone;
    }

    private static PassiveDefinition CreatePassive(string dir, string file, string displayName, Sprite icon, PassiveStat stat, float value, string format)
    {
        string path = dir + "/" + file + ".asset";
        bool isNew = AssetDatabase.LoadAssetAtPath<PassiveDefinition>(path) == null;
        PassiveDefinition passive = GetOrCreateAsset<PassiveDefinition>(path);
        passive.displayName = displayName;
        passive.icon = icon;
        if (isNew)
        {
            passive.stat = stat;
            passive.valuePerLevel = value;
            passive.descriptionFormat = format;
        }
        EditorUtility.SetDirty(passive);
        return passive;
    }

    // Anel branco 64x64 (efeito do Corte de Katana), 2 metros de diâmetro na escala 1
    private static Sprite CreateRingSprite()
    {
        const int size = 64;
        return PaintSprite("FX_Ring", size, size, 32f, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(32f, 32f));
            if (d > 31f || d < 25f) return null;
            byte a = (byte)(d > 29f ? 255 : 140);
            return new Color32(255, 255, 255, a);
        }, false);
    }

    // Desenha um sprite pixel a pixel. "paint" devolve a cor do pixel ou null (transparente).
    // Com outline = true, ganha contorno escuro de 1 pixel em volta.
    private static Sprite PaintSprite(string name, int width, int height, float pixelsPerUnit,
        System.Func<int, int, Color32?> paint, bool outline = true)
    {
        int w = width + (outline ? 2 : 0), h = height + (outline ? 2 : 0);
        int o = outline ? 1 : 0;
        Color32[] pixels = new Color32[w * h];
        bool[] filled = new bool[w * h];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32? c = paint(x, y);
                if (c.HasValue)
                {
                    pixels[(y + o) * w + x + o] = c.Value;
                    filled[(y + o) * w + x + o] = true;
                }
            }
        }
        if (outline)
        {
            Color32 dark = new Color32(16, 12, 16, 255);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (filled[y * w + x]) continue;
                    bool near = false;
                    for (int dy = -1; dy <= 1 && !near; dy++)
                        for (int dx = -1; dx <= 1 && !near; dx++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx >= 0 && ny >= 0 && nx < w && ny < h && filled[ny * w + nx]) near = true;
                        }
                    if (near) pixels[y * w + x] = dark;
                }
            }
        }
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels32(pixels);
        return SaveSprite(tex, name, pixelsPerUnit, true);
    }

    private static Text CreateText(Transform parent, string name, string content, int size, Color color, Vector2 position)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(1600f, 300f);
        Text text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = content;
        text.raycastTarget = false;
        go.AddComponent<Outline>().effectColor = Color.black;
        return text;
    }

    // Preenche campos [SerializeField] de um componente pelo nome
    private static void SetRefs(Object target, params (string field, Object value)[] refs)
    {
        SerializedObject so = new SerializedObject(target);
        foreach (var r in refs)
        {
            SerializedProperty prop = so.FindProperty(r.field);
            if (prop == null)
            {
                Debug.LogError("[ProjectSetup] Campo não encontrado: " + target.GetType().Name + "." + r.field);
                continue;
            }
            prop.objectReferenceValue = r.value;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }

    // Estrela de 4 pontas 13x13, cinza metálico
    private static Sprite CreateShurikenSprite()
    {
        const int size = 13;
        Color32 clear = new Color32(0, 0, 0, 0);
        Color32 outline = new Color32(16, 12, 16, 255);
        Color32 steel = new Color32(190, 196, 210, 255);
        Color32 dark = new Color32(110, 116, 130, 255);
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = Mathf.Abs(x - 6), dy = Mathf.Abs(y - 6);
                Color32 c = clear;
                // pontas: losango fino em cruz
                bool blade = (dx <= 1 && dy <= 6 - dx * 3) || (dy <= 1 && dx <= 6 - dy * 3) || (dx + dy <= 3);
                if (blade) c = (dx + dy <= 1) ? dark : steel;
                if (dx == 0 && dy == 0) c = outline; // furo do meio
                tex.SetPixel(x, y, c);
            }
        }
        return SaveSprite(tex, "Shuriken", 20f, true);
    }

    // Dígitos 0-9 em pixel art 5x7 com contorno (cada célula 7x9), lado a lado
    private static Texture2D CreateDigitsTexture()
    {
        string[] glyphs =
        {
            "01110100011001110101110011000101110", // 0
            "00100011000010000100001000010001110", // 1
            "01110100010000100010001000100011111", // 2
            "11111000100010000010000011000101110", // 3
            "00010001100101010010111110001000010", // 4
            "11111100001111000001000011000101110", // 5
            "00110010001000011110100011000101110", // 6
            "11111000010001000100010000100001000", // 7
            "01110100011000101110100011000101110", // 8
            "01110100011000101111000010001001100", // 9
        };
        const int cellW = 7, cellH = 9;
        Texture2D tex = new Texture2D(cellW * 10, cellH, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);
        Color32 outline = new Color32(10, 8, 12, 255);
        Color32 white = new Color32(255, 255, 255, 255);
        Color32[] pixels = new Color32[tex.width * tex.height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        for (int d = 0; d < 10; d++)
        {
            // Primeiro o contorno, depois o miolo branco por cima
            for (int pass = 0; pass < 2; pass++)
            {
                for (int gy = 0; gy < 7; gy++)
                {
                    for (int gx = 0; gx < 5; gx++)
                    {
                        if (glyphs[d][gy * 5 + gx] != '1') continue;
                        int px = d * cellW + 1 + gx;
                        int py = cellH - 2 - gy; // linha 0 do glifo é o topo
                        if (pass == 0)
                        {
                            for (int oy = -1; oy <= 1; oy++)
                                for (int ox = -1; ox <= 1; ox++)
                                    pixels[(py + oy) * tex.width + px + ox] = outline;
                        }
                        else
                        {
                            pixels[py * tex.width + px] = white;
                        }
                    }
                }
            }
        }
        tex.SetPixels32(pixels);
        Sprite unused = SaveSprite(tex, "UI_Digits", 14f, false);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Sprites/UI_Digits.png");
    }

    // Quadrado branco 4x4 (1 metro) para barras
    private static Sprite CreatePixelSprite()
    {
        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[16];
        for (int i = 0; i < 16; i++) pixels[i] = new Color32(255, 255, 255, 255);
        tex.SetPixels32(pixels);
        return SaveSprite(tex, "UI_Pixel", 4f, true);
    }

    // Salva a textura como PNG e importa como sprite em pixel art (filtro Point)
    private static Sprite SaveSprite(Texture2D tex, string name, float pixelsPerUnit, bool asSprite)
    {
        string path = Root + "/Sprites/" + name + ".png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = asSprite ? TextureImporterType.Sprite : TextureImporterType.Default;
        if (asSprite)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
        }
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return asSprite ? AssetDatabase.LoadAssetAtPath<Sprite>(path) : null;
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
