using UnityEngine;

/// <summary>
/// Um projétil em voo. Não tem Update próprio: o ProjectileSystem move e testa todos de uma vez.
/// </summary>
public class Projectile : MonoBehaviour
{
    // Inimigos já atingidos por este projétil (para não acertar o mesmo duas vezes)
    private const int MaxHits = 16;
    private readonly Enemy[] hitEnemies = new Enemy[MaxHits];
    private int hitCount;

    [SerializeField] private SpriteRenderer spriteRenderer;

    public AbilityData Ability { get; private set; }
    public Vector3 Velocity { get; private set; }
    public float TimeLeft { get; set; }
    public int PierceLeft { get; set; }
    public float SpinAngle { get; set; }
    public int ActiveIndex { get; set; } = -1;

    public void Setup(SpriteRenderer renderer)
    {
        spriteRenderer = renderer;
    }

    public void Launch(AbilityData ability, Vector3 position, Vector3 direction)
    {
        Ability = ability;
        spriteRenderer.sprite = ability.projectileSprite;
        transform.position = position;
        Velocity = direction * ability.projectileSpeed;
        TimeLeft = ability.projectileLifetime;
        PierceLeft = ability.pierce;
        SpinAngle = 0f;
        hitCount = 0;
    }

    public bool HasHit(Enemy enemy)
    {
        for (int i = 0; i < hitCount; i++)
        {
            if (hitEnemies[i] == enemy)
            {
                return true;
            }
        }
        return false;
    }

    public void RegisterHit(Enemy enemy)
    {
        if (hitCount < MaxHits)
        {
            hitEnemies[hitCount++] = enemy;
        }
    }
}
