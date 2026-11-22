using UnityEngine;

/// <summary>
/// Dados de balanceamento do movimento do jogador.
/// Fica num asset (ScriptableObject) para ajustar sem mexer no código.
/// </summary>
[CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "JogoJapones/Player Movement Settings")]
public class PlayerMovementSettings : ScriptableObject
{
    [Tooltip("Velocidade de caminhada em metros por segundo.")]
    public float moveSpeed = 5f;

    [Tooltip("Gravidade aplicada quando o jogador está no ar (valor negativo).")]
    public float gravity = -25f;

    [Tooltip("Altura máxima de degrau que o jogador sobe andando, em metros.")]
    public float stepOffset = 0.4f;

    [Tooltip("Inclinação máxima de rampa que o jogador consegue subir, em graus.")]
    public float slopeLimit = 45f;

    [Tooltip("Entrada abaixo deste valor é ignorada (evita o personagem andar sozinho com joystick).")]
    [Range(0f, 0.5f)]
    public float inputDeadZone = 0.1f;
}
