using UnityEngine;

/// <summary>
/// Joias de experiência: nascem onde um inimigo morre, flutuam no chão,
/// são puxadas quando o jogador chega perto e viram XP ao encostar.
/// Usa uma lista fixa de joias (pool): quando enche, a mais antiga é juntada na nova.
/// </summary>
public class ExperienceGemSystem : MonoBehaviour
{
    private const float CollectDistance = 0.5f;
    private const float GemHeight = 0.35f;

    [SerializeField] private ProgressionSettings settings;
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private PlayerExperience experience;
    [SerializeField] private PlayerLoadout loadout;

    private ExperienceGem[] gems;
    private int nextIndex;
    private Transform player;
    private Transform cameraTransform;

    public int ActiveGemCount { get; private set; }

    private void Awake()
    {
        Transform root = new GameObject("ExperienceGems").transform;
        gems = new ExperienceGem[settings.maxGems];
        for (int i = 0; i < gems.Length; i++)
        {
            GameObject go = new GameObject("Gem");
            go.transform.SetParent(root, false);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            gems[i] = go.AddComponent<ExperienceGem>();
            gems[i].Setup(sr);
            go.SetActive(false);
        }
        player = experience.transform;
    }

    private void OnEnable()
    {
        spawner.EnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        spawner.EnemyKilled -= HandleEnemyKilled;
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void HandleEnemyKilled(Enemy enemy)
    {
        Spawn(enemy.GroundPosition, enemy.Data.xpValue);
    }

    public void Spawn(Vector3 groundPosition, int value)
    {
        if (value <= 0)
        {
            return;
        }

        ExperienceGem gem = gems[nextIndex];
        nextIndex = (nextIndex + 1) % gems.Length;

        // Lista cheia: a joia mais antiga é reaproveitada e o valor dela não se perde
        if (gem.InUse)
        {
            value += gem.Value;
            ActiveGemCount--;
        }

        gem.Value = value;
        gem.InUse = true;
        gem.Attracted = false;
        gem.Speed = 0f;
        gem.SpawnTime = Time.time;
        gem.BasePosition = groundPosition + Vector3.up * GemHeight;
        gem.transform.position = gem.BasePosition;
        gem.SetSprite(value >= settings.bigGemThreshold ? settings.bigGemSprite : settings.smallGemSprite);
        gem.gameObject.SetActive(true);
        ActiveGemCount++;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        Vector3 target = player.position + Vector3.up * 0.6f;
        float radius = settings.pickupRadius * loadout.Modifiers.PickupRadiusMultiplier;
        float radiusSqr = radius * radius;
        Quaternion faceCamera = cameraTransform != null ? cameraTransform.rotation : Quaternion.identity;
        float time = Time.time;

        for (int i = 0; i < gems.Length; i++)
        {
            ExperienceGem gem = gems[i];
            if (!gem.InUse)
            {
                continue;
            }

            Vector3 pos = gem.transform.position;
            Vector3 toPlayer = target - pos;

            if (!gem.Attracted)
            {
                Vector3 flat = toPlayer;
                flat.y = 0f;
                if (flat.sqrMagnitude <= radiusSqr)
                {
                    gem.Attracted = true;
                }
                else
                {
                    // Flutua levemente no lugar
                    pos = gem.BasePosition + Vector3.up * (Mathf.Sin((time - gem.SpawnTime) * 4f + i) * 0.08f);
                    gem.transform.SetPositionAndRotation(pos, faceCamera);
                    continue;
                }
            }

            // Puxada: acelera em direção ao jogador
            gem.Speed = Mathf.Min(settings.gemMagnetSpeed, gem.Speed + settings.gemMagnetSpeed * 3f * dt);
            float distance = toPlayer.magnitude;
            if (distance <= CollectDistance || distance <= gem.Speed * dt)
            {
                Collect(gem);
                continue;
            }
            pos += toPlayer / distance * gem.Speed * dt;
            gem.transform.SetPositionAndRotation(pos, faceCamera);
        }
    }

    private void Collect(ExperienceGem gem)
    {
        int value = gem.Value;
        gem.InUse = false;
        gem.gameObject.SetActive(false);
        ActiveGemCount--;
        experience.AddExperience(value);
    }
}
