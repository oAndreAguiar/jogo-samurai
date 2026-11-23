using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Embaixo do HUD: espaços de habilidades e de passivas, com ícone e nível.
/// Espaço vazio fica apagado. Só é atualizado quando o jogador ganha um upgrade.
/// </summary>
public class LoadoutHUD : MonoBehaviour
{
    [Serializable]
    public class SlotView
    {
        public Image icon;
        public Text levelText;
    }

    [SerializeField] private PlayerLoadout loadout;
    [SerializeField] private SlotView[] abilitySlots;
    [SerializeField] private SlotView[] passiveSlots;

    private readonly List<UpgradeDefinition> buffer = new List<UpgradeDefinition>();

    private void OnEnable()
    {
        loadout.UpgradeApplied += HandleUpgradeApplied;
    }

    private void OnDisable()
    {
        loadout.UpgradeApplied -= HandleUpgradeApplied;
    }

    private void Start()
    {
        Refresh();
    }

    private void HandleUpgradeApplied(UpgradeDefinition def, int level)
    {
        Refresh();
    }

    private void Refresh()
    {
        Fill(UpgradeKind.Ability, abilitySlots);
        Fill(UpgradeKind.Passive, passiveSlots);
    }

    private void Fill(UpgradeKind kind, SlotView[] slots)
    {
        loadout.GetOwned(kind, buffer);
        for (int i = 0; i < slots.Length; i++)
        {
            bool filled = i < buffer.Count;
            slots[i].icon.enabled = filled;
            if (filled)
            {
                UpgradeDefinition def = buffer[i];
                int level = loadout.GetLevel(def);
                slots[i].icon.sprite = def.icon;
                slots[i].levelText.text = level >= def.MaxLevel ? "MAX" : "Nv " + level;
            }
            else
            {
                slots[i].levelText.text = "";
            }
        }
    }
}
