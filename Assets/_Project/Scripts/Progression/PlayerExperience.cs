using System;
using UnityEngine;

/// <summary>
/// Experiência e nível do jogador dentro da partida.
/// Quando a XP enche, sobe de nível e guarda um level-up pendente para a tela de cartas.
/// </summary>
public class PlayerExperience : MonoBehaviour
{
    [SerializeField] private ProgressionSettings settings;

    // (XP atual, XP para o próximo nível, nível)
    public event Action<int, int, int> ExperienceChanged;
    // Novo nível alcançado
    public event Action<int> LeveledUp;

    public int Level { get; private set; } = 1;
    public int CurrentXp { get; private set; }
    public int XpToNext => settings.XpToNextLevel(Level);

    // Level-ups que ainda esperam o jogador escolher uma carta
    public int PendingLevelUps { get; private set; }

    private void Start()
    {
        ExperienceChanged?.Invoke(CurrentXp, XpToNext, Level);
    }

    public void AddExperience(int amount)
    {
        CurrentXp += amount;

        // Pode subir mais de um nível de uma vez com uma joia grande
        while (CurrentXp >= XpToNext)
        {
            CurrentXp -= XpToNext;
            Level++;
            PendingLevelUps++;
            LeveledUp?.Invoke(Level);
        }
        ExperienceChanged?.Invoke(CurrentXp, XpToNext, Level);
    }

    /// <summary>Chamado pela tela de cartas depois que o jogador escolhe.</summary>
    public void ConsumePendingLevelUp()
    {
        PendingLevelUps = Mathf.Max(0, PendingLevelUps - 1);
    }
}
