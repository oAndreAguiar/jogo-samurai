using UnityEngine;

/// <summary>Tipo de upgrade: habilidade (ocupa espaço de habilidade) ou passiva.</summary>
public enum UpgradeKind
{
    Ability,
    Passive
}

/// <summary>
/// Base de tudo que pode aparecer numa carta de level-up (habilidades e passivas).
/// Cada tipo concreto guarda os números de cada nível.
/// </summary>
public abstract class UpgradeDefinition : ScriptableObject
{
    public string displayName = "Upgrade";
    public Sprite icon;

    public abstract UpgradeKind Kind { get; }

    /// <summary>Nível máximo (teto) deste upgrade.</summary>
    public abstract int MaxLevel { get; }

    /// <summary>Texto da carta para o nível indicado (1 = primeiro nível).</summary>
    public abstract string GetDescription(int level);
}
