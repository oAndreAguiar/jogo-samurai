using UnityEngine;

/// <summary>
/// Faz o objeto (normalmente um sprite) ficar sempre virado para a câmera.
/// Usado em personagens, inimigos, plantas e efeitos no cenário 2.5D.
/// </summary>
public class Billboard : MonoBehaviour
{
    [Tooltip("Se ligado, o sprite só gira no eixo Y e fica sempre em pé (bom para personagens e árvores).")]
    [SerializeField] private bool lockVertical = false;

    // Câmera guardada para não procurar a cada frame
    private Transform cameraTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
        {
            return;
        }

        if (lockVertical)
        {
            // Copia só a rotação horizontal da câmera
            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(forward);
            }
        }
        else
        {
            // Fica paralelo ao plano da câmera (sem distorção na perspectiva inclinada)
            transform.rotation = cameraTransform.rotation;
        }
    }
}
