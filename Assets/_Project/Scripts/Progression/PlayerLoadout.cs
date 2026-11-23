using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// O que o jogador tem nesta partida: habilidades e passivas com seus níveis.
/// Respeita os tetos (nível máximo e quantidade de espaços), monta as cartas de level-up
/// e calcula os bônus das passivas.
/// </summary>
public class PlayerLoadout : MonoBehaviour
{
    [SerializeField] private ProgressionSettings settings;
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private ProjectileSystem projectiles;
    [SerializeField] private PlayerHealth playerHealth;

    private class OwnedUpgrade
    {
        public UpgradeDefinition Definition;
        public int Level;
        public AbilityBehaviour Ability; // null para passivas
    }

    private readonly List<OwnedUpgrade> owned = new List<OwnedUpgrade>();
    private readonly List<UpgradeOffer> candidates = new List<UpgradeOffer>();

    // Avisa quando algo foi ganho ou subiu de nível: (upgrade, novo nível)
    public event Action<UpgradeDefinition, int> UpgradeApplied;

    public PlayerModifiers Modifiers { get; } = new PlayerModifiers();
    public HordeSpawner Spawner => spawner;
    public ProjectileSystem Projectiles => projectiles;
    public ProgressionSettings Settings => settings;

    private void Start()
    {
        if (settings.startingAbility != null)
        {
            Apply(new UpgradeOffer { Definition = settings.startingAbility, NextLevel = 1 });
        }
    }

    public int GetLevel(UpgradeDefinition definition)
    {
        OwnedUpgrade item = Find(definition);
        return item != null ? item.Level : 0;
    }

    public int CountOwned(UpgradeKind kind)
    {
        int count = 0;
        foreach (OwnedUpgrade item in owned)
        {
            if (item.Definition.Kind == kind)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>Preenche "results" com as cartas sorteadas para este level-up.</summary>
    public void BuildOffers(List<UpgradeOffer> results)
    {
        results.Clear();
        candidates.Clear();

        bool abilitySlotFree = CountOwned(UpgradeKind.Ability) < settings.maxAbilities;
        bool passiveSlotFree = CountOwned(UpgradeKind.Passive) < settings.maxPassives;

        foreach (UpgradeDefinition def in settings.upgradePool)
        {
            int level = GetLevel(def);
            if (level >= def.MaxLevel)
            {
                continue; // já no teto
            }
            if (level == 0)
            {
                bool slotFree = def.Kind == UpgradeKind.Ability ? abilitySlotFree : passiveSlotFree;
                if (!slotFree)
                {
                    continue; // sem espaço para coisa nova desse tipo
                }
            }
            candidates.Add(new UpgradeOffer { Definition = def, NextLevel = level + 1 });
        }

        // Sorteia sem repetir
        int count = Mathf.Min(settings.choicesPerLevel, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            int pick = UnityEngine.Random.Range(i, candidates.Count);
            (candidates[i], candidates[pick]) = (candidates[pick], candidates[i]);
            results.Add(candidates[i]);
        }

        if (results.Count == 0)
        {
            results.Add(new UpgradeOffer { Definition = null, NextLevel = 0 }); // cura
        }
    }

    /// <summary>Aplica a carta escolhida.</summary>
    public void Apply(UpgradeOffer offer)
    {
        if (offer.IsHeal)
        {
            playerHealth.Heal(playerHealth.MaxHealth * settings.fallbackHealPercent);
            return;
        }

        UpgradeDefinition def = offer.Definition;
        OwnedUpgrade item = Find(def);
        if (item == null)
        {
            item = new OwnedUpgrade { Definition = def, Level = 0 };
            owned.Add(item);
            item.Ability = CreateAbility(def);
        }

        item.Level = Mathf.Min(item.Level + 1, def.MaxLevel);
        if (item.Ability != null)
        {
            item.Ability.SetLevel(item.Level);
        }
        if (def.Kind == UpgradeKind.Passive)
        {
            RecalculateModifiers();
        }

        UpgradeApplied?.Invoke(def, item.Level);
    }

    private AbilityBehaviour CreateAbility(UpgradeDefinition def)
    {
        AbilityBehaviour ability = null;
        if (def is ProjectileAbilityDefinition)
        {
            ability = gameObject.AddComponent<ProjectileAbility>();
        }
        else if (def is AuraAbilityDefinition)
        {
            ability = gameObject.AddComponent<AuraAbility>();
        }
        else if (def is OrbitAbilityDefinition)
        {
            ability = gameObject.AddComponent<OrbitAbility>();
        }

        if (ability != null)
        {
            ability.Initialize(def, this);
        }
        return ability;
    }

    private void RecalculateModifiers()
    {
        Modifiers.Reset();
        foreach (OwnedUpgrade item in owned)
        {
            if (!(item.Definition is PassiveDefinition passive))
            {
                continue;
            }
            float total = passive.valuePerLevel * item.Level;
            switch (passive.stat)
            {
                case PassiveStat.MoveSpeed: Modifiers.MoveSpeedMultiplier += total; break;
                case PassiveStat.MaxHealth: Modifiers.BonusMaxHealth += total; break;
                case PassiveStat.Damage: Modifiers.DamageMultiplier += total; break;
                case PassiveStat.Cooldown: Modifiers.CooldownMultiplier = Mathf.Max(0.2f, Modifiers.CooldownMultiplier - total); break;
                case PassiveStat.PickupRadius: Modifiers.PickupRadiusMultiplier += total; break;
            }
        }
        playerHealth.SetBonusMaxHealth(Modifiers.BonusMaxHealth);
    }

    private OwnedUpgrade Find(UpgradeDefinition definition)
    {
        foreach (OwnedUpgrade item in owned)
        {
            if (item.Definition == definition)
            {
                return item;
            }
        }
        return null;
    }
}
