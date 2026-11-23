using UnityEngine;

/// <summary>
/// Círculo que prende o jogador numa arena (Santuário). Mostra um anel no chão
/// e empurra o jogador de volta quando ele tenta sair.
/// </summary>
public class ArenaBoundary : MonoBehaviour
{
    [SerializeField] private CharacterController player;
    [SerializeField] private SpriteRenderer ring;

    public bool IsActive { get; private set; }
    public Vector3 Center { get; private set; }
    public float Radius { get; private set; }

    private void Awake()
    {
        ring.enabled = false;
    }

    /// <summary>Liga a arena com centro e raio dados.</summary>
    public void Activate(Vector3 center, float radius, Color color)
    {
        IsActive = true;
        Center = center;
        Radius = radius;

        // Anel deitado no chão, escalado até o raio
        ring.enabled = true;
        ring.color = color;
        ring.transform.SetPositionAndRotation(new Vector3(center.x, center.y + 0.05f, center.z), Quaternion.Euler(90f, 0f, 0f));
        float spriteSize = ring.sprite != null ? ring.sprite.bounds.size.x : 1f;
        float diameter = radius * 2f + 0.6f; // um pouco maior para o jogador não "entrar" no anel
        ring.transform.localScale = new Vector3(diameter / spriteSize, diameter / spriteSize, 1f);
    }

    private void LateUpdate()
    {
        if (!IsActive)
        {
            return;
        }

        Vector3 offset = player.transform.position - Center;
        offset.y = 0f;
        float distance = offset.magnitude;
        if (distance > Radius)
        {
            // Empurra de volta só o que passou da borda
            Vector3 back = -offset / distance * (distance - Radius);
            player.Move(back);
        }
    }
}
