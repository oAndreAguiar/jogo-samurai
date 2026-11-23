using System;
using UnityEngine;

/// <summary>
/// Altar de Fusão: um por zona, num ponto do mapa. A fusão só acontece aqui.
/// Quando o jogador chega perto, avisa (PlayerArrived) para a tela de fusão abrir.
/// Depois de uma fusão, o altar se apaga até a próxima zona.
/// </summary>
public class FusionAltar : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private ZoneController zones;
    [SerializeField] private SanctuaryController sanctuary;
    [SerializeField] private MinimapController minimap;
    [SerializeField] private SpriteRenderer glow;
    [SerializeField] private SpriteRenderer ring;

    [Tooltip("Distância do jogador para ativar o altar, em metros.")]
    [SerializeField] private float activationRadius = 2.4f;

    [Tooltip("Distância do centro do mapa onde o altar pode aparecer (mín. e máx.).")]
    [SerializeField] private Vector2 distanceFromCenter = new Vector2(18f, 32f);

    [Tooltip("Metade do tamanho do mapa (o chão vai de -limite a +limite).")]
    [SerializeField] private float mapHalfSize = 44f;

    [SerializeField] private Color activeColor = new Color(0.4f, 0.95f, 1f);
    [SerializeField] private Color usedColor = new Color(0.35f, 0.35f, 0.4f);

    public event Action<FusionAltar> PlayerArrived;

    public bool IsUsed { get; private set; }
    public bool PlayerInside { get; private set; }

    private SpriteRenderer minimapMarker;
    private Vector3 ringBaseScale;

    private void Awake()
    {
        ringBaseScale = ring.transform.localScale;
    }

    private void OnEnable()
    {
        zones.ZoneStarted += HandleZoneStarted;
        sanctuary.SanctuaryStarted += Hide;
    }

    private void OnDisable()
    {
        zones.ZoneStarted -= HandleZoneStarted;
        sanctuary.SanctuaryStarted -= Hide;
    }

    private void Start()
    {
        minimapMarker = minimap.AddPointOfInterest(activeColor, 3f);
        // Garante uma posição caso a zona ainda não tenha posicionado o altar
        if (transform.position == Vector3.zero)
        {
            PlaceRandomly();
        }
        UpdateMarker();
    }

    private void HandleZoneStarted(int index, ZoneData zone, EnemyData guardian)
    {
        gameObject.SetActive(true);
        IsUsed = false;
        PlayerInside = false;
        PlaceRandomly();
        SetColor(activeColor);
        UpdateMarker();
    }

    private void Hide()
    {
        if (minimapMarker != null)
        {
            minimapMarker.gameObject.SetActive(false);
        }
        gameObject.SetActive(false); // no Santuário não há fusão
    }

    private void Update()
    {
        // Brilho pulsando
        float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * 3f);
        ring.transform.localScale = ringBaseScale * (IsUsed ? 1f : pulse);

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        bool inside = offset.sqrMagnitude <= activationRadius * activationRadius;

        // Abre a tela só ao ENTRAR (para não reabrir enquanto o jogador continua ali)
        if (inside && !PlayerInside && !IsUsed && Time.timeScale > 0f)
        {
            PlayerArrived?.Invoke(this);
        }
        PlayerInside = inside;
    }

    /// <summary>Chamado depois de uma fusão: o altar se apaga até a próxima zona.</summary>
    public void MarkUsed()
    {
        IsUsed = true;
        SetColor(usedColor);
        UpdateMarker();
    }

    private void PlaceRandomly()
    {
        // Procura um lugar livre no chão (sem blocos em cima)
        for (int attempt = 0; attempt < 20; attempt++)
        {
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float distance = UnityEngine.Random.Range(distanceFromCenter.x, distanceFromCenter.y);
            Vector3 p = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
            if (Mathf.Abs(p.x) > mapHalfSize || Mathf.Abs(p.z) > mapHalfSize)
            {
                continue;
            }
            if (Physics.Raycast(p + Vector3.up * 30f, Vector3.down, out RaycastHit hit, 40f) && hit.point.y < 0.05f
                && !Physics.CheckSphere(hit.point + Vector3.up * 2f, 1.6f)) // esfera acima do chão: só acha blocos
            {
                transform.position = hit.point;
                return;
            }
        }
        transform.position = new Vector3(distanceFromCenter.x, 0f, 0f);
    }

    private void SetColor(Color color)
    {
        glow.color = color;
        Color ringColor = color;
        ringColor.a = 0.7f;
        ring.color = ringColor;
    }

    private void UpdateMarker()
    {
        if (minimapMarker == null)
        {
            return;
        }
        minimapMarker.gameObject.SetActive(true);
        minimapMarker.color = IsUsed ? usedColor : activeColor;
        Vector3 p = transform.position;
        minimapMarker.transform.position = new Vector3(p.x, MinimapController.MarkerY(p.y), p.z);
    }
}
