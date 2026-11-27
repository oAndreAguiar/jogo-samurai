using UnityEngine;

/// <summary>
/// Quadros e velocidade das animações do personagem (idle, andar, ataque, dano, morte).
/// Os sprites vêm de um spritesheet fatiado; o lado padrão olha para a DIREITA.
/// </summary>
[CreateAssetMenu(menuName = "JogoJapones/Player/Animation Set", fileName = "PlayerAnimationSet")]
public class PlayerAnimationSet : ScriptableObject
{
    public SpriteClip idle;
    public SpriteClip walk;
    public SpriteClip attack;
    public SpriteClip hurt;
    public SpriteClip death;

    [Tooltip("Retrato de frente (HUD e menu).")]
    public Sprite portrait;

    [Tooltip("Força mínima do comando de movimento (0 a 1) para tocar a animação de andar.")]
    public float walkThreshold = 0.2f;
}
