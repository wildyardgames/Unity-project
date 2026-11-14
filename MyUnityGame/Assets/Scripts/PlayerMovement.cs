using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Beweging Snelheden")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Springen & Zwaartekracht")]
    [SerializeField] private float jumpHeight = 2.0f;
    [SerializeField] private float gravity = -22f;

    [Header("Rol Instellingen")]
    [SerializeField] private float rollSpeed = 15f;
    [SerializeField] private float rollDuration = 0.4f;
    [SerializeField] private float rollCooldown = 0.8f;

    private CharacterController controller;
    private Transform cameraTransform;
    private Vector3 velocity;
    private Vector3 moveDirection;

    private bool isRolling = false;
    private float rollTimer = 0f;
    private float cooldownTimer = 0f;
    private Vector3 rollDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        ApplyGravity();

        if (isRolling)
        {
            HandleRoll();
        }
        else
        {
            AlignWithCamera();
            HandleMovement();
            CheckJumpInput();
            CheckRollInput();
        }
    }

    private void AlignWithCamera()
    {
        if (cameraTransform == null) return;

        // Neem alleen de horizontale kijkrichting (Y-as) van de camera over
        // Zo kijkt de speler altijd dezelfde kant op als jij, zonder voorover te kantelen
        Quaternion targetRotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            // Beweeg relatief aan de richting waar de speler/camera naartoe kijkt
            moveDirection = (transform.forward * inputDir.z + transform.right * inputDir.x).normalized;

            bool isSprinting = Keyboard.current.leftShiftKey.isPressed;
            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
        else
        {
            moveDirection = Vector3.zero;
        }
    }

    private void CheckJumpInput()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void CheckRollInput()
    {
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame && cooldownTimer <= 0f && controller.isGrounded)
        {
            StartRoll();
        }
    }

    private void StartRoll()
    {
        isRolling = true;
        rollTimer = rollDuration;
        cooldownTimer = rollCooldown;

        if (moveDirection.magnitude >= 0.1f)
        {
            rollDirection = moveDirection;
        }
        else
        {
            rollDirection = transform.forward;
        }
    }

    private void HandleRoll()
    {
        rollTimer -= Time.deltaTime;
        controller.Move(rollDirection * rollSpeed * Time.deltaTime);

        if (rollTimer <= 0f)
        {
            isRolling = false;
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
