using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Mapa da fase em tela cheia (tecla M ou tocando no minimapa). Pausa o jogo enquanto aberto.
/// Usa uma câmera de cima que enxerga o mapa inteiro, desenhada só enquanto o mapa está aberto.
/// </summary>
public class MapScreen : MonoBehaviour
{
    [SerializeField] private Camera mapCamera;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button minimapButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("Centro do mapa (o chão da fase).")]
    [SerializeField] private Vector3 mapCenter = Vector3.zero;

    public bool IsOpen => panel.activeSelf;

    private void Awake()
    {
        mapCamera.enabled = false;
        panel.SetActive(false);
        minimapButton.onClick.AddListener(Toggle);
        closeButton.onClick.AddListener(Close);
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                Toggle();
            }
            else if (IsOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
            }
        }

        if (IsOpen)
        {
            // Desenha o mapa (o jogo está pausado, então é barato)
            mapCamera.transform.SetPositionAndRotation(mapCenter + Vector3.up * 70f, Quaternion.Euler(90f, 0f, 0f));
            mapCamera.Render();
        }
    }

    public void Toggle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private void Open()
    {
        // Não abre por cima de outra pausa (level-up, fusão, derrota, vitória)
        if (Time.timeScale <= 0f || playerHealth.IsDead)
        {
            return;
        }
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }
        panel.SetActive(false);
        if (!playerHealth.IsDead)
        {
            Time.timeScale = 1f;
        }
    }
}
