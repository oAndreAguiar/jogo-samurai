using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dados de uma zona: duração, horda, elites e guardiões possíveis.
/// Cada região tem 3 zonas jogadas em sequência.
/// </summary>
[CreateAssetMenu(fileName = "Zone", menuName = "JogoJapones/Zone Data")]
public class ZoneData : ScriptableObject
{
    public string displayName = "Zona";

    [Tooltip("Teto de tempo da zona, em segundos (600 = 10 minutos).")]
    public float durationSeconds = 600f;

    [Header("Horda")]
    public HordeSettings horde;

    [Tooltip("Multiplica a vida de todos os inimigos desta zona.")]
    public float enemyHealthMultiplier = 1f;

    [Header("Elites")]
    [Tooltip("Tipos de elite que podem aparecer (sorteados).")]
    public List<EnemyData> elites = new List<EnemyData>();

    [Tooltip("Segundos até a primeira elite.")]
    public float firstEliteTime = 60f;

    [Tooltip("Segundos entre uma leva de elites e a próxima.")]
    public float eliteInterval = 90f;

    [Tooltip("Elites por leva (cresce +1 a cada leva até este máximo).")]
    public int maxElitesPerWave = 3;

    [Header("Guardião")]
    [Tooltip("Um destes é sorteado no começo da zona.")]
    public List<EnemyData> guardianOptions = new List<EnemyData>();

    [Tooltip("Segundos de zona antes de poder chamar o guardião mais cedo.")]
    public float callGuardianAfter = 60f;
}
