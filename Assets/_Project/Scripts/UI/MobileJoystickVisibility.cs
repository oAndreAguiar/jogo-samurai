using UnityEngine;

/// <summary>
/// Mostra o joystick virtual só em aparelhos com tela de toque (celular e tablet).
/// No PC ele fica escondido, a não ser que "Show In Editor" esteja ligado para testar.
/// </summary>
public class MobileJoystickVisibility : MonoBehaviour
{
    [Tooltip("Liga para ver e testar o joystick com o mouse no editor.")]
    [SerializeField] private bool showInEditor = false;

    private void Start()
    {
        // No navegador, o Touchscreen existe até no PC; isMobilePlatform diz se é celular/tablet de verdade
        bool isTouchDevice = Application.isMobilePlatform;
        bool show = isTouchDevice || (Application.isEditor && showInEditor);
        gameObject.SetActive(show);
    }
}
