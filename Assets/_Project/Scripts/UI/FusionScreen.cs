using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Tela do Altar de Fusão. Abre quando o jogador chega no altar e pausa o jogo.
/// Se houver uma receita pronta (duas habilidades no máximo que combinam), mostra A + B = C
/// e o botão Fundir (F). Se não houver, explica o que falta. Esc ou "Agora não" fecha.
/// </summary>
public class FusionScreen : MonoBehaviour
{
    [SerializeField] private FusionAltar altar;
    [SerializeField] private PlayerLoadout loadout;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject panel;

    [Header("Receita")]
    [SerializeField] private GameObject recipeGroup;
    [SerializeField] private Image iconA;
    [SerializeField] private Image iconB;
    [SerializeField] private Image iconResult;
    [SerializeField] private Text recipeText;
    [SerializeField] private Text messageText;

    [Header("Botões")]
    [SerializeField] private Button fuseButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private float inputDelay = 0.3f;

    private FusionRecipe recipe;
    private float shownTime;

    public bool IsOpen => panel.activeSelf;

    private void Awake()
    {
        panel.SetActive(false);
        fuseButton.onClick.AddListener(Fuse);
        closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        altar.PlayerArrived += Open;
    }

    private void OnDisable()
    {
        altar.PlayerArrived -= Open;
    }

    private void Open(FusionAltar source)
    {
        if (IsOpen || playerHealth.IsDead)
        {
            return;
        }

        recipe = loadout.FindAvailableFusion();
        bool ready = recipe != null;
        recipeGroup.SetActive(ready);
        fuseButton.gameObject.SetActive(ready);

        if (ready)
        {
            iconA.sprite = recipe.ingredientA.icon;
            iconB.sprite = recipe.ingredientB.icon;
            iconResult.sprite = recipe.result.icon;
            recipeText.text = string.Format("{0}  +  {1}  =  {2}",
                recipe.ingredientA.displayName, recipe.ingredientB.displayName, recipe.result.displayName);
            messageText.text = recipe.description + "\n\nAs duas habilidades somem e a nova ocupa o lugar.";
        }
        else
        {
            messageText.text = "Nenhuma fusão disponível.\n\nLeve duas habilidades que combinam ao nível máximo e volte a um Altar de Fusão.";
        }

        panel.SetActive(true);
        shownTime = Time.unscaledTime;
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (!IsOpen || Keyboard.current == null || Time.unscaledTime - shownTime < inputDelay)
        {
            return;
        }
        if (Keyboard.current.fKey.wasPressedThisFrame && recipe != null)
        {
            Fuse();
        }
        else if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    public void Fuse()
    {
        if (!IsOpen || recipe == null || Time.unscaledTime - shownTime < inputDelay)
        {
            return;
        }
        if (loadout.Fuse(recipe))
        {
            altar.MarkUsed();
        }
        Close();
    }

    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }
        panel.SetActive(false);
        recipe = null;
        if (!playerHealth.IsDead)
        {
            Time.timeScale = 1f;
        }
    }
}
