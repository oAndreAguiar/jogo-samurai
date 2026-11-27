using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// De tempos em tempos acerta todos os inimigos num círculo ao redor do jogador
/// e mostra um anel no chão (ex.: Corte de Katana).
/// </summary>
public class AuraAbility : AbilityBehaviour
{
    private AuraAbilityDefinition data;
    private SpriteRenderer effect;
    private float cooldownTimer;
    private float effectTimer;

    protected override void OnInitialize()
    {
        data = (AuraAbilityDefinition)Definition;

        // Anel deitado no chão, filho do jogador
        GameObject go = new GameObject("AuraEffect");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0.06f, 0f);
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        effect = go.AddComponent<SpriteRenderer>();
        effect.sprite = data.effectSprite;
        effect.sortingOrder = 40;
        effect.enabled = false;
    }

    private void OnDestroy()
    {
        if (effect != null)
        {
            Destroy(effect.gameObject);
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        AuraAbilityDefinition.Level stats = data.levels[Level - 1];

        if (effectTimer > 0f)
        {
            effectTimer -= dt;
            float t = 1f - effectTimer / data.effectDuration;
            Color c = data.effectColor;
            c.a *= 1f - t;
            effect.color = c;
            // Anel cresce um pouco enquanto some
            float size = stats.radius * 2f * Mathf.Lerp(0.85f, 1.05f, t);
            effect.transform.localScale = new Vector3(size, size, 1f) / SpriteWorldSize();
            effect.enabled = effectTimer > 0f;
        }

        cooldownTimer -= dt;
        if (cooldownTimer > 0f)
        {
            return;
        }
        cooldownTimer = stats.cooldown * Modifiers.CooldownMultiplier;
        Strike(stats);
    }

    private void Strike(AuraAbilityDefinition.Level stats)
    {
        effectTimer = data.effectDuration;
        effect.enabled = true;
        Loadout.NotifyAttack();

        float damage = stats.damage * Modifiers.DamageMultiplier;
        Vector3 pos = transform.position;
        IReadOnlyList<Enemy> enemies = Spawner.ActiveEnemies;

        // De trás para frente, porque inimigos mortos saem da lista
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (i >= enemies.Count)
            {
                continue;
            }
            Enemy enemy = enemies[i];
            Vector3 offset = enemy.GroundPosition - pos;
            if (Mathf.Abs(offset.y) > 1.5f)
            {
                continue;
            }
            offset.y = 0f;
            float reach = stats.radius + enemy.BodyRadius;
            if (offset.sqrMagnitude <= reach * reach)
            {
                Spawner.DamageEnemy(enemy, damage);
            }
        }
    }

    // Tamanho do sprite em metros com escala 1 (para escalar o anel até o raio certo)
    private float SpriteWorldSize()
    {
        return data.effectSprite != null ? data.effectSprite.bounds.size.x : 1f;
    }
}
