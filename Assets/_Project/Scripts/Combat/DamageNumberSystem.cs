using UnityEngine;

/// <summary>
/// Mostra números de dano saindo dos inimigos atingidos.
/// Usa uma lista fixa de números reaproveitados (pool circular): nada é criado durante o jogo.
/// </summary>
public class DamageNumberSystem : MonoBehaviour
{
    [SerializeField] private DamageNumberSettings settings;
    [SerializeField] private HordeSpawner spawner;

    private DamageNumber[] numbers;
    private Sprite[] digitSprites;
    private float digitWidth;
    private int nextIndex;
    private Transform cameraTransform;

    private void Awake()
    {
        BuildDigitSprites();

        Transform root = new GameObject("DamageNumbers").transform;
        numbers = new DamageNumber[settings.maxNumbers];
        for (int i = 0; i < numbers.Length; i++)
        {
            GameObject go = new GameObject("DamageNumber");
            go.transform.SetParent(root, false);
            numbers[i] = go.AddComponent<DamageNumber>();
            numbers[i].Setup(100); // por cima de inimigos e projéteis
            go.SetActive(false);
        }
    }

    private void OnEnable()
    {
        spawner.EnemyDamaged += HandleEnemyDamaged;
    }

    private void OnDisable()
    {
        spawner.EnemyDamaged -= HandleEnemyDamaged;
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void HandleEnemyDamaged(Enemy enemy, float damage, bool killed)
    {
        // Pequeno desvio aleatório para os números não ficarem um em cima do outro
        Vector3 jitter = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0f, 0.3f), 0f);
        Spawn(Mathf.RoundToInt(damage), enemy.CenterPosition + Vector3.up * 0.5f + jitter, killed);
    }

    public void Spawn(int value, Vector3 position, bool killed)
    {
        DamageNumber number = numbers[nextIndex];
        nextIndex = (nextIndex + 1) % numbers.Length;

        number.gameObject.SetActive(true);
        number.InUse = true;
        number.Age = 0f;
        number.StartPosition = position;
        number.Show(value, digitSprites, digitWidth, killed ? settings.killColor : settings.normalColor);
        number.transform.position = position;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        Quaternion faceCamera = cameraTransform != null ? cameraTransform.rotation : Quaternion.identity;

        for (int i = 0; i < numbers.Length; i++)
        {
            DamageNumber number = numbers[i];
            if (!number.InUse)
            {
                continue;
            }

            number.Age += dt;
            float t = number.Age / settings.lifetime;
            if (t >= 1f)
            {
                number.InUse = false;
                number.gameObject.SetActive(false);
                continue;
            }

            // Sobe rápido e desacelera; cresce no começo ("pop") e some no final
            float rise = 1f - (1f - t) * (1f - t);
            float scale = t < 0.15f ? Mathf.Lerp(0.6f, 1.25f, t / 0.15f) : Mathf.Lerp(1.25f, 1f, (t - 0.15f) / 0.85f);
            number.transform.SetPositionAndRotation(number.StartPosition + Vector3.up * rise * settings.riseHeight, faceCamera);
            number.transform.localScale = Vector3.one * scale;
            number.SetAlpha(t < 0.7f ? 1f : 1f - (t - 0.7f) / 0.3f);
        }
    }

    // Corta a textura de dígitos em 10 sprites (uma vez só, no início)
    private void BuildDigitSprites()
    {
        Texture2D tex = settings.digitsTexture;
        float cellWidth = tex.width / 10f;
        digitSprites = new Sprite[10];
        for (int d = 0; d < 10; d++)
        {
            Rect rect = new Rect(d * cellWidth, 0f, cellWidth, tex.height);
            digitSprites[d] = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), settings.pixelsPerUnit, 0, SpriteMeshType.FullRect);
        }
        // Os dígitos ficam encostados, com 1 pixel de contorno se sobrepondo
        digitWidth = (cellWidth - 1f) / settings.pixelsPerUnit;
    }
}
