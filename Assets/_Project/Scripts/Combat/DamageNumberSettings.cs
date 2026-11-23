using UnityEngine;

/// <summary>
/// Aparência dos números de dano (dígitos em pixel art, cores e animação).
/// </summary>
[CreateAssetMenu(fileName = "DamageNumberSettings", menuName = "JogoJapones/Damage Number Settings")]
public class DamageNumberSettings : ScriptableObject
{
    [Tooltip("Textura com os dígitos 0 a 9 lado a lado, todos da mesma largura.")]
    public Texture2D digitsTexture;

    [Tooltip("Pixels por metro dos dígitos (menor = número maior na tela).")]
    public float pixelsPerUnit = 14f;

    [Tooltip("Cor de um golpe normal.")]
    public Color normalColor = Color.white;

    [Tooltip("Cor do golpe que mata o inimigo.")]
    public Color killColor = new Color(1f, 0.85f, 0.2f);

    [Tooltip("Quanto tempo o número fica na tela, em segundos.")]
    public float lifetime = 0.6f;

    [Tooltip("Quanto o número sobe durante a animação, em metros.")]
    public float riseHeight = 1f;

    [Tooltip("Máximo de números ao mesmo tempo (os mais antigos são reaproveitados).")]
    public int maxNumbers = 120;
}
