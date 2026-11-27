using System;
using UnityEngine;

/// <summary>
/// Uma animação simples de sprite: lista de quadros + velocidade.
/// Usada pelo jogador e pelos inimigos (sem Animator, mais leve para a horda).
/// </summary>
[Serializable]
public class SpriteClip
{
    public Sprite[] frames;
    [Tooltip("Quadros por segundo.")]
    public float fps = 10f;
    [Tooltip("Se desligado, toca uma vez e para no último quadro.")]
    public bool loop = true;

    // Duração de uma volta completa, em segundos
    public float Duration => frames != null && fps > 0f ? frames.Length / fps : 0f;

    public bool IsEmpty => frames == null || frames.Length == 0;

    /// <summary>Quadro para o tempo dado (em segundos desde o início da animação).</summary>
    public Sprite FrameAt(float time)
    {
        int frame = Mathf.FloorToInt(time * fps);
        frame = loop ? frame % frames.Length : Mathf.Min(frame, frames.Length - 1);
        return frames[frame];
    }
}
