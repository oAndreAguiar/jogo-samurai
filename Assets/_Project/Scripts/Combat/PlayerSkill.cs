using UnityEngine;

/// <summary>
/// Base das habilidades que o jogador aperta (ativa no R e ultimate no C).
/// O HUD lê quanto falta para ficar pronta e os botões chamam TryActivate.
/// </summary>
public abstract class PlayerSkill : MonoBehaviour
{
    public abstract string DisplayName { get; }
    public abstract Sprite Icon { get; }

    /// <summary>0 = acabou de usar, 1 = pronta.</summary>
    public abstract float Readiness { get; }

    public bool IsReady => Readiness >= 1f;

    /// <summary>Tenta usar. Devolve true se usou.</summary>
    public abstract bool TryActivate();
}
