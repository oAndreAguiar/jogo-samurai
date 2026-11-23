using UnityEngine;

/// <summary>
/// Atributos de vida do jogador (balanceamento).
/// </summary>
[CreateAssetMenu(fileName = "PlayerStats", menuName = "JogoJapones/Player Stats")]
public class PlayerStats : ScriptableObject
{
    public float maxHealth = 100f;

    [Tooltip("Vida recuperada por segundo.")]
    public float regenPerSecond = 1f;

    [Tooltip("Depois de levar dano, o jogador fica invulnerável por este tempo, em segundos.")]
    public float invulnerabilityTime = 0.5f;

    [Tooltip("Distância em que um inimigo encosta no jogador, em metros.")]
    public float contactRadius = 0.8f;
}
