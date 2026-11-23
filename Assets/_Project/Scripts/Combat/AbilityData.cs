using UnityEngine;

/// <summary>
/// Dados de uma habilidade de projétil automática (ex.: shuriken).
/// No protótipo só existe uma; o catálogo de habilidades vem depois.
/// </summary>
[CreateAssetMenu(fileName = "AbilityData", menuName = "JogoJapones/Ability Data")]
public class AbilityData : ScriptableObject
{
    public string displayName = "Shuriken";
    public Sprite projectileSprite;

    [Header("Disparo")]
    [Tooltip("Segundos entre um disparo e outro.")]
    public float cooldown = 0.6f;

    [Tooltip("Só mira em inimigos até esta distância, em metros.")]
    public float targetRange = 14f;

    [Tooltip("Projéteis por disparo (espalhados em leque).")]
    public int projectilesPerShot = 1;

    [Tooltip("Abertura do leque quando há mais de um projétil, em graus.")]
    public float spreadAngle = 15f;

    [Header("Projétil")]
    public float damage = 10f;
    public float projectileSpeed = 14f;

    [Tooltip("Segundos até o projétil sumir se não acertar nada.")]
    public float projectileLifetime = 1.5f;

    [Tooltip("Quantos inimigos o projétil atravessa depois do primeiro acerto.")]
    public int pierce = 1;

    [Tooltip("Raio de acerto do projétil, em metros.")]
    public float hitRadius = 0.45f;

    [Tooltip("Velocidade de giro do sprite, em graus por segundo (só visual).")]
    public float spinSpeed = 900f;
}
