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

    // ---- Animação (só quando o EnemyData tem "animations") ----
    private enum AnimState { Walk, Idle, Attack, Hurt, Death }
    private EnemyAnimationSet anims;
    private AnimState animState;
    private SpriteClip clip;
    private float clipTime;
    private Sprite shownSprite;

    // Está tocando a animação de morte (já saiu da lista de inimigos ativos)
    public bool IsDying => animState == AnimState.Death && anims != null;

    public void Setup(SpriteRenderer renderer)
    {
        spriteRenderer = renderer;
        // Ordem de desenho pelo pé do sprite: quem está atrás de uma casa fica escondido por ela
        spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
    }

    /// <summary>Prepara o inimigo para entrar em jogo (chamado ao sair do pool).</summary>
    public void Spawn(EnemyData data, Vector3 groundPosition, float healthMultiplier)
    {
        Data = data;
        anims = data.animations;
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
        shownSprite = null;
        if (anims != null)
        {
            Play(AnimState.Walk);
        }
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
        bool killed = CurrentHealth <= 0f;
        // Dano: só recomeça se não estiver no meio de um ataque ou de outro dano
        if (!killed && anims != null && animState != AnimState.Attack && animState != AnimState.Hurt)
        {
            Play(AnimState.Hurt);
        }
        return killed;
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
        // Sprites animados têm o pivô nos pés; os antigos, no centro
        Vector3 position = anims != null ? GroundPosition : CenterPosition;
        transform.SetPositionAndRotation(position, cameraRotation);
    }

    /// <summary>
    /// Avança a animação. "touchingPlayer": está colado no jogador (toca o ataque).
    /// "moving": está andando (senão fica parado).
    /// </summary>
    public void TickAnimation(float deltaTime, bool touchingPlayer, bool moving)
    {
        if (anims == null)
        {
            return;
        }
        clipTime += deltaTime;

        bool oneShotDone = clipTime >= clip.Duration;
        bool busy = (animState == AnimState.Attack || animState == AnimState.Hurt) && !oneShotDone;
        if (!busy)
        {
            if (touchingPlayer)
            {
                // Encostado: golpeia de novo a cada fim de ataque
                if (animState != AnimState.Attack || oneShotDone)
                {
                    Play(AnimState.Attack);
                }
            }
            else
            {
                AnimState wanted = moving ? AnimState.Walk : AnimState.Idle;
                if (wanted != animState)
                {
                    Play(wanted);
                }
            }
        }
        ShowFrame();
    }

    /// <summary>Toca o ataque agora (ex.: golpe especial do boss).</summary>
    public void PlayAttack()
    {
        if (anims != null && animState != AnimState.Death)
        {
            Play(AnimState.Attack);
        }
    }

    /// <summary>Começa a animação de morte. Devolve false se o inimigo não tem animação de morte.</summary>
    public bool StartDeath()
    {
        if (anims == null || anims.death.IsEmpty)
        {
            return false;
        }
        flashTimer = 0f;
        spriteRenderer.color = Data.tint;
        Play(AnimState.Death);
        return true;
    }

    /// <summary>Avança a morte. Devolve true quando terminou (pode voltar ao pool).</summary>
    public bool TickDeath(float deltaTime)
    {
        clipTime += deltaTime;
        ShowFrame();
        return clipTime >= clip.Duration + anims.corpseTime;
    }

    private void Play(AnimState state)
    {
        animState = state;
        clipTime = 0f;
        switch (state)
        {
            case AnimState.Idle: clip = anims.idle.IsEmpty ? anims.walk : anims.idle; break;
            case AnimState.Attack: clip = anims.attack; break;
            case AnimState.Hurt: clip = anims.hurt; break;
            case AnimState.Death: clip = anims.death; break;
            default: clip = anims.walk; break;
        }
        if (clip.IsEmpty)
        {
            clip = anims.walk;
        }
        ShowFrame();
    }

    private void ShowFrame()
    {
        Sprite sprite = clip.FrameAt(clipTime);
        // Só mexe no SpriteRenderer quando o quadro muda (mais barato com 300 inimigos)
        if (sprite != shownSprite)
        {
            shownSprite = sprite;
            spriteRenderer.sprite = sprite;
        }
    }

    public void SetFacingLeft(bool left)
    {
        spriteRenderer.flipX = left;
    }
}
