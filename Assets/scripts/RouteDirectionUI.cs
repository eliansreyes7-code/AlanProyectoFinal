using TMPro;
using UnityEngine;

public class RouteDirectionUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RouteManager routeManager;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerCamera;

    [Header("UI")]
    [SerializeField] private RectTransform directionArrow;
    [SerializeField] private TMP_Text directionText;
    [SerializeField] private TMP_Text distanceText;

    [Header("Configuración")]
    [SerializeField] private float straightAngle = 20f;
    [SerializeField] private float slightTurnAngle = 50f;
    [SerializeField] private float turnAroundAngle = 140f;

    private void Start()
    {
        // Buscar RouteManager automáticamente si no está asignado.
        if (routeManager == null)
            routeManager = FindFirstObjectByType<RouteManager>();

        // Buscar Player automáticamente.
        if (player == null)
        {
            WheelchairController wheelchair =
                FindFirstObjectByType<WheelchairController>();

            if (wheelchair != null)
                player = wheelchair.transform;
        }

        // Usamos la cámara principal para saber hacia dónde mira el usuario.
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        ValidateReferences();
    }

    private void Update()
    {
        if (routeManager == null ||
            player == null ||
            playerCamera == null)
            return;

        Checkpoint currentCheckpoint = routeManager.CurrentCheckpoint;

        if (currentCheckpoint == null)
        {
            SetHUDVisible(false);
            return;
        }

        SetHUDVisible(true);

        Vector3 targetPosition = currentCheckpoint.TargetTransform.position;

        UpdateDistance(targetPosition);
        UpdateDirection(targetPosition);
    }

    private void UpdateDistance(Vector3 targetPosition)
    {
        // Distancia horizontal.
        Vector3 playerPosition = player.position;

        playerPosition.y = 0f;
        targetPosition.y = 0f;

        float distance = Vector3.Distance(
            playerPosition,
            targetPosition
        );

        if (distanceText != null)
            distanceText.text = $"{Mathf.RoundToInt(distance)} m";
    }

    private void UpdateDirection(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - playerCamera.position;

        // Solo navegación horizontal.
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Vector3 cameraForward = playerCamera.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        direction.Normalize();

        float angle = Vector3.SignedAngle(
            cameraForward,
            direction,
            Vector3.up
        );

        UpdateArrow(angle);
        UpdateDirectionText(angle);
    }

    private void UpdateArrow(float angle)
    {
        if (directionArrow == null)
            return;

        /*
         * La imagen de la flecha debe apuntar hacia ARRIBA
         * originalmente en el Canvas.
         */
        directionArrow.localRotation =
            Quaternion.Euler(0f, 0f, -angle);
    }

    private void UpdateDirectionText(float angle)
    {
        if (directionText == null)
            return;

        float absAngle = Mathf.Abs(angle);

        // Objetivo prácticamente delante.
        if (absAngle <= straightAngle)
        {
            directionText.text = "Sigue adelante";
            return;
        }

        // Objetivo prácticamente detrás.
        if (absAngle >= turnAroundAngle)
        {
            directionText.text = "Date la vuelta";
            return;
        }

        // IZQUIERDA
        if (angle < 0f)
        {
            if (absAngle <= slightTurnAngle)
                directionText.text = "Mantente hacia la izquierda";
            else
                directionText.text = "Gira a la izquierda";

            return;
        }

        // DERECHA
        if (absAngle <= slightTurnAngle)
            directionText.text = "Mantente hacia la derecha";
        else
            directionText.text = "Gira a la derecha";
    }

    private void SetHUDVisible(bool visible)
    {
        if (directionArrow != null)
            directionArrow.gameObject.SetActive(visible);

        if (directionText != null)
            directionText.gameObject.SetActive(visible);

        if (distanceText != null)
            distanceText.gameObject.SetActive(visible);
    }

    private void ValidateReferences()
    {
        if (routeManager == null)
            Debug.LogError(
                "RouteDirectionUI: Falta RouteManager."
            );

        if (player == null)
            Debug.LogError(
                "RouteDirectionUI: Falta Player."
            );

        if (playerCamera == null)
            Debug.LogError(
                "RouteDirectionUI: Falta Player Camera."
            );

        if (directionArrow == null)
            Debug.LogError(
                "RouteDirectionUI: Falta Direction Arrow."
            );

        if (directionText == null)
            Debug.LogError(
                "RouteDirectionUI: Falta Direction Text."
            );

        if (distanceText == null)
            Debug.LogError(
                "RouteDirectionUI: Falta Distance Text."
            );
    }
}