using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Habilidade automática do jogador: de tempos em tempos lança projéteis
/// no inimigo mais próximo dentro do alcance.
/// </summary>
public class AutoShooter : MonoBehaviour
{
    [SerializeField] private AbilityData ability;
    [SerializeField] private ProjectileSystem projectiles;
    [SerializeField] private HordeSpawner spawner;

    [Tooltip("Altura de onde sai o projétil, em relação ao pé do jogador.")]
    [SerializeField] private float launchHeight = 0.8f;

    private float cooldownTimer;

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
            return; // sem alvo: fica pronto para atirar assim que aparecer alguém
        }

        cooldownTimer = ability.cooldown;
        Vector3 origin = transform.position + Vector3.up * launchHeight;
        Vector3 direction = target.CenterPosition - origin;
        direction.y = 0f;

        int count = Mathf.Max(1, ability.projectilesPerShot);
        for (int i = 0; i < count; i++)
        {
            // Espalha em leque, centrado no alvo
            float angle = count == 1 ? 0f : Mathf.Lerp(-ability.spreadAngle, ability.spreadAngle, i / (count - 1f));
            projectiles.Fire(ability, origin, Quaternion.Euler(0f, angle, 0f) * direction);
        }
    }

    private Enemy FindNearestEnemy()
    {
        IReadOnlyList<Enemy> enemies = spawner.ActiveEnemies;
        Vector3 pos = transform.position;
        float bestSqr = ability.targetRange * ability.targetRange;
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
