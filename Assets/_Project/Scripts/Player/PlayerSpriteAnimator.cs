using UnityEngine;

/// <summary>
/// Troca os quadros do sprite do jogador conforme o estado: parado, andando,
/// atacando, tomando dano ou morto. Também vira o sprite para a esquerda/direita.
/// Fica no objeto "Sprite" (filho do Player), ao lado do SpriteRenderer e do Billboard.
/// </summary>
public class PlayerSpriteAnimator : MonoBehaviour
{
    private enum State { Idle, Walk, Attack, Hurt, Death }

    [SerializeField] private PlayerAnimationSet animations;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerHealth health;
    [SerializeField] private PlayerLoadout loadout;

    private State state = State.Idle;
    private SpriteClip clip;
    private float clipTime;
    private int lastFrame = -1;

    private void OnEnable()
    {
        if (health != null)
        {
            health.Damaged += HandleDamaged;
            health.Died += HandleDied;
        }
        if (loadout != null)
        {
            loadout.Attacked += HandleAttacked;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Damaged -= HandleDamaged;
            health.Died -= HandleDied;
        }
        if (loadout != null)
        {
            loadout.Attacked -= HandleAttacked;
        }
    }

    private void Start()
    {
        Play(State.Idle);
    }

    private void Update()
    {
        if (animations == null || spriteRenderer == null)
        {
            return;
        }

        // A morte usa tempo real, porque a tela de derrota pausa o jogo (timeScale = 0)
        float dt = state == State.Death ? Time.unscaledDeltaTime : Time.deltaTime;
        clipTime += dt;

        Vector2 input = movement != null ? movement.MoveInput : Vector2.zero;

        // Vira para o lado em que está andando (o desenho original olha para a direita)
        if (state != State.Death && Mathf.Abs(input.x) > 0.01f)
        {
            spriteRenderer.flipX = input.x < 0f;
        }

        // Ataque e dano tocam uma vez e depois voltam para parado/andando
        bool oneShotRunning = (state == State.Attack || state == State.Hurt) && clipTime < clip.Duration;
        if (state != State.Death && !oneShotRunning)
        {
            bool walking = input.sqrMagnitude > animations.walkThreshold * animations.walkThreshold;
            State wanted = walking ? State.Walk : State.Idle;
            if (wanted != state)
            {
                Play(wanted);
            }
        }

        ShowFrame();
    }

    private void HandleAttacked()
    {
        if (state == State.Death || state == State.Hurt)
        {
            return;
        }
        Play(State.Attack);
    }

    private void HandleDamaged()
    {
        if (state != State.Death)
        {
            Play(State.Hurt);
        }
    }

    private void HandleDied()
    {
        Play(State.Death);
    }

    private void Play(State newState)
    {
        state = newState;
        clipTime = 0f;
        lastFrame = -1;
        clip = GetClip(newState);
        ShowFrame();
    }

    private SpriteClip GetClip(State s)
    {
        switch (s)
        {
            case State.Walk: return animations.walk;
            case State.Attack: return animations.attack;
            case State.Hurt: return animations.hurt;
            case State.Death: return animations.death;
            default: return animations.idle;
        }
    }

    private void ShowFrame()
    {
        if (clip == null || clip.frames == null || clip.frames.Length == 0)
        {
            return;
        }

        int frame = Mathf.FloorToInt(clipTime * clip.fps);
        frame = clip.loop ? frame % clip.frames.Length : Mathf.Min(frame, clip.frames.Length - 1);

        // Só troca o sprite quando o quadro muda
        if (frame != lastFrame)
        {
            lastFrame = frame;
            spriteRenderer.sprite = clip.frames[frame];
        }
    }
}
