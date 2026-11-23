using UnityEngine;

/// <summary>
/// No celular em pé (retrato), mostra um aviso pedindo para girar o aparelho.
/// O HUD foi pensado para a tela deitada (paisagem).
/// </summary>
public class OrientationHint : MonoBehaviour
{
    [SerializeField] private GameObject hint;

    private void Update()
    {
        bool portrait = Screen.height > Screen.width;
        bool touch = Application.isMobilePlatform;
        bool show = portrait && touch;
        if (hint.activeSelf != show)
        {
            hint.SetActive(show);
        }
    }
}
