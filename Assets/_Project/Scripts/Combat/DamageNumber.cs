using UnityEngine;

/// <summary>
/// Um número de dano na tela, montado com até 4 dígitos (sprites em pixel art).
/// É reaproveitado pelo DamageNumberSystem.
/// </summary>
public class DamageNumber : MonoBehaviour
{
    public const int MaxDigits = 4;

    private readonly SpriteRenderer[] digits = new SpriteRenderer[MaxDigits];

    public float Age { get; set; }
    public Vector3 StartPosition { get; set; }
    public Color BaseColor { get; private set; }
    public bool InUse { get; set; }

    public void Setup(int sortingOrder)
    {
        for (int i = 0; i < MaxDigits; i++)
        {
            GameObject child = new GameObject("Digit" + i);
            child.transform.SetParent(transform, false);
            digits[i] = child.AddComponent<SpriteRenderer>();
            digits[i].sortingOrder = sortingOrder;
        }
    }

    /// <summary>Mostra o valor com os sprites de dígito dados, centralizado.</summary>
    public void Show(int value, Sprite[] digitSprites, float digitWidth, Color color)
    {
        value = Mathf.Clamp(value, 0, 9999);

        // Separa os dígitos sem criar texto (evita lixo de memória a cada golpe)
        int count = value >= 1000 ? 4 : value >= 100 ? 3 : value >= 10 ? 2 : 1;
        int divisor = count == 4 ? 1000 : count == 3 ? 100 : count == 2 ? 10 : 1;
        float startX = -(count - 1) * digitWidth * 0.5f;

        for (int i = 0; i < MaxDigits; i++)
        {
            bool visible = i < count;
            digits[i].enabled = visible;
            if (!visible)
            {
                continue;
            }
            digits[i].sprite = digitSprites[(value / divisor) % 10];
            divisor /= 10;
            digits[i].color = color;
            digits[i].transform.localPosition = new Vector3(startX + i * digitWidth, 0f, 0f);
        }
        BaseColor = color;
    }

    public void SetAlpha(float alpha)
    {
        Color c = BaseColor;
        c.a = alpha;
        for (int i = 0; i < MaxDigits; i++)
        {
            digits[i].color = c;
        }
    }
}
