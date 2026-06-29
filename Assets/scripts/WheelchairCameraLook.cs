using UnityEngine;

public class WheelchairCameraLook : MonoBehaviour
{
    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 120f;
    [SerializeField] private float minVerticalAngle = -55f;
    [SerializeField] private float maxVerticalAngle = 65f;
    [SerializeField] private float maxHorizontalAngle = 85f;

    private float verticalRotation;
    private float horizontalRotation;

    private Quaternion initialLocalRotation;

    private void Start()
    {
        initialLocalRotation = transform.localRotation;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        horizontalRotation += mouseX;
        verticalRotation -= mouseY;

        horizontalRotation = Mathf.Clamp(horizontalRotation, -maxHorizontalAngle, maxHorizontalAngle);
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        transform.localRotation = initialLocalRotation * Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }
}