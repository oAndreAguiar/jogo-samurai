using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Move o jogador com o teclado (WASD ou setas), controle ou joystick virtual.
/// Usa CharacterController para colidir com o cenário e subir degraus e rampas.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerMovementSettings settings;

    [Tooltip("Opcional: bônus de velocidade das passivas.")]
    [SerializeField] private PlayerLoadout loadout;

    private CharacterController controller;
    private InputAction moveAction;
    private Transform cameraTransform;
    private float verticalSpeed;

    /// <summary>Entrada de movimento deste frame (-1 a 1). Usada pela animação do sprite.</summary>
    public Vector2 MoveInput { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.stepOffset = settings.stepOffset;
        controller.slopeLimit = settings.slopeLimit;

        // Ação "Move" do arquivo InputSystem_Actions (WASD, setas, controle e joystick virtual)
        if (InputSystem.actions != null)
        {
            moveAction = InputSystem.actions.FindAction("Player/Move");
        }

        // Plano B, caso o arquivo de ações não esteja configurado no projeto
        if (moveAction == null)
        {
            moveAction = CreateFallbackMoveAction();
        }
    }

    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        if (input.magnitude < settings.inputDeadZone)
        {
            input = Vector2.zero;
        }
        input = Vector2.ClampMagnitude(input, 1f);
        MoveInput = input;

        // Converte a entrada para a direção da câmera (cima na tela = frente da câmera)
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;
        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();
            right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();
        }
        float speed = settings.moveSpeed * (loadout != null ? loadout.Modifiers.MoveSpeedMultiplier : 1f);
        Vector3 horizontal = (forward * input.y + right * input.x) * speed;

        // Gravidade simples: encostado no chão, fica levemente "grudado" nele
        if (controller.isGrounded && verticalSpeed < 0f)
        {
            verticalSpeed = -2f;
        }
        verticalSpeed += settings.gravity * Time.deltaTime;

        Vector3 velocity = horizontal + Vector3.up * verticalSpeed;
        controller.Move(velocity * Time.deltaTime);
    }

    private static InputAction CreateFallbackMoveAction()
    {
        InputAction action = new InputAction("Move", InputActionType.Value);
        action.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        action.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        action.AddBinding("<Gamepad>/leftStick");
        return action;
    }
}
