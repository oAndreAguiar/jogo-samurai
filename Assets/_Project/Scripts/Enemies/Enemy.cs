using UnityEngine;

/// <summary>
/// Um inimigo da horda. Ele não tem Update próprio: quem move todos de uma vez
/// é o EnemyMovementSystem, o que é bem mais rápido com centenas de inimigos.
/// Os objetos são reaproveitados pelo pool do HordeSpawner.
/// </summary>
public class Enemy : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public EnemyData Data { get; private set; }

    // Altura do chão embaixo do inimigo (atualizada aos poucos pelo sistema de movimento)
    public float GroundHeight { get; set; }

    // Posição no chão (sem a altura do sprite). O sprite fica acima disso.
    public Vector3 GroundPosition { get; set; }

    // Posição na lista de inimigos ativos do HordeSpawner (para remover rápido)
    public int ActiveIndex { get; set; } = -1;

    public void Setup(SpriteRenderer renderer)
    {
        spriteRenderer = renderer;
    }

    /// <summary>Prepara o inimigo para entrar em jogo (chamado ao sair do pool).</summary>
    public void Spawn(EnemyData data, Vector3 groundPosition)
    {
        Data = data;
        spriteRenderer.sprite = data.sprite;
        GroundHeight = groundPosition.y;
        GroundPosition = groundPosition;
        ApplyTransform(Quaternion.identity);
    }

    /// <summary>Copia a posição no chão para o transform, com o sprite virado para a câmera.</summary>
    public void ApplyTransform(Quaternion cameraRotation)
    {
        Vector3 pos = GroundPosition;
        pos.y += Data.spriteHeight;
        transform.SetPositionAndRotation(pos, cameraRotation);
    }

    public void SetFacingLeft(bool left)
    {
        spriteRenderer.flipX = left;
    }
}
