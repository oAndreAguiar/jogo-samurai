using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// HUD da zona: timer, botão de chamar o guardião, avisos no meio da tela
/// e barra de vida do guardião. Quando a região acaba, mostra a mensagem final.
/// </summary>
public class ZoneHUD : MonoBehaviour
{
    [SerializeField] private ZoneController zones;

    [Header("Topo")]
    [SerializeField] private Text timerText;
    [SerializeField] private Button callGuardianButton;

    [Header("Aviso central")]
    [SerializeField] private Text bannerText;
    [SerializeField] private float bannerSeconds = 2.5f;

    [Header("Guardião")]
    [SerializeField] private GameObject guardianBar;
    [SerializeField] private Image guardianFill;
    [SerializeField] private Text guardianName;

    private float bannerTimer;
    private float refreshTimer;
    private bool regionDone;

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
        zones.RegionCleared += HandleRegionCleared;
    }

    private void OnDisable()
    {
        zones.ZoneStarted -= HandleZoneStarted;
        zones.GuardianSummoned -= HandleGuardianSummoned;
        zones.EliteSpawned -= HandleEliteSpawned;
        zones.ZoneCleared -= HandleZoneCleared;
        zones.RegionCleared -= HandleRegionCleared;
    }

    private void HandleZoneStarted(int index, ZoneData zone, EnemyData guardian)
    {
        string guardianText = guardian != null ? "\nGuardião: " + guardian.displayName : "";
        ShowBanner(zone.displayName + "  " + (index + 1) + "/" + zones.ZoneCount + guardianText);
    }

    private void HandleGuardianSummoned(Enemy guardian)
    {
        guardianBar.SetActive(true);
        guardianName.text = guardian.Data.displayName;
        guardianFill.fillAmount = 1f;
        ShowBanner(guardian.Data.displayName + " apareceu!");
    }

    private void HandleEliteSpawned(Enemy elite)
    {
        ShowBanner("Elite: " + elite.Data.displayName);
    }

    private void HandleZoneCleared(int index)
    {
        guardianBar.SetActive(false);
        ShowBanner("Zona " + (index + 1) + " concluída!");
    }

    private void HandleRegionCleared()
    {
        regionDone = true;
        bannerTimer = float.MaxValue;
        bannerText.text = "Região concluída!\n<size=40>O Santuário vem na próxima etapa.\nR ou toque para jogar de novo</size>";
        Time.timeScale = 0f;
    }

    private void ShowBanner(string message)
    {
        if (regionDone)
        {
            return;
        }
        bannerText.text = message;
        bannerTimer = bannerSeconds;
    }

    private void Update()
    {
        if (regionDone)
        {
            bool restart =
                (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                || (Pointer.current != null && Pointer.current.press.wasPressedThisFrame);
            if (restart)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }

        if (bannerTimer > 0f)
        {
            bannerTimer -= Time.unscaledDeltaTime;
            if (bannerTimer <= 0f)
            {
                bannerText.text = "";
            }
        }

        // Barra do guardião acompanha a vida (barato: é só um número)
        if (zones.Guardian != null)
        {
            guardianFill.fillAmount = Mathf.Clamp01(zones.Guardian.CurrentHealth / zones.Guardian.MaxHealth);
        }

        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer > 0f)
        {
            return;
        }
        refreshTimer = 0.2f;

        int seconds = Mathf.CeilToInt(zones.TimeRemaining);
        string status = zones.GuardianActive ? "  GUARDIÃO" : "";
        timerText.text = string.Format("Zona {0}/{1}   {2:00}:{3:00}{4}",
            zones.ZoneIndex + 1, zones.ZoneCount, seconds / 60, seconds % 60, status);
        callGuardianButton.gameObject.SetActive(zones.CanCallGuardian);
    }
}
