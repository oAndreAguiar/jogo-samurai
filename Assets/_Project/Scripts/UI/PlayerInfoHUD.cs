using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Topo esquerdo do HUD: retrato, nome do personagem, nível da conta e nível na partida.
/// </summary>
public class PlayerInfoHUD : MonoBehaviour
{
    [SerializeField] private PlayerExperience experience;
    [SerializeField] private Image portrait;
    [SerializeField] private Text nameText;
    [SerializeField] private Text accountLevelText;
    [SerializeField] private Text runLevelText;

    [Tooltip("Nome do personagem (vem da seleção de personagem no futuro).")]
    [SerializeField] private string characterName = "Samurai";
    [SerializeField] private Sprite portraitSprite;

    private void Start()
    {
        portrait.sprite = portraitSprite;
        nameText.text = characterName;
        accountLevelText.text = "Conta Nv " + PlayerProfile.AccountLevel;
    }

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
        runLevelText.text = "Nv " + level;
    }
}
