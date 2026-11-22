using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Balanceamento da horda: quais inimigos, quantos por segundo, ondas e limites.
/// </summary>
[CreateAssetMenu(fileName = "HordeSettings", menuName = "JogoJapones/Horde Settings")]
public class HordeSettings : ScriptableObject
{
    [Header("Inimigos")]
    [Tooltip("Tipos de inimigo que podem nascer (sorteados pelo peso de cada um).")]
    public List<EnemyData> enemyTypes = new List<EnemyData>();

    [Header("Ritmo")]
    [Tooltip("Inimigos por segundo (eixo Y) conforme os minutos de partida (eixo X).")]
    public AnimationCurve spawnsPerSecond = new AnimationCurve(
        new Keyframe(0f, 1f), new Keyframe(2f, 4f), new Keyframe(5f, 10f), new Keyframe(10f, 18f));

    [Tooltip("Segundos entre uma onda e outra.")]
    public float waveInterval = 30f;

    [Tooltip("Inimigos na primeira onda.")]
    public int waveBaseCount = 8;

    [Tooltip("Quantos inimigos a mais em cada onda seguinte.")]
    public int waveCountGrowth = 6;

    [Header("Limites")]
    [Tooltip("Máximo de inimigos vivos ao mesmo tempo.")]
    public int maxAlive = 300;

    [Tooltip("Inimigos criados já no início, para não travar quando a horda cresce.")]
    public int poolPrewarm = 150;

    [Header("Distâncias (metros)")]
    [Tooltip("Distância do jogador em que os inimigos nascem (sempre fora da tela).")]
    public float spawnDistance = 22f;

    [Tooltip("Inimigos mais longe que isso voltam para o pool e renascem perto.")]
    public float despawnDistance = 45f;

    [Tooltip("Os inimigos param a esta distância do jogador.")]
    public float stopDistance = 0.6f;

    [Header("Separação (evita inimigos empilhados)")]
    public float separationRadius = 0.8f;
    public float separationStrength = 3f;
}
