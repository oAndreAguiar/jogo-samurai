using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Cuida de todos os projéteis: pool, movimento, giro e acerto nos inimigos.
/// </summary>
public class ProjectileSystem : MonoBehaviour
{
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private int poolPrewarm = 40;

    private readonly List<Projectile> active = new List<Projectile>();
    private ObjectPool<Projectile> pool;
    private Transform poolRoot;
    private Transform cameraTransform;

    private void Awake()
    {
        poolRoot = new GameObject("Projectiles").transform;
        pool = new ObjectPool<Projectile>(
            CreateProjectile,
            p => p.gameObject.SetActive(true),
            p => p.gameObject.SetActive(false),
            p => Destroy(p.gameObject),
            collectionCheck: false,
            defaultCapacity: poolPrewarm,
            maxSize: 500);

        var temp = new List<Projectile>();
        for (int i = 0; i < poolPrewarm; i++)
        {
            temp.Add(pool.Get());
        }
        foreach (Projectile p in temp)
        {
            pool.Release(p);
        }
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    /// <summary>Dispara um projétil da posição dada, na direção dada (no plano do chão).</summary>
    public void Fire(AbilityData ability, Vector3 position, Vector3 direction)
    {
        Projectile p = pool.Get();
        p.Launch(ability, position, direction.normalized);
        p.ActiveIndex = active.Count;
        active.Add(p);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        Quaternion faceCamera = cameraTransform != null ? cameraTransform.rotation : Quaternion.identity;
        IReadOnlyList<Enemy> enemies = spawner.ActiveEnemies;

        // De trás para frente, porque projéteis podem ser removidos no meio
        for (int i = active.Count - 1; i >= 0; i--)
        {
            Projectile p = active[i];
            p.TimeLeft -= dt;
            if (p.TimeLeft <= 0f)
            {
                Remove(p);
                continue;
            }

            Vector3 pos = p.transform.position + p.Velocity * dt;
            p.SpinAngle += p.Ability.spinSpeed * dt;
            p.transform.SetPositionAndRotation(pos, faceCamera * Quaternion.Euler(0f, 0f, p.SpinAngle));

            if (CheckHits(p, pos, enemies))
            {
                Remove(p);
            }
        }
    }

    // Devolve true quando o projétil acabou (não atravessa mais ninguém)
    private bool CheckHits(Projectile p, Vector3 pos, IReadOnlyList<Enemy> enemies)
    {
        float radius = p.Ability.hitRadius;
        // De trás para frente, porque um inimigo morto sai da lista
        for (int e = enemies.Count - 1; e >= 0; e--)
        {
            if (e >= enemies.Count)
            {
                continue;
            }
            Enemy enemy = enemies[e];
            Vector3 offset = enemy.CenterPosition - pos;
            if (Mathf.Abs(offset.y) > 1.2f)
            {
                continue; // muito acima ou abaixo (em cima de um bloco, por exemplo)
            }
            offset.y = 0f;
            if (offset.sqrMagnitude > radius * radius || p.HasHit(enemy))
            {
                continue;
            }

            p.RegisterHit(enemy);
            spawner.DamageEnemy(enemy, p.Ability.damage);
            if (p.PierceLeft <= 0)
            {
                return true;
            }
            p.PierceLeft--;
        }
        return false;
    }

    private void Remove(Projectile p)
    {
        int index = p.ActiveIndex;
        int last = active.Count - 1;
        active[index] = active[last];
        active[index].ActiveIndex = index;
        active.RemoveAt(last);
        p.ActiveIndex = -1;
        pool.Release(p);
    }

    private Projectile CreateProjectile()
    {
        GameObject go = new GameObject("Projectile");
        go.transform.SetParent(poolRoot, false);
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 50; // desenha por cima dos inimigos
        Projectile p = go.AddComponent<Projectile>();
        p.Setup(renderer);
        return p;
    }
}
