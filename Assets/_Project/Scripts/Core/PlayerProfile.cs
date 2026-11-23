using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Recorde de uma região.</summary>
[Serializable]
public class RegionRecord
{
    public string regionId;
    public float bestTimeSeconds; // 0 = ainda não venceu
    public int bestKills;
}

/// <summary>
/// Dados salvos do jogador. Hoje fica no aparelho (PlayerPrefs, em JSON);
/// depois vai para o servidor (Supabase) ligado ao mesmo ID.
/// </summary>
[Serializable]
public class ProfileData
{
    public string playerId;
    public int coins;
    public int totalKills;
    public int runsPlayed;
    public List<string> unlockedItems = new List<string>(); // personagens, skins etc. (futuro)
    public List<RegionRecord> records = new List<RegionRecord>();
}

/// <summary>
/// Acesso ao perfil salvo do jogador: ID de convidado, moedas, nível da conta e recordes.
/// Classe estática simples: carrega na primeira vez que alguém pede.
/// </summary>
public static class PlayerProfile
{
    private const string SaveKey = "player_profile_v1";
    private const int KillsPerAccountLevel = 1000;

    private static ProfileData data;

    public static ProfileData Data
    {
        get
        {
            if (data == null)
            {
                Load();
            }
            return data;
        }
    }

    // Nível da conta provisório: sobe a cada 1000 abates somando todas as partidas
    public static int AccountLevel => 1 + Data.totalKills / KillsPerAccountLevel;

    public static RegionRecord GetRecord(string regionId)
    {
        foreach (RegionRecord record in Data.records)
        {
            if (record.regionId == regionId)
            {
                return record;
            }
        }
        return null;
    }

    /// <summary>Guarda o resultado de uma partida (vitória ou derrota).</summary>
    public static void SaveRun(string regionId, bool won, float timeSeconds, int kills, int coins)
    {
        ProfileData d = Data;
        d.runsPlayed++;
        d.totalKills += kills;
        d.coins += coins;

        RegionRecord record = GetRecord(regionId);
        if (record == null)
        {
            record = new RegionRecord { regionId = regionId };
            d.records.Add(record);
        }
        record.bestKills = Mathf.Max(record.bestKills, kills);
        if (won && (record.bestTimeSeconds <= 0f || timeSeconds < record.bestTimeSeconds))
        {
            record.bestTimeSeconds = timeSeconds;
        }
        Save();
    }

    public static void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }

    private static void Load()
    {
        string json = PlayerPrefs.GetString(SaveKey, "");
        data = string.IsNullOrEmpty(json) ? new ProfileData() : JsonUtility.FromJson<ProfileData>(json);
        if (string.IsNullOrEmpty(data.playerId))
        {
            // Convidado: ID gerado no aparelho, depois pode virar conta
            data.playerId = Guid.NewGuid().ToString();
            Save();
        }
    }
}
