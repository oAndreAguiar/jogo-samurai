using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD da zona: timer, botão de chamar o guardião, avisos no meio da tela
/// e barra de vida do guardião ou do chefe do Santuário.
/// </summary>
public class ZoneHUD : MonoBehaviour
{
    [SerializeField] private ZoneController zones;
    [SerializeField] private SanctuaryController sanctuary;

    [Header("Topo")]
    [Tooltip("Linha com região, zona e dificuldade.")]
    [SerializeField] private Text zoneLineText;
    [Tooltip("Contagem regressiva da zona, em destaque.")]
    [SerializeField] private Text timerText;
    [Tooltip("Dificuldade mostrada no HUD (as dificuldades entram depois).")]
    [SerializeField] private string difficultyName = "Normal";
    [SerializeField] private Button callGuardianButton;

    [Header("Aviso central")]
    [SerializeField] private Text bannerText;
    [SerializeField] private float bannerSeconds = 2.5f;

    [Header("Guardião / chefe")]
    [SerializeField] private GameObject guardianBar;
    [SerializeField] private Image guardianFill;
    [SerializeField] private Text guardianName;

    private float bannerTimer;
    private float refreshTimer;

    // Inimigo cuja vida aparece na barra do topo (guardião ou boss)
    private Enemy trackedEnemy;

    private void Awake()
    {
        callGuardianButton.onClick.AddListener(zones.CallGuardianEarly);
        guardianBar.SetActive(false);
        bannerText.text = "";
    }

    private void OnEnable()
    {
        zones.ZoneStarted += HandleZoneStarted;
        zones.GuardianSummoned += HandleGuardianSummoned;
        zones.EliteSpawned += HandleEliteSpawned;
        zones.ZoneCleared += HandleZoneCleared;
        sanctuary.SanctuaryStarted += HandleSanctuaryStarted;
        sanctuary.BossSpawned += HandleBossSpawned;
        sanctuary.Victory += HandleVictory;
    }

    private void OnDisable()
    {
        zones.ZoneStarted -= HandleZoneStarted;
        zones.GuardianSummoned -= HandleGuardianSummoned;
        zones.EliteSpawned -= HandleEliteSpawned;
        zones.ZoneCleared -= HandleZoneCleared;
        sanctuary.SanctuaryStarted -= HandleSanctuaryStarted;
        sanctuary.BossSpawned -= HandleBossSpawned;
        sanctuary.Victory -= HandleVictory;
    }

    private void HandleZoneStarted(int index, ZoneData zone, EnemyData guardian)
    {
        string guardianText = guardian != null ? "\nGuardião: " + guardian.displayName : "";
        ShowBanner(zone.displayName + "  " + (index + 1) + "/" + zones.ZoneCount + guardianText);
    }

    private void HandleGuardianSummoned(Enemy guardian)
    {
        Track(guardian, "Guardião: ");
        ShowBanner(guardian.Data.displayName + " apareceu!");
    }

    private void HandleEliteSpawned(Enemy elite)
    {
        ShowBanner("Elite: " + elite.Data.displayName);
    }

    private void HandleZoneCleared(int index)
    {
        Track(null, "");
        ShowBanner("Zona " + (index + 1) + " concluída!");
    }

    private void HandleSanctuaryStarted()
    {
        ShowBanner("Santuário\n<size=40>Só o chefe da região espera por você</size>");
    }

    private void HandleBossSpawned(Enemy boss)
    {
        Track(boss, "Chefe: ");
        ShowBanner(boss.Data.displayName + " desperta!");
    }

    private void HandleVictory()
    {
        Track(null, "");
        bannerText.text = "";
        bannerTimer = 0f;
    }

    private void Track(Enemy enemy, string prefix)
    {
        trackedEnemy = enemy;
        guardianBar.SetActive(enemy != null);
        if (enemy != null)
        {
            guardianName.text = prefix + enemy.Data.displayName;
            guardianFill.fillAmount = 1f;
        }
    }

    private void ShowBanner(string message)
    {
        bannerText.text = message;
        bannerTimer = bannerSeconds;
    }

    private void Update()
    {
        if (bannerTimer > 0f)
        {
            bannerTimer -= Time.unscaledDeltaTime;
            if (bannerTimer <= 0f)
            {
                bannerText.text = "";
            }
        }

        // Barra do guardião/chefe acompanha a vida (barato: é só um número)
        if (trackedEnemy != null && trackedEnemy.IsAlive)
        {
            guardianFill.fillAmount = Mathf.Clamp01(trackedEnemy.CurrentHealth / trackedEnemy.MaxHealth);
        }

        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer > 0f)
        {
            return;
        }
        refreshTimer = 0.2f;

        if (sanctuary.IsActive)
        {
            bool bossFight = trackedEnemy != null && trackedEnemy.Data.rank == EnemyRank.Boss;
            zoneLineText.text = zones.Region.displayName + "  ·  Santuário  ·  " + difficultyName;
            timerText.text = bossFight ? "CHEFE" : "--:--";
            callGuardianButton.gameObject.SetActive(false);
            return;
        }

        int seconds = Mathf.CeilToInt(zones.TimeRemaining);
        zoneLineText.text = string.Format("{0}  ·  Zona {1}/{2}  ·  {3}",
            zones.Region.displayName, zones.ZoneIndex + 1, zones.ZoneCount, difficultyName);
        timerText.text = zones.GuardianActive
            ? "GUARDIÃO"
            : string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
        callGuardianButton.gameObject.SetActive(zones.CanCallGuardian);
    }
}
