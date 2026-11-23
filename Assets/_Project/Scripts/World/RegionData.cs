using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dados de uma região (o mapa): as 3 zonas em sequência e o Santuário com o boss.
/// </summary>
[CreateAssetMenu(fileName = "Region", menuName = "JogoJapones/Region Data")]
public class RegionData : ScriptableObject
{
    public string displayName = "Região";

    [Tooltip("Zonas jogadas em ordem antes do Santuário.")]
    public List<ZoneData> zones = new List<ZoneData>();

    [Header("Santuário")]
    public EnemyData boss;

    [Tooltip("Raio da arena do Santuário, em metros.")]
    public float arenaRadius = 13f;

    public Color arenaColor = new Color(0.85f, 0.15f, 0.15f, 0.8f);

    [Tooltip("Cor da luz principal no Santuário.")]
    public Color sanctuaryLightColor = new Color(1f, 0.55f, 0.45f);

    [Tooltip("Cor da luz ambiente no Santuário.")]
    public Color sanctuaryAmbientColor = new Color(0.45f, 0.25f, 0.3f);

    [Header("Ataque do boss: onda de choque")]
    public float bossAttackInterval = 5f;
    public float bossAttackRadius = 4.5f;

    [Tooltip("Tempo do aviso (anel vermelho crescendo) antes do golpe.")]
    public float bossAttackWarning = 1.2f;
    public float bossAttackDamage = 25f;

    [Tooltip("Abaixo desta fração de vida o boss fica furioso.")]
    [Range(0f, 1f)]
    public float enrageHealthFraction = 0.5f;

    [Tooltip("Na fúria: multiplica a velocidade e divide o intervalo de ataque.")]
    public float enrageMultiplier = 1.5f;

    [Header("Medalhas por tempo total (minutos)")]
    public float goldMinutes = 25f;
    public float silverMinutes = 35f;
}
