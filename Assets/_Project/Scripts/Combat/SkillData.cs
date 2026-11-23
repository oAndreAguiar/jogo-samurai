using UnityEngine;

/// <summary>
/// Dados das habilidades de botão (ativa e ultimate). Provisório: no protótipo
/// existe uma ativa (avanço) e uma ultimate (golpe em todos na tela).
/// </summary>
[CreateAssetMenu(fileName = "Skill", menuName = "JogoJapones/Skill Data")]
public class SkillData : ScriptableObject
{
    public string displayName = "Habilidade";
    public Sprite icon;

    [Header("Ativa (avanço)")]
    [Tooltip("Recarga em segundos.")]
    public float cooldown = 6f;
    public float dashDistance = 5f;
    public float dashDuration = 0.18f;
    [Tooltip("Invulnerável por este tempo ao usar.")]
    public float invulnerability = 0.4f;

    [Header("Ultimate")]
    [Tooltip("Abates para carregar a ultimate.")]
    public int killsToCharge = 120;
    public float damage = 150f;
    [Tooltip("Alcance a partir do jogador, em metros.")]
    public float range = 20f;
}
