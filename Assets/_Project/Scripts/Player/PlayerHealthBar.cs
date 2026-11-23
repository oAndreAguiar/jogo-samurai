using UnityEngine;

/// <summary>
/// Barra de vida pequena embaixo do personagem (fundo escuro + preenchimento colorido).
/// Fica sempre virada para a câmera e só é atualizada quando a vida muda.
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private PlayerHealth health;
    [SerializeField] private Transform fill;
    [SerializeField] private SpriteRenderer fillRenderer;

    [SerializeField] private Color healthyColor = new Color(0.35f, 0.85f, 0.35f);
    [SerializeField] private Color lowColor = new Color(0.9f, 0.2f, 0.2f);

    private float fullWidth;
    private Transform cameraTransform;

    private void Awake()
    {
        fullWidth = fill.localScale.x;
    }

    private void OnEnable()
    {
        health.HealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        health.HealthChanged -= HandleHealthChanged;
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform != null)
        {
            transform.rotation = cameraTransform.rotation;
        }
    }

    private void HandleHealthChanged(float current, float max)
    {
        float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;

        // Encolhe a barra a partir da esquerda
        Vector3 scale = fill.localScale;
        scale.x = fullWidth * ratio;
        fill.localScale = scale;
        Vector3 pos = fill.localPosition;
        pos.x = -fullWidth * (1f - ratio) * 0.5f;
        fill.localPosition = pos;

        fillRenderer.color = Color.Lerp(lowColor, healthyColor, ratio);
    }
}
