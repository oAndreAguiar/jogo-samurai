using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Balanceamento da progressão dentro da partida: curva de XP, joias, espaços e upgrades.
/// </summary>
[CreateAssetMenu(fileName = "ProgressionSettings", menuName = "JogoJapones/Progression Settings")]
public class ProgressionSettings : ScriptableObject
{
    [Header("Curva de XP: XP para passar do nível N = base x N^expoente")]
    public float xpBase = 5f;
    public float xpExponent = 1.35f;

    [Header("Espaços (tetos de quantidade)")]
    [Tooltip("Habilidade principal + espaços extras.")]
    public int maxAbilities = 4;
    public int maxPassives = 3;

    [Header("Upgrades")]
    [Tooltip("Habilidade com que o jogador começa (ocupa o espaço principal).")]
    public UpgradeDefinition startingAbility;

    [Tooltip("Todos os upgrades que podem aparecer nas cartas.")]
    public List<UpgradeDefinition> upgradePool = new List<UpgradeDefinition>();

    [Tooltip("Cartas oferecidas por level-up.")]
    public int choicesPerLevel = 3;

    [Tooltip("Cura (em % da vida máxima) quando não há mais upgrades disponíveis.")]
    [Range(0f, 1f)]
    public float fallbackHealPercent = 0.3f;

    [Header("Joias de XP")]
    public Sprite smallGemSprite;
    public Sprite bigGemSprite;

    [Tooltip("Joias com este valor ou mais usam o sprite grande.")]
    public int bigGemThreshold = 2;

    [Tooltip("Distância em que as joias começam a ser puxadas, em metros.")]
    public float pickupRadius = 2.5f;

    [Tooltip("Velocidade final das joias sendo puxadas.")]
    public float gemMagnetSpeed = 14f;

    [Tooltip("Máximo de joias no chão. Passando disso, as mais antigas são juntadas nas novas.")]
    public int maxGems = 300;

    /// <summary>XP necessária para sair do nível indicado.</summary>
    public int XpToNextLevel(int level)
    {
        return Mathf.Max(1, Mathf.RoundToInt(xpBase * Mathf.Pow(level, xpExponent)));
    }
}
