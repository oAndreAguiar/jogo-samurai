/// <summary>
/// Uma carta oferecida no level-up.
/// Se Definition for null, é a carta de cura (quando não há mais upgrades disponíveis).
/// </summary>
public struct UpgradeOffer
{
    public UpgradeDefinition Definition;

    // Nível que o upgrade vai ter depois de escolhido (1 = novo)
    public int NextLevel;

    public bool IsNew => NextLevel == 1;
    public bool IsHeal => Definition == null;
}
