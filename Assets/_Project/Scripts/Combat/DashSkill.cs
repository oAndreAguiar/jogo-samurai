using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Habilidade ativa provisória (tecla R): avanço rápido na direção em que o jogador
/// está andando, com invulnerabilidade curta.
/// </summary>
public class DashSkill : PlayerSkill
{
    [SerializeField] private SkillData data;
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerHealth health;
    [SerializeField] private SpriteRenderer playerSprite;

    private float cooldownTimer;
    private float dashTimer;
    private Vector3 dashDirection = Vector3.forward;
    private Vector3 lastPosition;

    public override string DisplayName => data.displayName;
    public override Sprite Icon => data.icon;
    public override float Readiness => data.cooldown <= 0f ? 1f : 1f - Mathf.Clamp01(cooldownTimer / data.cooldown);

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        // Direção do último movimento (para avançar para onde está indo)
        Vector3 moved = transform.position - lastPosition;
        moved.y = 0f;
        if (moved.sqrMagnitude > 0.0001f && dashTimer <= 0f)
        {
            dashDirection = moved.normalized;
        }
        lastPosition = transform.position;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (dashTimer > 0f)
        {
            float dt = Mathf.Min(Time.deltaTime, dashTimer);
            dashTimer -= dt;
            controller.Move(dashDirection * (data.dashDistance / data.dashDuration) * dt);
        }

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame && Time.timeScale > 0f)
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
        cooldownTimer = data.cooldown;
        dashTimer = data.dashDuration;
        health.SetInvulnerable(data.invulnerability);
        return true;
    }
}
