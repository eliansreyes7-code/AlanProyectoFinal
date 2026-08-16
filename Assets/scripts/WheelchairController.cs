using UnityEngine;

public class WheelchairController : MonoBehaviour
{
    // =====================================================
    // MOVIMIENTO
    // =====================================================

    [Header("Wheel Push")]
    [SerializeField] private float pushForce = 0.45f;
    [SerializeField] private float maxForwardSpeed = 2.0f;
    [SerializeField] private float maxBackwardSpeed = 1.0f;
    [SerializeField] private float naturalDeceleration = 1.8f;

    // =====================================================
    // FRENO
    // =====================================================

    [Header("Brake")]
    [SerializeField] private float brakeDeceleration = 5.5f;
    [SerializeField] private KeyCode brakeKey = KeyCode.Space;

    // =====================================================
    // ROTACIÓN
    // =====================================================

    [Header("Rotation")]
    [SerializeField] private float turnForce = 55f;
    [SerializeField] private float maxTurnSpeed = 90f;
    [SerializeField] private float turnDeceleration = 3.5f;

    // =====================================================
    // TERRENO
    // =====================================================

    [Header("Ground Follow")]

    [Tooltip("Layer utilizada por el suelo, acera y carretera.")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("Punto desde donde se busca el suelo.")]
    [SerializeField] private Transform groundCheckPoint;

    [Tooltip("Altura desde la que comienza el Raycast.")]
    [SerializeField] private float rayStartHeight = 2f;

    [Tooltip("Distancia total que buscará hacia abajo.")]
    [SerializeField] private float groundCheckDistance = 5f;

    [Tooltip("Altura del centro del Player respecto al suelo.")]
    [SerializeField] private float groundOffset = 0.75f;

    [Tooltip("Velocidad con la que la silla sigue cambios de altura.")]
    [SerializeField] private float groundFollowSpeed = 20f;

    [Header("Debug")]
    [SerializeField] private bool showGroundRay = true;

    // =====================================================
    // VARIABLES
    // =====================================================

    private Rigidbody rb;

    private float currentSpeed;
    private float currentTurnSpeed;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError(
                "WheelchairController necesita Rigidbody."
            );

            enabled = false;
            return;
        }

        // No usamos gravedad.
        rb.useGravity = false;

        rb.isKinematic = false;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        /*
         * IMPORTANTE:
         *
         * Position X = libre
         * Position Y = libre
         * Position Z = libre
         *
         * Rotation X/Z bloqueadas.
         * Rotation Y libre.
         */
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        if (groundCheckPoint == null)
        {
            groundCheckPoint = transform;
        }
    }

    // =====================================================
    // INPUT
    // =====================================================

    private void Update()
    {
        bool reverseMode =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            PushLeftWheel(reverseMode);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            PushRightWheel(reverseMode);
        }
    }

    // =====================================================
    // FÍSICA
    // =====================================================

    private void FixedUpdate()
    {
        ApplyMovementAndGround();

        ApplyRotation();

        ApplyDeceleration();
    }

    // =====================================================
    // MOVIMIENTO + TERRENO
    // =====================================================

    private void ApplyMovementAndGround()
    {
        currentSpeed =
            Mathf.Clamp(
                currentSpeed,
                -maxBackwardSpeed,
                maxForwardSpeed
            );

        // =================================================
        // MOVIMIENTO HORIZONTAL
        // =================================================

        Vector3 horizontalMovement =
            transform.forward *
            currentSpeed *
            Time.fixedDeltaTime;

        horizontalMovement.y = 0f;

        Vector3 nextPosition =
            rb.position +
            horizontalMovement;

        // =================================================
        // BUSCAR ALTURA DEL SUELO
        // =================================================

        Vector3 rayBase =
            groundCheckPoint != null
                ? groundCheckPoint.position
                : rb.position;

        /*
         * IMPORTANTE:
         * buscamos el suelo desde la posición
         * a la que queremos movernos.
         */
        Vector3 rayOrigin =
            new Vector3(
                nextPosition.x,
                rayBase.y + rayStartHeight,
                nextPosition.z
            );

        RaycastHit hit;

        bool foundGround =
            Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out hit,
                groundCheckDistance,
                groundLayer,
                QueryTriggerInteraction.Ignore
            );

        if (showGroundRay)
        {
            Debug.DrawRay(
                rayOrigin,
                Vector3.down *
                groundCheckDistance,
                foundGround
                    ? Color.green
                    : Color.red
            );
        }

        // =================================================
        // CORREGIR Y
        // =================================================

        if (foundGround)
        {
            float targetY =
                hit.point.y +
                groundOffset;

            /*
             * Seguimiento suave pero rápido.
             */
            nextPosition.y =
                Mathf.MoveTowards(
                    rb.position.y,
                    targetY,
                    groundFollowSpeed *
                    Time.fixedDeltaTime
                );
        }
        else
        {
            /*
             * Si por alguna razón no encontramos suelo,
             * conservamos la altura actual.
             */
            nextPosition.y =
                rb.position.y;
        }

        // =================================================
        // UN SOLO MOVEPOSITION
        // =================================================

        rb.MovePosition(
            nextPosition
        );
    }

    // =====================================================
    // RUEDA IZQUIERDA
    // =====================================================

    private void PushLeftWheel(
        bool reverse)
    {
        float direction =
            reverse ? -1f : 1f;

        currentSpeed +=
            pushForce *
            direction;

        currentTurnSpeed +=
            turnForce *
            direction;
    }

    // =====================================================
    // RUEDA DERECHA
    // =====================================================

    private void PushRightWheel(
        bool reverse)
    {
        float direction =
            reverse ? -1f : 1f;

        currentSpeed +=
            pushForce *
            direction;

        currentTurnSpeed -=
            turnForce *
            direction;
    }

    // =====================================================
    // ROTACIÓN
    // =====================================================

    private void ApplyRotation()
    {
        currentTurnSpeed =
            Mathf.Clamp(
                currentTurnSpeed,
                -maxTurnSpeed,
                maxTurnSpeed
            );

        Quaternion deltaRotation =
            Quaternion.Euler(
                0f,
                currentTurnSpeed *
                Time.fixedDeltaTime,
                0f
            );

        rb.MoveRotation(
            rb.rotation *
            deltaRotation
        );
    }

    // =====================================================
    // DESACELERACIÓN
    // =====================================================

    private void ApplyDeceleration()
    {
        bool braking =
            Input.GetKey(brakeKey);

        float speedDeceleration =
            braking
                ? brakeDeceleration
                : naturalDeceleration;

        float rotationDeceleration =
            braking
                ? brakeDeceleration *
                  maxTurnSpeed
                : turnDeceleration *
                  maxTurnSpeed;

        currentSpeed =
            Mathf.MoveTowards(
                currentSpeed,
                0f,
                speedDeceleration *
                Time.fixedDeltaTime
            );

        currentTurnSpeed =
            Mathf.MoveTowards(
                currentTurnSpeed,
                0f,
                rotationDeceleration *
                Time.fixedDeltaTime
            );
    }

    // =====================================================
    // UTILIDADES
    // =====================================================

    public void ReduceSpeed(
        float multiplier)
    {
        currentSpeed *= multiplier;
        currentTurnSpeed *= multiplier;
    }

    public void StopMovement()
    {
        currentSpeed = 0f;
        currentTurnSpeed = 0f;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }
    }
}