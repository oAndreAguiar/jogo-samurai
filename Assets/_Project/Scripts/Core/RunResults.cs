using System;
using UnityEngine;

/// <summary>
/// Moedas e resultado da partida atual. Conta as moedas dos inimigos mortos
/// e, no fim (vitória ou derrota), grava tudo no perfil do jogador.
/// </summary>
public class RunResults : MonoBehaviour
{
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private ZoneController zones;
    [SerializeField] private SanctuaryController sanctuary;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RunClock clock;

    // (moedas da partida)
    public event Action<int> CoinsChanged;

    public int Coins { get; private set; }
    public bool Saved { get; private set; }

    private void OnEnable()
    {
        spawner.EnemyKilled += HandleEnemyKilled;
        sanctuary.Victory += HandleVictory;
        playerHealth.Died += HandleDied;
    }

    private void OnDisable()
    {
        spawner.EnemyKilled -= HandleEnemyKilled;
        sanctuary.Victory -= HandleVictory;
        playerHealth.Died -= HandleDied;
    }

    private void HandleEnemyKilled(Enemy enemy)
    {
        EnemyData data = enemy.Data;
        if (data.coinValue > 0 && UnityEngine.Random.value <= data.coinChance)
        {
            Coins += data.coinValue;
            CoinsChanged?.Invoke(Coins);
        }
    }

    private void HandleVictory()
    {
        SaveRun(true);
    }

    private void HandleDied()
    {
        SaveRun(false);
    }

    private void SaveRun(bool won)
    {
        if (Saved)
        {
            return;
        }
        Saved = true;
        PlayerProfile.SaveRun(zones.Region.name, won, clock.TotalTime, spawner.KillCount, Coins);
    }
}
