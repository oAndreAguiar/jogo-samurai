using UnityEngine;

/// <summary>
/// Animações de um tipo de inimigo (andar, ataque, dano, morte).
/// Os sprites têm o pivô nos pés e o desenho olha para a DIREITA (espelha para a esquerda).
/// Ligue no campo "animations" do EnemyData.
/// </summary>
[CreateAssetMenu(menuName = "JogoJapones/Enemy Animation Set", fileName = "EnemyAnimationSet")]
public class EnemyAnimationSet : ScriptableObject
{
    [Tooltip("Parado (ex.: quando já está colado no jogador e não está atacando).")]
    public SpriteClip idle;
    public SpriteClip walk;
    [Tooltip("Toca quando encosta no jogador (e quando o boss solta o golpe).")]
    public SpriteClip attack;
    public SpriteClip hurt;
    [Tooltip("Toca uma vez ao morrer; o corpo some no fim.")]
    public SpriteClip death;

    [Tooltip("Retrato de frente (Manual de Criaturas).")]
    public Sprite portrait;

    [Tooltip("Tempo extra parado no último quadro da morte antes de sumir (segundos).")]
    public float corpseTime = 0.25f;
}
