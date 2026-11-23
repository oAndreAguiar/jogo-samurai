using System;
using UnityEngine;

/// <summary>
/// Habilidade que atira projéteis no inimigo mais próximo (ex.: Shuriken).
/// Cada posição da lista "levels" é um nível (a primeira é o nível 1).
/// </summary>
[CreateAssetMenu(fileName = "Ability_Projectile", menuName = "JogoJapones/Abilities/Projectile Ability")]
public class ProjectileAbilityDefinition : UpgradeDefinition
{
    [Serializable]
    public struct Level
    {
        public string description;
        public float damage;
        public float cooldown;
        public int projectiles;
        public int pierce;
    }

    public Sprite projectileSprite;
    public float targetRange = 14f;
    public float projectileSpeed = 14f;
    public float projectileLifetime = 1.5f;
    public float hitRadius = 0.45f;
    public float spreadAngle = 12f;
    public float spinSpeed = 900f;
    public Level[] levels;

    public override UpgradeKind Kind => UpgradeKind.Ability;
    public override int MaxLevel => levels.Length;
    public override string GetDescription(int level) => levels[level - 1].description;
}
