using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registra no perfil o que o jogador encontrou durante a partida:
/// criaturas (ao aparecerem) e habilidades/passivas (ao aparecerem numa carta ou serem ganhas).
/// Os abates por criatura são somados e gravados no fim da partida.
/// </summary>
public class DiscoveryTracker : MonoBehaviour
{
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private PlayerLoadout loadout;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SanctuaryController sanctuary;

    // Abates desta partida por criatura (gravados só no fim, para não salvar a cada abate)
    private readonly Dictionary<EnemyData, int> killsThisRun = new Dictionary<EnemyData, int>();
    private readonly HashSet<EnemyData> seenThisRun = new HashSet<EnemyData>();
    private bool flushed;

    private void OnEnable()
    {
        spawner.EnemySpawned += HandleEnemySpawned;
        spawner.EnemyKilled += HandleEnemyKilled;
        loadout.OffersBuilt += HandleOffersBuilt;
        loadout.UpgradeApplied += HandleUpgradeApplied;
        playerHealth.Died += Flush;
        sanctuary.Victory += Flush;
    }

    private void OnDisable()
    {
        spawner.EnemySpawned -= HandleEnemySpawned;
        spawner.EnemyKilled -= HandleEnemyKilled;
        loadout.OffersBuilt -= HandleOffersBuilt;
        loadout.UpgradeApplied -= HandleUpgradeApplied;
        playerHealth.Died -= Flush;
        sanctuary.Victory -= Flush;
    }

    private void HandleEnemySpawned(Enemy enemy)
    {
        // O HashSet evita consultar o perfil a cada inimigo que nasce
        if (seenThisRun.Add(enemy.Data))
        {
            PlayerProfile.DiscoverCreature(enemy.Data.name);
        }
    }

    private void HandleEnemyKilled(Enemy enemy)
    {
        killsThisRun.TryGetValue(enemy.Data, out int count);
        killsThisRun[enemy.Data] = count + 1;
    }

    private void HandleOffersBuilt(List<UpgradeOffer> offers)
    {
        foreach (UpgradeOffer offer in offers)
        {
            if (!offer.IsHeal)
            {
                PlayerProfile.DiscoverUpgrade(offer.Definition.name);
            }
        }
    }

    private void HandleUpgradeApplied(UpgradeDefinition definition, int level)
    {
        PlayerProfile.DiscoverUpgrade(definition.name);
    }

    private void OnApplicationQuit()
    {
        Flush();
    }

    /// <summary>Grava os abates desta partida no perfil (uma vez só).</summary>
    public void Flush()
    {
        if (flushed)
        {
            return;
        }
        flushed = true;
        foreach (KeyValuePair<EnemyData, int> pair in killsThisRun)
        {
            PlayerProfile.AddCreatureKills(pair.Key.name, pair.Value);
        }
        PlayerProfile.Save();
    }
}
