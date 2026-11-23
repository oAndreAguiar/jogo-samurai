using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Volta para o menu principal: pelo botão ou pela tecla Esc (o M é o mapa).
/// Fica nos painéis de vitória e derrota, então só funciona quando eles estão visíveis.
/// </summary>
public class ReturnToMenu : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private Button button;

    private void Awake()
    {
        button.onClick.AddListener(Go);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Go();
        }
    }

    public void Go()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
