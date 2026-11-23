using UnityEngine;

/// <summary>
/// Um inimigo da horda. Ele não tem Update próprio: quem move todos de uma vez
/// é o EnemyMovementSystem, o que é bem mais rápido com centenas de inimigos.
/// Os objetos são reaproveitados pelo pool do HordeSpawner.
/// </summary>
public class Enemy : MonoBehaviour
{
    private const float FlashDuration = 0.1f;
    private const float BaseBodyRadius = 0.25f;
    private static readonly Color FlashColor = new Color(1f, 0.45f, 0.45f);

    [SerializeField] private SpriteRenderer spriteRenderer;

    public EnemyData Data { get; private set; }

    // Altura do chão embaixo do inimigo (atualizada aos poucos pelo sistema de movimento)
    public float GroundHeight { get; set; }

    // Posição no chão (sem a altura do sprite). O sprite fica acima disso.
    public Vector3 GroundPosition { get; set; }

    // Posição na lista de inimigos ativos do HordeSpawner (para remover rápido)
    public int ActiveIndex { get; set; } = -1;

    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public bool IsAlive => ActiveIndex >= 0 && CurrentHealth > 0f;

    // Raio do "corpo" (cresce com o tamanho do sprite): usado nos acertos e no contato
    public float BodyRadius => BaseBodyRadius * Data.scale;

    // Ponto no meio do sprite (para mirar e para os números de dano)
    public Vector3 CenterPosition => GroundPosition + Vector3.up * (Data.spriteHeight * Data.scale);

    // Até quando os objetos orbitando não podem acertar este inimigo de novo
    public float OrbitImmuneUntil { get; set; }

    // Multiplicador de velocidade (ex.: boss furioso). Volta a 1 ao renascer.
    public float SpeedMultiplier { get; set; } = 1f;

    private float flashTimer;

    public void Setup(SpriteRenderer renderer)
    {
        spriteRenderer = renderer;
    }

    /// <summary>Prepara o inimigo para entrar em jogo (chamado ao sair do pool).</summary>
    public void Spawn(EnemyData data, Vector3 groundPosition, float healthMultiplier)
    {
        Data = data;
        spriteRenderer.sprite = data.sprite;
        spriteRenderer.color = data.tint;
        // Guardiões e chefes desenham por cima da horda
        spriteRenderer.sortingOrder = data.rank == EnemyRank.Common ? 0 : 10;
        transform.localScale = Vector3.one * data.scale;
        flashTimer = 0f;
        OrbitImmuneUntil = 0f;
        SpeedMultiplier = 1f;
        MaxHealth = data.maxHealth * healthMultiplier;
        CurrentHealth = MaxHealth;
        GroundHeight = groundPosition.y;
        GroundPosition = groundPosition;
        ApplyTransform(Quaternion.identity);
    }

    /// <summary>Tira vida e pisca. Devolve true se o inimigo morreu com esse golpe.</summary>
    public bool ApplyDamage(float amount)
    {
        if (CurrentHealth <= 0f)
        {
            return false;
        }
        CurrentHealth -= amount;
        flashTimer = FlashDuration;
        spriteRenderer.color = FlashColor;
        return CurrentHealth <= 0f;
    }

    /// <summary>Chamado todo frame pelo sistema de movimento para apagar o piscar.</summary>
    public void TickFlash(float deltaTime)
    {
        if (flashTimer <= 0f)
        {
            return;
        }
        flashTimer -= deltaTime;
        if (flashTimer <= 0f)
        {
            spriteRenderer.color = Data.tint;
        }
    }

    /// <summary>Copia a posição no chão para o transform, com o sprite virado para a câmera.</summary>
    public void ApplyTransform(Quaternion cameraRotation)
    {
        transform.SetPositionAndRotation(CenterPosition, cameraRotation);
    }

    public void SetFacingLeft(bool left)
    {
        spriteRenderer.flipX = left;
    }
}
