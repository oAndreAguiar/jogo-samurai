using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Texto provisório no canto da tela com tempo, onda, inimigos vivos e FPS.
/// Serve para testar a horda até o HUD de verdade (etapa 7).
/// </summary>
public class HordeDebugHUD : MonoBehaviour
{
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private Text label;

    [Tooltip("Segundos entre atualizações do texto (evita gerar texto todo frame).")]
    [SerializeField] private float refreshInterval = 0.25f;

    private float timer;
    private int frames;

    private void Update()
    {
        frames++;
        timer += Time.unscaledDeltaTime;
        if (timer < refreshInterval)
        {
            return;
        }

        float fps = frames / timer;
        timer = 0f;
        frames = 0;

        int seconds = Mathf.FloorToInt(spawner.ElapsedTime);
        label.text = string.Format("{0:00}:{1:00}   Onda {2}   Inimigos {3}   {4:0} FPS",
            seconds / 60, seconds % 60, spawner.WaveIndex, spawner.ActiveEnemies.Count, fps);
    }
}
