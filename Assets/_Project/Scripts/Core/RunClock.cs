using UnityEngine;

/// <summary>
/// Tempo total da partida (todas as zonas + Santuário).
/// Usa o tempo do jogo, então não conta enquanto está pausado (level-up, derrota).
/// </summary>
public class RunClock : MonoBehaviour
{
    public float TotalTime { get; private set; }
    public bool IsRunning { get; private set; } = true;

    private void Awake()
    {
        // No editor, o Time.timeScale = 0 de uma pausa (level-up, derrota) continua valendo
        // se o Play for interrompido ali. Toda partida começa com o tempo andando.
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (IsRunning)
        {
            TotalTime += Time.deltaTime;
        }
    }

    public void Stop()
    {
        IsRunning = false;
    }

    /// <summary>Formata segundos como mm:ss.</summary>
    public static string Format(float seconds)
    {
        int s = Mathf.FloorToInt(seconds);
        return string.Format("{0:00}:{1:00}", s / 60, s % 60);
    }
}
