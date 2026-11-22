using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Faz nascer a horda ao redor do jogador, sempre fora da tela.
/// O ritmo aumenta com o tempo e, de tempos em tempos, vem uma onda maior.
/// Os inimigos são reaproveitados com ObjectPool (nada de Instantiate/Destroy durante o jogo).
/// </summary>
public class HordeSpawner : MonoBehaviour
{
    [SerializeField] private HordeSettings settings;
    [SerializeField] private Transform player;

    [Tooltip("Camadas consideradas chão (para os inimigos nascerem em cima dos blocos).")]
    [SerializeField] private LayerMask groundMask = ~0;

    // Avisos para outros sistemas (combate, contadores, etc.)
    public event Action<Enemy> EnemySpawned;
    public event Action<Enemy> EnemyDespawned;
    public event Action<int> WaveStarted;

    public IReadOnlyList<Enemy> ActiveEnemies => activeEnemies;
    public HordeSettings Settings => settings;
    public Transform Player => player;
    public float ElapsedTime { get; private set; }
    public int WaveIndex { get; private set; }

    private readonly List<Enemy> activeEnemies = new List<Enemy>();
    private ObjectPool<Enemy> pool;
    private Transform poolRoot;
    private Camera mainCamera;
    private float spawnAccumulator;
    private float nextWaveTime;
    private float totalWeight;

    private void Awake()
    {
        poolRoot = new GameObject("Enemies").transform;
        pool = new ObjectPool<Enemy>(
            CreateEnemy,
            enemy => enemy.gameObject.SetActive(true),
            enemy => enemy.gameObject.SetActive(false),
            enemy => Destroy(enemy.gameObject),
            collectionCheck: false,
            defaultCapacity: settings.poolPrewarm,
            maxSize: settings.maxAlive * 2);

        foreach (EnemyData data in settings.enemyTypes)
        {
            totalWeight += data.spawnWeight;
        }

        Prewarm();
        nextWaveTime = settings.waveInterval;
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        ElapsedTime += Time.deltaTime;

        // Nascimento contínuo: acumula "frações" de inimigo até dar um inteiro
        float minutes = ElapsedTime / 60f;
        spawnAccumulator += settings.spawnsPerSecond.Evaluate(minutes) * Time.deltaTime;
        while (spawnAccumulator >= 1f)
        {
            spawnAccumulator -= 1f;
            TrySpawn(UnityEngine.Random.Range(0f, 360f));
        }

        // Onda: um anel de inimigos de todos os lados ao mesmo tempo
        if (ElapsedTime >= nextWaveTime)
        {
            nextWaveTime += settings.waveInterval;
            WaveIndex++;
            int count = settings.waveBaseCount + settings.waveCountGrowth * (WaveIndex - 1);
            float offset = UnityEngine.Random.Range(0f, 360f);
            for (int i = 0; i < count; i++)
            {
                TrySpawn(offset + 360f * i / count);
            }
            WaveStarted?.Invoke(WaveIndex);
        }

        RecycleFarEnemies();
    }

    /// <summary>Tira um inimigo do jogo e devolve ao pool (morte, distância, fim de zona).</summary>
    public void Despawn(Enemy enemy)
    {
        int index = enemy.ActiveIndex;
        if (index < 0)
        {
            return;
        }

        // Remove trocando com o último da lista (rápido, sem deslocar a lista toda)
        int last = activeEnemies.Count - 1;
        Enemy moved = activeEnemies[last];
        activeEnemies[index] = moved;
        moved.ActiveIndex = index;
        activeEnemies.RemoveAt(last);
        enemy.ActiveIndex = -1;

        pool.Release(enemy);
        EnemyDespawned?.Invoke(enemy);
    }

    private bool TrySpawn(float angleDegrees)
    {
        if (activeEnemies.Count >= settings.maxAlive || settings.enemyTypes.Count == 0)
        {
            return false;
        }

        Vector3 position = FindSpawnPosition(angleDegrees);
        Enemy enemy = pool.Get();
        enemy.Spawn(PickEnemyType(), position);
        enemy.ActiveIndex = activeEnemies.Count;
        activeEnemies.Add(enemy);
        EnemySpawned?.Invoke(enemy);
        return true;
    }

    // Ponto no anel ao redor do jogador. Se cair dentro da tela, empurra mais para longe.
    private Vector3 FindSpawnPosition(float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians));
        float distance = settings.spawnDistance;
        Vector3 position = player.position + direction * distance;

        for (int i = 0; i < 4 && IsOnScreen(position); i++)
        {
            distance += 6f;
            position = player.position + direction * distance;
        }

        position.y = SampleGroundHeight(position);
        return position;
    }

    private bool IsOnScreen(Vector3 worldPosition)
    {
        if (mainCamera == null)
        {
            return false;
        }
        Vector3 viewport = mainCamera.WorldToViewportPoint(worldPosition);
        const float margin = 0.1f; // margem para o sprite não "aparecer do nada" na borda
        return viewport.z > 0f
            && viewport.x > -margin && viewport.x < 1f + margin
            && viewport.y > -margin && viewport.y < 1f + margin;
    }

    public float SampleGroundHeight(Vector3 position)
    {
        Vector3 origin = new Vector3(position.x, position.y + 20f, position.z);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 40f, groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point.y;
        }
        return 0f;
    }

    private EnemyData PickEnemyType()
    {
        float roll = UnityEngine.Random.Range(0f, totalWeight);
        foreach (EnemyData data in settings.enemyTypes)
        {
            roll -= data.spawnWeight;
            if (roll <= 0f)
            {
                return data;
            }
        }
        return settings.enemyTypes[settings.enemyTypes.Count - 1];
    }

    private void RecycleFarEnemies()
    {
        float maxSqr = settings.despawnDistance * settings.despawnDistance;
        Vector3 playerPos = player.position;
        // De trás para frente, porque Despawn mexe na lista
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            Vector3 offset = activeEnemies[i].GroundPosition - playerPos;
            offset.y = 0f;
            if (offset.sqrMagnitude > maxSqr)
            {
                Despawn(activeEnemies[i]);
            }
        }
    }

    private void Prewarm()
    {
        Enemy[] temp = new Enemy[settings.poolPrewarm];
        for (int i = 0; i < temp.Length; i++)
        {
            temp[i] = pool.Get();
        }
        for (int i = 0; i < temp.Length; i++)
        {
            pool.Release(temp[i]);
        }
    }

    private Enemy CreateEnemy()
    {
        GameObject go = new GameObject("Enemy");
        go.transform.SetParent(poolRoot, false);
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        Enemy enemy = go.AddComponent<Enemy>();
        enemy.Setup(renderer);
        return enemy;
    }
}
