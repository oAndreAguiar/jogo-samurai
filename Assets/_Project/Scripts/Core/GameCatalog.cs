using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lista de tudo que existe no jogo, na ordem em que aparece nas coleções do menu
/// (Manual de Criaturas e Habilidades). Novo inimigo ou habilidade: é só acrescentar aqui.
/// </summary>
[CreateAssetMenu(fileName = "GameCatalog", menuName = "JogoJapones/Game Catalog")]
public class GameCatalog : ScriptableObject
{
    public List<EnemyData> creatures = new List<EnemyData>();
    public List<UpgradeDefinition> upgrades = new List<UpgradeDefinition>();
    public List<FusionRecipe> recipes = new List<FusionRecipe>();
}
