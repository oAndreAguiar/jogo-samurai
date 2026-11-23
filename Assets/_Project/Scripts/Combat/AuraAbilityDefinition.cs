using System;
using UnityEngine;

/// <summary>
/// Habilidade que acerta todos os inimigos num círculo ao redor do jogador (ex.: Corte de Katana).
/// </summary>
[CreateAssetMenu(fileName = "Ability_Aura", menuName = "JogoJapones/Abilities/Aura Ability")]
public class AuraAbilityDefinition : UpgradeDefinition
{
    [Serializable]
    public struct Level
    {
        public string description;
        public float damage;
        public float cooldown;
        public float radius;
    }

    [Tooltip("Sprite do efeito (um anel), desenhado deitado no chão.")]
    public Sprite effectSprite;
    public Color effectColor = new Color(1f, 1f, 1f, 0.8f);
    public float effectDuration = 0.2f;
    public Level[] levels;

    public override UpgradeKind Kind => UpgradeKind.Ability;
    public override int MaxLevel => levels.Length;
    public override string GetDescription(int level) => levels[level - 1].description;
}
