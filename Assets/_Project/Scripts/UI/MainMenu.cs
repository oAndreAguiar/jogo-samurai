using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menu inicial: Jogar, Manual de Criaturas, Habilidades e Fusões.
/// Troca entre o painel principal e as páginas de coleção.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Prototype";

    [Header("Painéis")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private CollectionPage creaturesPage;
    [SerializeField] private CollectionPage upgradesPage;
    [SerializeField] private CollectionPage recipesPage;

    [Header("Botões")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button creaturesButton;
    [SerializeField] private Button upgradesButton;
    [SerializeField] private Button recipesButton;

    [SerializeField] private Text profileText;

    private void Awake()
    {
        // Garante tempo normal (o jogo pode ter voltado pausado de uma tela de fim)
        Time.timeScale = 1f;

        playButton.onClick.AddListener(Play);
        creaturesButton.onClick.AddListener(() => Open(creaturesPage));
        upgradesButton.onClick.AddListener(() => Open(upgradesPage));
        recipesButton.onClick.AddListener(() => Open(recipesPage));
        creaturesPage.BackRequested += ShowMain;
        upgradesPage.BackRequested += ShowMain;
        recipesPage.BackRequested += ShowMain;
    }

    private void Start()
    {
        ShowMain();
        ProfileData p = PlayerProfile.Data;
        profileText.text = string.Format("Conta Nv {0}   ·   Moedas {1}   ·   Partidas {2}",
            PlayerProfile.AccountLevel, p.coins, p.runsPlayed);
    }

    public void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void Open(CollectionPage page)
    {
        mainPanel.SetActive(false);
        creaturesPage.gameObject.SetActive(page == creaturesPage);
        upgradesPage.gameObject.SetActive(page == upgradesPage);
        recipesPage.gameObject.SetActive(page == recipesPage);
        page.Open();
    }

    private void ShowMain()
    {
        mainPanel.SetActive(true);
        creaturesPage.gameObject.SetActive(false);
        upgradesPage.gameObject.SetActive(false);
        recipesPage.gameObject.SetActive(false);
    }
}
