using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Página de coleção do menu: grade com tudo que existe no jogo.
/// O que já foi encontrado mostra ícone e nome; o resto aparece como silhueta "???".
/// Clicar num item mostra os detalhes ao lado.
/// Serve para o Manual de Criaturas e para as Habilidades (campo "mode").
/// </summary>
public class CollectionPage : MonoBehaviour
{
    public enum Mode
    {
        Creatures,
        Upgrades,
        Recipes
    }

    [SerializeField] private Mode mode;
    [SerializeField] private GameCatalog catalog;

    [Header("Grade")]
    [SerializeField] private Transform gridContent;
    [Tooltip("Modelo de item (desativado na cena); é copiado uma vez por entrada ao abrir.")]
    [SerializeField] private GameObject entryTemplate;
    [SerializeField] private Text progressText;
    [SerializeField] private Button backButton;

    [Header("Detalhes")]
    [SerializeField] private Image detailIcon;
    [SerializeField] private Text detailName;
    [SerializeField] private Text detailSubtitle;
    [SerializeField] private Text detailBody;

    public event Action BackRequested;

    private readonly List<GameObject> entries = new List<GameObject>();
    private static readonly Color Silhouette = new Color(0f, 0f, 0f, 0.85f);

    private void Awake()
    {
        entryTemplate.SetActive(false);
        backButton.onClick.AddListener(() => BackRequested?.Invoke());
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            BackRequested?.Invoke();
        }
    }

    /// <summary>Monta a grade com o estado atual do perfil.</summary>
    public void Open()
    {
        foreach (GameObject e in entries)
        {
            Destroy(e);
        }
        entries.Clear();

        int found = 0;
        int total = mode == Mode.Creatures ? catalog.creatures.Count
            : mode == Mode.Upgrades ? catalog.upgrades.Count
            : catalog.recipes.Count;
        for (int i = 0; i < total; i++)
        {
            bool discovered;
            if (mode == Mode.Creatures)
            {
                EnemyData creature = catalog.creatures[i];
                discovered = PlayerProfile.GetCreature(creature.name) != null;
                AddEntry(creature.sprite, discovered ? creature.displayName : "???", discovered, creature.tint, () => ShowCreature(creature));
            }
            else if (mode == Mode.Recipes)
            {
                FusionRecipe recipe = catalog.recipes[i];
                discovered = PlayerProfile.IsRecipeDiscovered(recipe.name);
                AddEntry(recipe.result.icon, discovered ? recipe.result.displayName : "???", discovered, Color.white, () => ShowRecipe(recipe));
            }
            else
            {
                UpgradeDefinition upgrade = catalog.upgrades[i];
                discovered = PlayerProfile.IsUpgradeDiscovered(upgrade.name);
                AddEntry(upgrade.icon, discovered ? upgrade.displayName : "???", discovered, Color.white, () => ShowUpgrade(upgrade));
            }
            if (discovered)
            {
                found++;
            }
        }

        string label = mode == Mode.Creatures ? "Encontradas" : "Descobertas";
        progressText.text = string.Format("{0} {1}/{2}", label, found, total);
        ClearDetails();
    }

    private void AddEntry(Sprite sprite, string label, bool discovered, Color tint, Action onClick)
    {
        GameObject entry = Instantiate(entryTemplate, gridContent);
        entry.SetActive(true);
        Image icon = entry.transform.Find("Icon").GetComponent<Image>();
        icon.sprite = sprite;
        icon.color = discovered ? tint : Silhouette;
        entry.transform.Find("Name").GetComponent<Text>().text = label;
        entry.GetComponent<Button>().onClick.AddListener(() => onClick());
        entries.Add(entry);
    }

    private void ShowCreature(EnemyData creature)
    {
        CreatureRecord record = PlayerProfile.GetCreature(creature.name);
        detailIcon.enabled = true;
        detailIcon.sprite = creature.sprite;
        if (record == null)
        {
            detailIcon.color = Silhouette;
            detailName.text = "???";
            detailSubtitle.text = RankName(creature.rank);
            detailBody.text = "Ainda não encontrada.\nEla aparece em alguma região... continue jogando.";
            return;
        }

        detailIcon.color = creature.tint;
        detailName.text = creature.displayName;
        detailSubtitle.text = RankName(creature.rank);
        detailBody.text = string.Format("{0}\n\nVida: {1}\nVelocidade: {2}\nDano de contato: {3}\n\nDerrotadas: {4}",
            creature.description, Mathf.RoundToInt(creature.maxHealth), creature.moveSpeed.ToString("0.0"),
            Mathf.RoundToInt(creature.contactDamage), record.kills);
    }

    private void ShowUpgrade(UpgradeDefinition upgrade)
    {
        bool discovered = PlayerProfile.IsUpgradeDiscovered(upgrade.name);
        detailIcon.enabled = true;
        detailIcon.sprite = upgrade.icon;
        detailIcon.color = discovered ? Color.white : Silhouette;
        string kind = upgrade.Kind == UpgradeKind.Ability ? "Habilidade" : "Passiva";
        if (!discovered)
        {
            detailName.text = "???";
            detailSubtitle.text = kind;
            detailBody.text = "Ainda não descoberta.\nEla aparece nas cartas de level-up.";
            return;
        }

        detailName.text = upgrade.displayName;
        detailSubtitle.text = kind + "  ·  Nível máximo " + upgrade.MaxLevel;
        StringBuilder sb = new StringBuilder();
        for (int level = 1; level <= upgrade.MaxLevel; level++)
        {
            sb.Append("Nv ").Append(level).Append(": ").Append(upgrade.GetDescription(level)).Append('\n');
        }
        detailBody.text = sb.ToString();
    }

    private void ShowRecipe(FusionRecipe recipe)
    {
        bool discovered = PlayerProfile.IsRecipeDiscovered(recipe.name);
        detailIcon.enabled = true;
        detailIcon.sprite = recipe.result.icon;
        detailIcon.color = discovered ? Color.white : Silhouette;
        if (!discovered)
        {
            detailName.text = "???";
            detailSubtitle.text = "Fusão";
            detailBody.text = "Receita ainda não descoberta.\n\nLeve duas habilidades que combinam ao nível máximo e encontre um Altar de Fusão.";
            return;
        }
        detailName.text = recipe.result.displayName;
        detailSubtitle.text = recipe.ingredientA.displayName + "  +  " + recipe.ingredientB.displayName;
        detailBody.text = recipe.description + "\n\n" + recipe.result.GetDescription(1);
    }

    private void ClearDetails()
    {
        detailIcon.enabled = false;
        detailName.text = "";
        detailSubtitle.text = "";
        detailBody.text = mode == Mode.Creatures ? "Escolha uma criatura para ver os detalhes."
            : mode == Mode.Upgrades ? "Escolha uma habilidade para ver os detalhes."
            : "Escolha uma fusão para ver a receita.";
    }

    private static string RankName(EnemyRank rank)
    {
        switch (rank)
        {
            case EnemyRank.Elite: return "Elite";
            case EnemyRank.Guardian: return "Guardião";
            case EnemyRank.Boss: return "Chefe";
            default: return "Comum";
        }
    }
}
