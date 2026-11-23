using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla as zonas da região: timer, elites, guardião sorteado e a passagem de uma zona para a outra.
/// Depois da última zona avisa (AllZonesCleared) e o SanctuaryController assume.
/// Quando o timer zera, o guardião aparece sozinho. O jogador também pode chamá-lo antes (tecla G ou botão).
/// </summary>
public class ZoneController : MonoBehaviour
{
    [SerializeField] private RegionData region;
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private ExperienceGemSystem gems;
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("Segundos de pausa na horda entre uma zona e outra.")]
    [SerializeField] private float transitionSeconds = 4f;

    // (índice da zona, dados da zona, guardião sorteado)
    public event Action<int, ZoneData, EnemyData> ZoneStarted;
    public event Action<Enemy> GuardianSummoned;
    public event Action<Enemy> EliteSpawned;
    // índice da zona concluída
    public event Action<int> ZoneCleared;
    // Todas as zonas concluídas (o Santuário começa)
    public event Action AllZonesCleared;

    public int ZoneIndex { get; private set; }
    public RegionData Region => region;
    public int ZoneCount => region.zones.Count;
    public ZoneData CurrentZone => region.zones[ZoneIndex];
    public EnemyData ChosenGuardian { get; private set; }
    public Enemy Guardian { get; private set; }
    public float TimeRemaining { get; private set; }
    public bool GuardianActive => Guardian != null;
    public bool InTransition { get; private set; }
    public bool IsFinished { get; private set; }

    // Pode chamar o guardião agora?
    public bool CanCallGuardian =>
        !InTransition && !IsFinished && Guardian == null && !playerHealth.IsDead
        && CurrentZone.durationSeconds - TimeRemaining >= CurrentZone.callGuardianAfter;

    private float nextEliteTime;
    private int eliteWave;

    private void OnEnable()
    {
        spawner.EnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        spawner.EnemyKilled -= HandleEnemyKilled;
    }

    private void Start()
    {
        StartZone(0);
    }

    private void Update()
    {
        if (InTransition || IsFinished || playerHealth.IsDead)
        {
            return;
        }

        // O timer para quando o guardião já está em campo
        if (Guardian == null)
        {
            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                SummonGuardian();
            }
        }

        float elapsed = CurrentZone.durationSeconds - TimeRemaining;
        if (Guardian == null && elapsed >= nextEliteTime)
        {
            SpawnEliteWave();
        }

        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            CallGuardianEarly();
        }
    }

    /// <summary>Chamado pelo botão "Chamar guardião" e pela tecla G.</summary>
    public void CallGuardianEarly()
    {
        if (CanCallGuardian)
        {
            SummonGuardian();
        }
    }

    private void StartZone(int index)
    {
        ZoneIndex = index;
        ZoneData zone = CurrentZone;
        TimeRemaining = zone.durationSeconds;
        nextEliteTime = zone.firstEliteTime;
        eliteWave = 0;
        Guardian = null;

        // Sorteia o guardião desta zona (um por partida)
        ChosenGuardian = zone.guardianOptions.Count > 0
            ? zone.guardianOptions[UnityEngine.Random.Range(0, zone.guardianOptions.Count)]
            : null;

        spawner.Configure(zone.horde, zone.enemyHealthMultiplier);
        spawner.SpawningEnabled = true;
        ZoneStarted?.Invoke(ZoneIndex, zone, ChosenGuardian);
    }

    private void SpawnEliteWave()
    {
        ZoneData zone = CurrentZone;
        nextEliteTime += zone.eliteInterval;
        eliteWave++;
        if (zone.elites.Count == 0)
        {
            return;
        }
        int count = Mathf.Min(eliteWave, zone.maxElitesPerWave);
        for (int i = 0; i < count; i++)
        {
            EnemyData data = zone.elites[UnityEngine.Random.Range(0, zone.elites.Count)];
            EliteSpawned?.Invoke(spawner.SpawnSpecific(data));
        }
    }

    private void SummonGuardian()
    {
        if (Guardian != null || ChosenGuardian == null)
        {
            return;
        }
        Guardian = spawner.SpawnSpecific(ChosenGuardian);
        GuardianSummoned?.Invoke(Guardian);
    }

    private void HandleEnemyKilled(Enemy enemy)
    {
        if (enemy == Guardian)
        {
            Guardian = null;
            StartCoroutine(FinishZone());
        }
    }

    private IEnumerator FinishZone()
    {
        InTransition = true;
        spawner.SpawningEnabled = false;
        int cleared = ZoneIndex;
        ZoneCleared?.Invoke(cleared);

        // Recolhe as joias que sobraram e limpa a horda
        yield return new WaitForSeconds(1f);
        gems.CollectAll();
        spawner.DespawnAll();

        yield return new WaitForSeconds(transitionSeconds);
        InTransition = false;

        if (cleared + 1 < region.zones.Count)
        {
            StartZone(cleared + 1);
        }
        else
        {
            IsFinished = true;
            AllZonesCleared?.Invoke();
        }
    }
}
