using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Tela de derrota: aparece quando o jogador morre, pausa o jogo
/// e reinicia a cena com R, Enter, clique ou toque.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private GameObject panel;
    [SerializeField] private Text summaryText;

    [Tooltip("Espera antes de aceitar o reinício, para não reiniciar sem querer.")]
    [SerializeField] private float restartDelay = 0.8f;

    private float shownTime = -1f;

    private void Awake()
    {
        panel.SetActive(false);
    }

    private void OnEnable()
    {
        playerHealth.Died += HandleDied;
    }

    private void OnDisable()
    {
        playerHealth.Died -= HandleDied;
    }

    private void HandleDied()
    {
        int seconds = Mathf.FloorToInt(spawner.ElapsedTime);
        summaryText.text = string.Format(
            "Sobreviveu {0:00}:{1:00}   Abates {2}\n\nR ou toque para recomeçar",
            seconds / 60, seconds % 60, spawner.KillCount);
        panel.SetActive(true);
        shownTime = Time.unscaledTime;
        Time.timeScale = 0f; // pausa inimigos, projéteis e timer
    }

    private void Update()
    {
        if (shownTime < 0f || Time.unscaledTime - shownTime < restartDelay)
        {
            return;
        }

        bool restart =
            (Keyboard.current != null && (Keyboard.current.rKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            || (Pointer.current != null && Pointer.current.press.wasPressedThisFrame);

        if (restart)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
