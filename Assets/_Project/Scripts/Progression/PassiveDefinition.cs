using UnityEngine;

/// <summary>Atributo que uma passiva melhora.</summary>
public enum PassiveStat
{
    MoveSpeed,      // % de velocidade
    MaxHealth,      // pontos de vida
    Damage,         // % de dano
    Cooldown,       // % a menos de recarga
    PickupRadius    // % de alcance de coleta
}

/// <summary>
/// Passiva: bônus de atributo que cresce a cada nível.
/// </summary>
[CreateAssetMenu(fileName = "Passive", menuName = "JogoJapones/Passive")]
public class PassiveDefinition : UpgradeDefinition
{
    public PassiveStat stat;

    [Tooltip("Quanto o atributo melhora por nível. Porcentagem como 0.1 = 10%; vida em pontos.")]
    public float valuePerLevel = 0.1f;

    [SerializeField] private int maxLevel = 5;

    [Tooltip("Texto da carta. {0} vira o valor por nível (já formatado).")]
    public string descriptionFormat = "+{0} de atributo";

    public override UpgradeKind Kind => UpgradeKind.Passive;
    public override int MaxLevel => maxLevel;

    public override string GetDescription(int level)
    {
        string value = stat == PassiveStat.MaxHealth
            ? Mathf.RoundToInt(valuePerLevel).ToString()
            : Mathf.RoundToInt(valuePerLevel * 100f) + "%";
        return string.Format(descriptionFormat, value);
    }
}
