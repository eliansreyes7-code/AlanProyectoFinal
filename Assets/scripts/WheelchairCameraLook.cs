using System.Collections;
using UnityEngine;

public class WheelchairCameraLook : MonoBehaviour
{
    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 120f;
    [SerializeField] private float minVerticalAngle = -55f;
    [SerializeField] private float maxVerticalAngle = 65f;
    [SerializeField] private float maxHorizontalAngle = 85f;

    [Header("Cinematic")]
    [SerializeField] private float zoomFOV = 42f;

    private float verticalRotation;
    private float horizontalRotation;

    private Quaternion initialLocalRotation;

    private Camera cam;

    private bool canLook = true;
    private bool cinematicActive = false;

    private void Start()
    {
        initialLocalRotation = transform.localRotation;

        cam = GetComponent<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!canLook || cinematicActive)
            return;

        float mouseX =
            Input.GetAxis("Mouse X") *
            mouseSensitivity *
            Time.deltaTime;

        float mouseY =
            Input.GetAxis("Mouse Y") *
            mouseSensitivity *
            Time.deltaTime;

        horizontalRotation += mouseX;
        verticalRotation -= mouseY;

        horizontalRotation = Mathf.Clamp(
            horizontalRotation,
            -maxHorizontalAngle,
            maxHorizontalAngle
        );

        verticalRotation = Mathf.Clamp(
            verticalRotation,
            minVerticalAngle,
            maxVerticalAngle
        );

        transform.localRotation =
            initialLocalRotation *
            Quaternion.Euler(
                verticalRotation,
                horizontalRotation,
                0f
            );
    }

    public IEnumerator LookAtTargetWithZoom(
        Transform target,
        float turnDuration,
        float holdDuration)
    {
        if (target == null)
            yield break;

        cinematicActive = true;
        canLook = false;

        Quaternion startLocalRotation =
            transform.localRotation;

        float originalFOV =
            cam != null ? cam.fieldOfView : 60f;

        Vector3 direction =
            target.position - transform.position;

        Quaternion worldTargetRotation =
            Quaternion.LookRotation(
                direction,
                Vector3.up
            );

        Quaternion targetLocalRotation;

        if (transform.parent != null)
        {
            targetLocalRotation =
                Quaternion.Inverse(
                    transform.parent.rotation
                ) *
                worldTargetRotation;
        }
        else
        {
            targetLocalRotation =
                worldTargetRotation;
        }

        // ==========================================
        // GIRAR HACIA EL PERRO + ZOOM
        // ==========================================

        float timer = 0f;

        while (timer < turnDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / turnDuration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            transform.localRotation =
                Quaternion.Slerp(
                    startLocalRotation,
                    targetLocalRotation,
                    t
                );

            if (cam != null)
            {
                cam.fieldOfView =
                    Mathf.Lerp(
                        originalFOV,
                        zoomFOV,
                        t
                    );
            }

            yield return null;
        }

        yield return new WaitForSeconds(
            holdDuration
        );

        // ==========================================
        // REGRESAR A LA VISTA ORIGINAL
        // ==========================================

        Quaternion currentRotation =
            transform.localRotation;

        timer = 0f;

        while (timer < turnDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / turnDuration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            transform.localRotation =
                Quaternion.Slerp(
                    currentRotation,
                    startLocalRotation,
                    t
                );

            if (cam != null)
            {
                cam.fieldOfView =
                    Mathf.Lerp(
                        zoomFOV,
                        originalFOV,
                        t
                    );
            }

            yield return null;
        }

        transform.localRotation =
            startLocalRotation;

        if (cam != null)
            cam.fieldOfView = originalFOV;

        cinematicActive = false;
        canLook = true;
    }

    public void SetLookEnabled(bool state)
    {
        canLook = state;
    }
}