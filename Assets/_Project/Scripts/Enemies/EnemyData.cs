using UnityEngine;

/// <summary>Categoria do inimigo dentro da zona.</summary>
public enum EnemyRank
{
    Common,     // horda comum
    Elite,      // mini boss que aparece ao longo da zona
    Guardian,   // mini boss que encerra a zona
    Boss        // chefe do santuário (etapa 6)
}

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

    public EnemyRank rank = EnemyRank.Common;

    [Tooltip("Tamanho do sprite (1 = normal). Elites e guardiões são maiores.")]
    public float scale = 1f;

    [Tooltip("Cor aplicada ao sprite (branco = cores originais).")]
    public Color tint = Color.white;

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

    [Header("Recompensa")]
    [Tooltip("Experiência da joia que o inimigo derruba ao morrer.")]
    public int xpValue = 1;

    [Tooltip("Moedas que o inimigo dá ao morrer.")]
    public int coinValue = 1;

    [Tooltip("Chance de dar as moedas (0 a 1). Inimigos comuns: baixa; elites e chefes: 1.")]
    [Range(0f, 1f)]
    public float coinChance = 0.05f;
}
