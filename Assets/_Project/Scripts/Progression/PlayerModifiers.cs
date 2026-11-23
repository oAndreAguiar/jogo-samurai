/// <summary>
/// Multiplicadores de atributos do jogador, somados a partir das passivas.
/// Habilidades, movimento e coleta leem daqui.
/// </summary>
public class PlayerModifiers
{
    public float DamageMultiplier = 1f;
    public float CooldownMultiplier = 1f;
    public float MoveSpeedMultiplier = 1f;
    public float PickupRadiusMultiplier = 1f;
    public float BonusMaxHealth = 0f;

    public void Reset()
    {
        DamageMultiplier = 1f;
        CooldownMultiplier = 1f;
        MoveSpeedMultiplier = 1f;
        PickupRadiusMultiplier = 1f;
        BonusMaxHealth = 0f;
    }
}
