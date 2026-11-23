using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Objetos girando em volta do jogador que ferem os inimigos que encostam (ex.: Ofuda).
/// Os objetos são criados só quando o nível muda (nunca por frame).
/// </summary>
public class OrbitAbility : AbilityBehaviour
{
    private const float OrbitHeight = 0.7f;

    private OrbitAbilityDefinition data;
    private readonly List<Transform> orbiters = new List<Transform>();
    private float angle;
    private Transform cameraTransform;

    protected override void OnInitialize()
    {
        data = (OrbitAbilityDefinition)Definition;
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    protected override void OnLevelChanged()
    {
        int wanted = data.levels[Level - 1].count;
        while (orbiters.Count < wanted)
        {
            GameObject go = new GameObject("Orbiter");
            go.transform.SetParent(transform, false);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = data.orbiterSprite;
            sr.sortingOrder = 45;
            orbiters.Add(go.transform);
        }
    }

    private void OnDestroy()
    {
        // Some com os talismãs junto com a habilidade (ex.: depois de uma fusão)
        foreach (Transform orbiter in orbiters)
        {
            if (orbiter != null)
            {
                Destroy(orbiter.gameObject);
            }
        }
    }

    private void Update()
    {
        OrbitAbilityDefinition.Level stats = data.levels[Level - 1];
        float dt = Time.deltaTime;
        angle += stats.rotationSpeed * dt;

        Quaternion faceCamera = cameraTransform != null ? cameraTransform.rotation : Quaternion.identity;
        Vector3 center = transform.position + Vector3.up * OrbitHeight;
        float damage = stats.damage * Modifiers.DamageMultiplier;
        IReadOnlyList<Enemy> enemies = Spawner.ActiveEnemies;
        float now = Time.time;

        for (int o = 0; o < orbiters.Count; o++)
        {
            // Distribui os objetos igualmente no círculo
            float a = (angle + 360f * o / orbiters.Count) * Mathf.Deg2Rad;
            Vector3 pos = center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * stats.radius;
            orbiters[o].SetPositionAndRotation(pos, faceCamera);

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (i >= enemies.Count)
                {
                    continue;
                }
                Enemy enemy = enemies[i];
                if (enemy.OrbitImmuneUntil > now)
                {
                    continue;
                }
                Vector3 offset = enemy.CenterPosition - pos;
                if (Mathf.Abs(offset.y) > 1.2f + enemy.BodyRadius)
                {
                    continue;
                }
                offset.y = 0f;
                float reach = data.hitRadius + enemy.BodyRadius;
                if (offset.sqrMagnitude <= reach * reach)
                {
                    enemy.OrbitImmuneUntil = now + data.hitInterval;
                    Spawner.DamageEnemy(enemy, damage);
                }
            }
        }
    }
}
