using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de experiência no topo da tela, com o nível atual.
/// Só é atualizada quando a XP muda (por evento).
/// </summary>
public class ExperienceBar : MonoBehaviour
{
    [SerializeField] private PlayerExperience experience;
    [SerializeField] private Image fill;
    [SerializeField] private Text levelText;

    private void OnEnable()
    {
        experience.ExperienceChanged += HandleExperienceChanged;
    }

    private void OnDisable()
    {
        experience.ExperienceChanged -= HandleExperienceChanged;
    }

    private void HandleExperienceChanged(int current, int toNext, int level)
    {
        fill.fillAmount = toNext > 0 ? (float)current / toNext : 0f;
        levelText.text = "Nv " + level;
    }
}
