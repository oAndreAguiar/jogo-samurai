using UnityEngine;

/// <summary>
/// Base das habilidades em jogo. Cada habilidade é um componente no jogador,
/// criado pelo PlayerLoadout quando o jogador a escolhe numa carta.
/// </summary>
public abstract class AbilityBehaviour : MonoBehaviour
{
    public UpgradeDefinition Definition { get; private set; }
    public int Level { get; private set; }

    protected PlayerLoadout Loadout { get; private set; }
    protected HordeSpawner Spawner => Loadout.Spawner;
    protected PlayerModifiers Modifiers => Loadout.Modifiers;

    public void Initialize(UpgradeDefinition definition, PlayerLoadout loadout)
    {
        Definition = definition;
        Loadout = loadout;
        OnInitialize();
    }

    public void SetLevel(int level)
    {
        Level = level;
        OnLevelChanged();
    }

    protected virtual void OnInitialize() { }
    protected virtual void OnLevelChanged() { }
}
