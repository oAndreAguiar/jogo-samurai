using UnityEngine;

/// <summary>
/// Dados de um tipo de inimigo (visual e balanceamento).
/// Cada tipo de inimigo é um asset criado em Assets/_Project/ScriptableObjects/Enemies.
/// </summary>
[CreateAssetMenu(fileName = "EnemyData", menuName = "JogoJapones/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Tooltip("Nome mostrado em telas e no editor.")]
    public string displayName = "Inimigo";

    [Tooltip("Sprite em pixel art do inimigo.")]
    public Sprite sprite;

    [Tooltip("Velocidade em metros por segundo. O jogador anda a 5.")]
    public float moveSpeed = 2.5f;

    [Tooltip("Chance relativa de nascer. Peso 3 nasce 3x mais que peso 1.")]
    public float spawnWeight = 1f;

    [Tooltip("Altura do centro do sprite em relação ao chão, em metros.")]
    public float spriteHeight = 0.6f;

    [Header("Combate")]
    [Tooltip("Vida máxima do inimigo.")]
    public float maxHealth = 20f;

    [Tooltip("Dano causado ao jogador quando encosta nele.")]
    public float contactDamage = 10f;
}
