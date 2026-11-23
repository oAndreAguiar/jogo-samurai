using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Tela de vitória: aparece quando o boss do Santuário morre.
/// Mostra tempo total, abates, nível e a medalha pelo tempo. R, Enter ou toque recomeça.
/// </summary>
public class VictoryScreen : MonoBehaviour
{
    [SerializeField] private SanctuaryController sanctuary;
    [SerializeField] private ZoneController zones;
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private PlayerExperience experience;
    [SerializeField] private RunClock clock;
    [SerializeField] private GameObject panel;
    [SerializeField] private Text summaryText;
    [SerializeField] private Text medalText;

    [SerializeField] private float restartDelay = 1f;

    private float shownTime = -1f;

    public string Medal { get; private set; }

    private void Awake()
    {
        panel.SetActive(false);
    }

    private void OnEnable()
    {
        sanctuary.Victory += HandleVictory;
    }

    private void OnDisable()
    {
        sanctuary.Victory -= HandleVictory;
    }

    private void HandleVictory()
    {
        clock.Stop();
        RegionData region = zones.Region;
        float minutes = clock.TotalTime / 60f;

        Color medalColor;
        if (minutes <= region.goldMinutes)
        {
            Medal = "Ouro";
            medalColor = new Color(1f, 0.82f, 0.25f);
        }
        else if (minutes <= region.silverMinutes)
        {
            Medal = "Prata";
            medalColor = new Color(0.82f, 0.85f, 0.9f);
        }
        else
        {
            Medal = "Bronze";
            medalColor = new Color(0.8f, 0.5f, 0.25f);
        }

        summaryText.text = string.Format("{0}\n\nTempo {1}   Abates {2}   Nível {3}",
            region.displayName, RunClock.Format(clock.TotalTime), spawner.KillCount, experience.Level);
        medalText.text = "Medalha de " + Medal;
        medalText.color = medalColor;

        panel.SetActive(true);
        shownTime = Time.unscaledTime;
        Time.timeScale = 0f;
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
