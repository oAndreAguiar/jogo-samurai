using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Botão redondo de habilidade (ativa ou ultimate): mostra o ícone, a tecla
/// e um preenchimento circular enquanto recarrega. Tocar no botão usa a habilidade.
/// </summary>
public class SkillButtonHUD : MonoBehaviour
{
    [SerializeField] private PlayerSkill skill;
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private Image readyGlow;

    private void Awake()
    {
        button.onClick.AddListener(() => skill.TryActivate());
    }

    private void Start()
    {
        icon.sprite = skill.Icon;
    }

    private void Update()
    {
        float readiness = skill.Readiness;
        // Escurece a parte que ainda falta recarregar
        cooldownOverlay.fillAmount = 1f - readiness;
        bool ready = readiness >= 1f;
        icon.color = ready ? Color.white : new Color(0.55f, 0.55f, 0.6f);
        readyGlow.enabled = ready;
    }
}
