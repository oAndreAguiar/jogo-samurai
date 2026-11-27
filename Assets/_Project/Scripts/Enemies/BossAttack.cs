using UnityEngine;

/// <summary>
/// Ataque do boss: de tempos em tempos marca um anel vermelho no chão em volta dele,
/// que cresce como aviso. Quando o anel fecha, quem estiver dentro leva dano.
/// Abaixo de uma fração da vida o boss fica furioso: anda e ataca mais rápido.
/// </summary>
public class BossAttack : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SpriteRenderer warningRing;

    private Enemy boss;
    private RegionData region;
    private float attackTimer;
    private float warningTimer = -1f;
    private Vector3 attackCenter;
    private float flashTimer;

    public bool IsEnraged { get; private set; }

    private void Awake()
    {
        warningRing.enabled = false;
    }

    /// <summary>Começa a atacar com o boss dado.</summary>
    public void Begin(Enemy bossEnemy, RegionData regionData)
    {
        boss = bossEnemy;
        region = regionData;
        attackTimer = region.bossAttackInterval;
        IsEnraged = false;
        enabled = true;
    }

    public void End()
    {
        boss = null;
        warningRing.enabled = false;
        enabled = false;
    }

    private void Update()
    {
        if (boss == null || !boss.IsAlive || playerHealth.IsDead)
        {
            warningRing.enabled = false;
            return;
        }

        if (!IsEnraged && boss.CurrentHealth <= boss.MaxHealth * region.enrageHealthFraction)
        {
            IsEnraged = true;
            boss.SpeedMultiplier = region.enrageMultiplier;
        }

        float dt = Time.deltaTime;

        if (warningTimer >= 0f)
        {
            UpdateWarning(dt);
            return;
        }

        if (flashTimer > 0f)
        {
            flashTimer -= dt;
            warningRing.color = new Color(1f, 1f, 1f, Mathf.Clamp01(flashTimer / 0.15f));
            if (flashTimer <= 0f)
            {
                warningRing.enabled = false;
            }
        }

        attackTimer -= dt;
        if (attackTimer <= 0f)
        {
            float interval = region.bossAttackInterval / (IsEnraged ? region.enrageMultiplier : 1f);
            attackTimer = interval;
            StartWarning();
        }
    }

    private void StartWarning()
    {
        warningTimer = 0f;
        attackCenter = boss.GroundPosition;
        warningRing.enabled = true;
        warningRing.transform.SetPositionAndRotation(attackCenter + Vector3.up * 0.07f, Quaternion.Euler(90f, 0f, 0f));
    }

    private void UpdateWarning(float dt)
    {
        warningTimer += dt;
        float t = Mathf.Clamp01(warningTimer / region.bossAttackWarning);

        // Anel cresce até o raio do golpe e pisca mais rápido no final
        float size = region.bossAttackRadius * 2f * Mathf.Lerp(0.2f, 1f, t) / SpriteSize();
        warningRing.transform.localScale = new Vector3(size, size, 1f);
        float blink = 0.5f + 0.5f * Mathf.Sin(warningTimer * Mathf.Lerp(8f, 30f, t));
        warningRing.color = new Color(1f, 0.15f, 0.1f, Mathf.Lerp(0.4f, 0.9f, blink));

        if (t >= 1f)
        {
            warningTimer = -1f;
            Strike();
        }
    }

    private void Strike()
    {
        boss.PlayAttack();
        Vector3 offset = playerHealth.transform.position - attackCenter;
        offset.y = 0f;
        if (offset.magnitude <= region.bossAttackRadius)
        {
            playerHealth.TakeDamage(region.bossAttackDamage);
        }
        flashTimer = 0.15f;
        warningRing.color = Color.white;
    }

    private float SpriteSize()
    {
        return warningRing.sprite != null ? warningRing.sprite.bounds.size.x : 1f;
    }
}
