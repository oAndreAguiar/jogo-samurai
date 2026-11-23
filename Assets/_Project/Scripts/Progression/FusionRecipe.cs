using UnityEngine;

/// <summary>
/// Receita de fusão: duas habilidades no nível máximo viram uma nova (só num Altar de Fusão).
/// </summary>
[CreateAssetMenu(fileName = "Fusion", menuName = "JogoJapones/Fusion Recipe")]
public class FusionRecipe : ScriptableObject
{
    public UpgradeDefinition ingredientA;
    public UpgradeDefinition ingredientB;
    public UpgradeDefinition result;

    [TextArea(2, 4)]
    public string description = "";

    /// <summary>True se a receita usa exatamente estes dois (em qualquer ordem).</summary>
    public bool Uses(UpgradeDefinition a, UpgradeDefinition b)
    {
        return (ingredientA == a && ingredientB == b) || (ingredientA == b && ingredientB == a);
    }
}
