using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Atira projéteis no inimigo mais próximo, de tempos em tempos (ex.: Shuriken).
/// </summary>
public class ProjectileAbility : AbilityBehaviour
{
    private const float LaunchHeight = 0.8f;

    private ProjectileAbilityDefinition data;
    private float cooldownTimer;

    protected override void OnInitialize()
    {
        data = (ProjectileAbilityDefinition)Definition;
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f)
        {
            return;
        }

        Enemy target = FindNearestEnemy();
        if (target == null)
        {
            return; // sem alvo: atira assim que alguém aparecer
        }

        ProjectileAbilityDefinition.Level stats = data.levels[Level - 1];
        cooldownTimer = stats.cooldown * Modifiers.CooldownMultiplier;

        Vector3 origin = transform.position + Vector3.up * LaunchHeight;
        Vector3 direction = target.CenterPosition - origin;
        direction.y = 0f;

        ProjectileShot shot = new ProjectileShot
        {
            sprite = data.projectileSprite,
            position = origin,
            speed = data.projectileSpeed,
            damage = stats.damage * Modifiers.DamageMultiplier,
            pierce = stats.pierce,
            hitRadius = data.hitRadius,
            lifetime = data.projectileLifetime,
            spinSpeed = data.spinSpeed
        };

        int count = Mathf.Max(1, stats.projectiles);
        for (int i = 0; i < count; i++)
        {
            // Espalha em leque, centrado no alvo
            float angle = (i - (count - 1) * 0.5f) * data.spreadAngle;
            shot.direction = Quaternion.Euler(0f, angle, 0f) * direction;
            Loadout.Projectiles.Fire(shot);
        }
    }

    private Enemy FindNearestEnemy()
    {
        IReadOnlyList<Enemy> enemies = Spawner.ActiveEnemies;
        Vector3 pos = transform.position;
        float bestSqr = data.targetRange * data.targetRange;
        Enemy best = null;
        for (int i = 0; i < enemies.Count; i++)
        {
            Vector3 offset = enemies[i].GroundPosition - pos;
            offset.y = 0f;
            float dSqr = offset.sqrMagnitude;
            if (dSqr < bestSqr)
            {
                bestSqr = dSqr;
                best = enemies[i];
            }
        }
        return best;
    }
}
