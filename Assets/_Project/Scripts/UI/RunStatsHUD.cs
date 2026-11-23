using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Linha de números no topo central: tempo total, recorde, moedas e abates.
/// Atualiza poucas vezes por segundo (texto custa caro para gerar todo frame).
/// </summary>
public class RunStatsHUD : MonoBehaviour
{
    [SerializeField] private RunClock clock;
    [SerializeField] private RunResults results;
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private ZoneController zones;
    [SerializeField] private Text statsText;
    [SerializeField] private float refreshInterval = 0.25f;

    private float timer;
    private string recordText;

    private void Start()
    {
        RegionRecord record = PlayerProfile.GetRecord(zones.Region.name);
        recordText = record != null && record.bestTimeSeconds > 0f ? RunClock.Format(record.bestTimeSeconds) : "--:--";
    }

    private void Update()
    {
        timer -= Time.unscaledDeltaTime;
        if (timer > 0f)
        {
            return;
        }
        timer = refreshInterval;
        statsText.text = string.Format("Total {0}    Recorde {1}    Moedas {2}    Abates {3}",
            RunClock.Format(clock.TotalTime), recordText, results.Coins, spawner.KillCount);
    }
}
