using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Santuário: a etapa final da região. Depois da última zona, fecha uma arena,
/// muda a luz e traz só o boss (sem horda e sem loot). Matar o boss é a vitória.
/// </summary>
public class SanctuaryController : MonoBehaviour
{
    [SerializeField] private ZoneController zones;
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private ArenaBoundary arena;
    [SerializeField] private BossAttack bossAttack;
    [SerializeField] private Transform player;
    [SerializeField] private Light mainLight;

    [Tooltip("Segundos entre o aviso do Santuário e a chegada do boss.")]
    [SerializeField] private float bossDelay = 2.5f;

    public event Action SanctuaryStarted;
    public event Action<Enemy> BossSpawned;
    public event Action Victory;

    public bool IsActive { get; private set; }
    public Enemy Boss { get; private set; }
    public bool IsWon { get; private set; }

    private void OnEnable()
    {
        zones.AllZonesCleared += HandleAllZonesCleared;
        spawner.EnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        zones.AllZonesCleared -= HandleAllZonesCleared;
        spawner.EnemyKilled -= HandleEnemyKilled;
    }

    private void HandleAllZonesCleared()
    {
        StartCoroutine(StartSanctuary());
    }

    private IEnumerator StartSanctuary()
    {
        RegionData region = zones.Region;
        IsActive = true;

        // Sem horda: só o boss
        spawner.SpawningEnabled = false;
        spawner.DespawnAll();
        spawner.Configure(spawner.Settings, 1f); // vida do boss sem multiplicador de zona

        arena.Activate(player.position, region.arenaRadius, region.arenaColor);
        if (mainLight != null)
        {
            mainLight.color = region.sanctuaryLightColor;
        }
        RenderSettings.ambientLight = region.sanctuaryAmbientColor;

        SanctuaryStarted?.Invoke();
        yield return new WaitForSeconds(bossDelay);

        if (region.boss != null)
        {
            Boss = spawner.SpawnSpecific(region.boss);
            // Coloca o boss dentro da arena, do lado oposto à câmera, para entrar em cena
            Vector3 spawnPos = arena.Center + Vector3.forward * (region.arenaRadius * 0.7f);
            spawnPos.y = spawner.SampleGroundHeight(spawnPos);
            Boss.GroundPosition = spawnPos;
            bossAttack.Begin(Boss, region);
            BossSpawned?.Invoke(Boss);
        }
    }

    private void HandleEnemyKilled(Enemy enemy)
    {
        if (IsActive && enemy == Boss)
        {
            Boss = null;
            IsWon = true;
            bossAttack.End();
            Victory?.Invoke();
        }
    }
}
