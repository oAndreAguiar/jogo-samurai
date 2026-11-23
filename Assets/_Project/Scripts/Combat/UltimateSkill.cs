using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Ultimate provisória (tecla C): carrega com abates e, quando cheia,
/// acerta todos os inimigos em volta do jogador, com um clarão na tela.
/// </summary>
public class UltimateSkill : PlayerSkill
{
    [SerializeField] private SkillData data;
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private PlayerHealth health;
    [SerializeField] private PlayerLoadout loadout;

    [Tooltip("Imagem de tela cheia que pisca ao usar (opcional).")]
    [SerializeField] private Image flash;

    private int charge;
    private bool firing; // abates causados pela própria ultimate não recarregam ela
    private float flashTimer;
    private const float FlashTime = 0.35f;

    public override string DisplayName => data.displayName;
    public override Sprite Icon => data.icon;
    public override float Readiness => data.killsToCharge <= 0 ? 1f : Mathf.Clamp01((float)charge / data.killsToCharge);

    private void OnEnable()
    {
        spawner.EnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        spawner.EnemyKilled -= HandleEnemyKilled;
    }

    private void HandleEnemyKilled(Enemy enemy)
    {
        if (!firing && charge < data.killsToCharge)
        {
            charge++;
        }
    }

    private void Update()
    {
        if (flash != null && flashTimer > 0f)
        {
            flashTimer -= Time.unscaledDeltaTime;
            Color c = flash.color;
            c.a = Mathf.Clamp01(flashTimer / FlashTime) * 0.7f;
            flash.color = c;
            flash.enabled = flashTimer > 0f;
        }

        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame && Time.timeScale > 0f)
        {
            TryActivate();
        }
    }

    public override bool TryActivate()
    {
        if (!IsReady || health.IsDead || Time.timeScale <= 0f)
        {
            return false;
        }
        charge = 0;
        firing = true;

        float damage = data.damage * (loadout != null ? loadout.Modifiers.DamageMultiplier : 1f);
        float rangeSqr = data.range * data.range;
        Vector3 pos = transform.position;
        IReadOnlyList<Enemy> enemies = spawner.ActiveEnemies;
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (i >= enemies.Count)
            {
                continue;
            }
            Vector3 offset = enemies[i].GroundPosition - pos;
            offset.y = 0f;
            if (offset.sqrMagnitude <= rangeSqr)
            {
                spawner.DamageEnemy(enemies[i], damage);
            }
        }

        firing = false;

        if (flash != null)
        {
            flashTimer = FlashTime;
            flash.enabled = true;
        }
        return true;
    }
}
