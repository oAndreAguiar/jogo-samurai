using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Vida do jogador: toma dano quando um inimigo encosta, regenera com o tempo
/// e avisa (por eventos) quando a vida muda e quando morre.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // (vida atual, vida máxima)
    public event Action<float, float> HealthChanged;
    public event Action Died;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => stats.maxHealth + bonusMaxHealth;
    public bool IsDead { get; private set; }

    private float invulnerableTimer;
    private float bonusMaxHealth;

    private void Start()
    {
        CurrentHealth = MaxHealth;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private void Update()
    {
        if (IsDead)
        {
            return;
        }

        invulnerableTimer -= Time.deltaTime;

        // Pisca o sprite enquanto está invulnerável
        if (spriteRenderer != null)
        {
            bool blink = invulnerableTimer > 0f && Mathf.FloorToInt(invulnerableTimer * 20f) % 2 == 0;
            spriteRenderer.color = blink ? new Color(1f, 0.4f, 0.4f) : Color.white;
        }

        if (invulnerableTimer <= 0f)
        {
            float damage = FindStrongestContact();
            if (damage > 0f)
            {
                TakeDamage(damage);
                return;
            }
        }

        if (CurrentHealth < MaxHealth && stats.regenPerSecond > 0f)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + stats.regenPerSecond * Time.deltaTime);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }

    /// <summary>Fica invulnerável por um tempo (ex.: durante o avanço).</summary>
    public void SetInvulnerable(float seconds)
    {
        invulnerableTimer = Mathf.Max(invulnerableTimer, seconds);
    }

    /// <summary>Recupera vida (sem passar do máximo).</summary>
    public void Heal(float amount)
    {
        if (IsDead)
        {
            return;
        }
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    /// <summary>Muda o bônus de vida máxima (passivas). O aumento também cura.</summary>
    public void SetBonusMaxHealth(float bonus)
    {
        float gained = bonus - bonusMaxHealth;
        bonusMaxHealth = bonus;
        CurrentHealth = Mathf.Clamp(CurrentHealth + Mathf.Max(0f, gained), 0f, MaxHealth);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        invulnerableTimer = stats.invulnerabilityTime;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0f)
        {
            IsDead = true;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.4f, 0.4f, 0.4f);
            }
            Died?.Invoke();
        }
    }

    // Maior dano de contato entre os inimigos encostados no jogador (0 se nenhum)
    private float FindStrongestContact()
    {
        IReadOnlyList<Enemy> enemies = spawner.ActiveEnemies;
        Vector3 pos = transform.position;
        float strongest = 0f;
        for (int i = 0; i < enemies.Count; i++)
        {
            Vector3 offset = enemies[i].GroundPosition - pos;
            if (Mathf.Abs(offset.y) > 1f)
            {
                continue;
            }
            offset.y = 0f;
            // Inimigos grandes encostam de mais longe
            float reach = stats.contactRadius + enemies[i].BodyRadius - 0.25f;
            if (offset.sqrMagnitude <= reach * reach && enemies[i].Data.contactDamage > strongest)
            {
                strongest = enemies[i].Data.contactDamage;
            }
        }
        return strongest;
    }
}
