using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Doelwit & Afstand")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private float distance = 5.0f;

    [Header("Muis Gevoeligheid & Limieten")]
    [SerializeField] private float mouseSensitivity = 0.2f;
    [SerializeField] private float minPitch = -30f; // Hoe ver je omhoog kunt kijken
    [SerializeField] private float maxPitch = 70f;  // Hoe ver je omlaag kunt kijken

    [Header("Botsing (Collision)")]
    [SerializeField] private LayerMask collisionLayers;

    private float yaw = 0f;
    private float pitch = 15f;

    void Start()
    {
        // Vergrendel en verberg de muiscursor in het midden van het scherm
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null || Mouse.current == null) return;

        // Muisbeweging uitlezen (Unity 6 Input System)
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Bereken de rotatie en gewenste positie
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focusPoint = target.position + targetOffset;
        Vector3 desiredPosition = focusPoint - (rotation * Vector3.forward * distance);

        // Voorkom dat de camera door de vloer of muren heen zakt
        if (Physics.Linecast(focusPoint, desiredPosition, out RaycastHit hit, collisionLayers))
        {
            desiredPosition = hit.point + hit.normal * 0.2f;
        }

        transform.position = desiredPosition;
        transform.LookAt(focusPoint);
    }
}
