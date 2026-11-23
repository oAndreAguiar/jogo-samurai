using UnityEngine;

/// <summary>
/// Uma joia de experiência no chão. Não tem Update próprio:
/// o ExperienceGemSystem move e coleta todas de uma vez.
/// </summary>
public class ExperienceGem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public int Value { get; set; }
    public bool InUse { get; set; }
    public bool Attracted { get; set; }
    public float Speed { get; set; }
    public float SpawnTime { get; set; }
    public Vector3 BasePosition { get; set; }

    public void Setup(SpriteRenderer renderer)
    {
        spriteRenderer = renderer;
    }

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }
}
