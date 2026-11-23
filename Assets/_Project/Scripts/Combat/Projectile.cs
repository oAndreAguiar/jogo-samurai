using UnityEngine;

/// <summary>Dados de um disparo (o que a habilidade manda para o ProjectileSystem).</summary>
public struct ProjectileShot
{
    public Sprite sprite;
    public Vector3 position;
    public Vector3 direction;
    public float speed;
    public float damage;
    public int pierce;
    public float hitRadius;
    public float lifetime;
    public float spinSpeed;
}

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

    public Vector3 Velocity { get; private set; }
    public float Damage { get; private set; }
    public float HitRadius { get; private set; }
    public float SpinSpeed { get; private set; }
    public float TimeLeft { get; set; }
    public int PierceLeft { get; set; }
    public float SpinAngle { get; set; }
    public int ActiveIndex { get; set; } = -1;

    public void Setup(SpriteRenderer renderer)
    {
        spriteRenderer = renderer;
    }

    public void Launch(in ProjectileShot shot)
    {
        spriteRenderer.sprite = shot.sprite;
        transform.position = shot.position;
        Velocity = shot.direction.normalized * shot.speed;
        Damage = shot.damage;
        HitRadius = shot.hitRadius;
        SpinSpeed = shot.spinSpeed;
        TimeLeft = shot.lifetime;
        PierceLeft = shot.pierce;
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
