using System;
using UnityEngine;

/// <summary>
/// Habilidade de objetos girando em volta do jogador que acertam quem encostar (ex.: Ofuda).
/// </summary>
[CreateAssetMenu(fileName = "Ability_Orbit", menuName = "JogoJapones/Abilities/Orbit Ability")]
public class OrbitAbilityDefinition : UpgradeDefinition
{
    [Serializable]
    public struct Level
    {
        public string description;
        public float damage;
        public int count;
        public float radius;
        [Tooltip("Graus por segundo.")]
        public float rotationSpeed;
    }

    public Sprite orbiterSprite;
    public float hitRadius = 0.5f;

    [Tooltip("Tempo mínimo entre dois acertos no mesmo inimigo, em segundos.")]
    public float hitInterval = 0.5f;

    public Level[] levels;

    public override UpgradeKind Kind => UpgradeKind.Ability;
    public override int MaxLevel => levels.Length;
    public override string GetDescription(int level) => levels[level - 1].description;
}
