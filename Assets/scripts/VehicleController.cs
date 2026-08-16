using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class VehicleController : MonoBehaviour
{
    // =====================================================
    // DESTINO
    // =====================================================

    [Header("Destination")]
    [SerializeField] private Transform destinationPoint;
    [SerializeField] private float destinationDistance = 2f;

    // =====================================================
    // MOVIMIENTO
    // =====================================================

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float acceleration = 4f;
    [SerializeField] private float braking = 20f;

    // =====================================================
    // SEMÁFORO
    // =====================================================

    [Header("Traffic Light")]
    [SerializeField] private TrafficLightController trafficLight;

    [Tooltip("Línea donde se detiene el primer carro.")]
    [SerializeField] private Transform stopPoint;

    [Tooltip("Después de pasar este punto ignora el semáforo.")]
    [SerializeField] private Transform trafficExitPoint;

    [SerializeField] private float trafficDetectionDistance = 12f;
    [SerializeField] private float stopDistance = 1.25f;

    // =====================================================
    // SENSOR DE SEGURIDAD
    // =====================================================

    [Header("Vehicle Safety")]

    [Tooltip("Empty situado en el frente del vehículo.")]
    [SerializeField] private Transform safetySensorPoint;

    [Tooltip("Qué tan lejos mira el sensor.")]
    [SerializeField] private float safetyLength = 7f;

    [Tooltip("Ancho total de la zona de seguridad.")]
    [SerializeField] private float safetyWidth = 2f;

    [Tooltip("Altura total de la zona de seguridad.")]
    [SerializeField] private float safetyHeight = 1.5f;

    [Tooltip("Distancia lateral máxima para considerarlo del mismo carril.")]
    [SerializeField] private float laneTolerance = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool showSafetyZone = true;

    // =====================================================
    // FINAL
    // =====================================================

    [Header("Route End")]
    [SerializeField] private bool destroyAtEnd = true;

    // =====================================================
    // VARIABLES
    // =====================================================

    private Rigidbody rb;

    private float currentSpeed = 0f;

    private bool passedTrafficLight = false;

    private Vector3 laneDirection;

    private bool vehicleAheadDetected = false;

    private VehicleController vehicleAhead = null;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        /*
         * Si olvidaste arrastrar el SafetySensorPoint,
         * intenta buscarlo automáticamente por nombre.
         */
        if (safetySensorPoint == null)
        {
            Transform found =
                transform.Find("SafetySensorPoint");

            if (found != null)
            {
                safetySensorPoint = found;
            }
        }

        if (safetySensorPoint == null)
        {
            Debug.LogError(
                name +
                ": falta SafetySensorPoint en el prefab."
            );
        }
    }

    // =====================================================
    // SETUP DESDE SPAWNER
    // =====================================================

    public void Setup(
        Transform newDestinationPoint,
        TrafficLightController newTrafficLight,
        Transform newStopPoint,
        Transform newTrafficExitPoint)
    {
        destinationPoint =
            newDestinationPoint;

        trafficLight =
            newTrafficLight;

        stopPoint =
            newStopPoint;

        trafficExitPoint =
            newTrafficExitPoint;

        currentSpeed = 0f;

        passedTrafficLight = false;

        /*
         * El carro mantiene la dirección
         * con la que salió del SpawnPoint.
         */
        laneDirection =
            transform.forward;

        laneDirection.y = 0f;

        if (laneDirection.sqrMagnitude > 0.001f)
        {
            laneDirection.Normalize();
        }
        else
        {
            laneDirection =
                Vector3.forward;
        }
    }

    // =====================================================
    // FIXED UPDATE
    // =====================================================

    private void FixedUpdate()
    {
        if (destinationPoint == null)
            return;

        // 1. Revisar si ya salió del semáforo.
        CheckTrafficExit();

        // 2. Buscar vehículos delante.
        DetectVehicleAhead();

        // 3. Mover.
        MoveVehicle();

        // 4. Revisar final.
        CheckDestination();
    }

    // =====================================================
    // DETECTAR VEHÍCULO DELANTE
    // =====================================================

    private void DetectVehicleAhead()
    {
        vehicleAheadDetected = false;
        vehicleAhead = null;

        if (safetySensorPoint == null)
            return;

        /*
         * La caja empieza exactamente en
         * SafetySensorPoint y se extiende hacia delante.
         */
        Vector3 center =
            safetySensorPoint.position +
            laneDirection *
            (safetyLength * 0.5f);

        Vector3 halfExtents =
            new Vector3(
                safetyWidth * 0.5f,
                safetyHeight * 0.5f,
                safetyLength * 0.5f
            );

        /*
         * NO usamos Layers.
         * NO usamos Tags.
         *
         * Revisamos todos los colliders
         * y después preguntamos si pertenecen
         * a un VehicleController.
         */
        Collider[] hits =
            Physics.OverlapBox(
                center,
                halfExtents,
                transform.rotation,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide
            );

        float nearestForwardDistance =
            Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            VehicleController other =
                hit.GetComponentInParent<VehicleController>();

            if (other == null)
                continue;

            // No detectarnos a nosotros mismos.
            if (other == this)
                continue;

            /*
             * Posición del otro carro respecto
             * a nuestro carro.
             */
            Vector3 localPosition =
                transform.InverseTransformPoint(
                    other.transform.position
                );

            // Está detrás.
            if (localPosition.z <= 0f)
                continue;

            /*
             * Está demasiado desplazado lateralmente:
             * probablemente pertenece al otro carril.
             */
            if (Mathf.Abs(localPosition.x) >
                laneTolerance)
            {
                continue;
            }

            // Nos quedamos con el carro más cercano.
            if (localPosition.z <
                nearestForwardDistance)
            {
                nearestForwardDistance =
                    localPosition.z;

                vehicleAhead =
                    other;
            }
        }

        vehicleAheadDetected =
            vehicleAhead != null;
    }

    // =====================================================
    // MOVIMIENTO
    // =====================================================

    private void MoveVehicle()
    {
        // =================================================
        // PRIORIDAD ABSOLUTA:
        // VEHÍCULO DELANTE
        // =================================================

        /*
         * Si hay otro carro dentro de nuestra
         * zona de seguridad:
         *
         * STOP.
         *
         * No revisamos StopPoint.
         * No revisamos semáforo.
         * No avanzamos.
         */
        if (vehicleAheadDetected)
        {
            EmergencyStop();
            return;
        }

        // =================================================
        // SOLO AHORA REVISAMOS EL SEMÁFORO
        // =================================================

        bool stopForLight =
            ShouldStopForTrafficLight();

        float desiredSpeed =
            stopForLight
                ? 0f
                : maxSpeed;

        // =================================================
        // ACELERACIÓN / FRENADO
        // =================================================

        float changeRate =
            desiredSpeed < currentSpeed
                ? braking
                : acceleration;

        currentSpeed =
            Mathf.MoveTowards(
                currentSpeed,
                desiredSpeed,
                changeRate *
                Time.fixedDeltaTime
            );

        float movementDistance =
            currentSpeed *
            Time.fixedDeltaTime;

        // =================================================
        // SEGUNDA COMPROBACIÓN JUSTO ANTES DE MOVER
        // =================================================

        DetectVehicleAhead();

        if (vehicleAheadDetected)
        {
            EmergencyStop();
            return;
        }

        // =================================================
        // STOP POINT
        // =================================================

        /*
         * Llegamos aquí únicamente si NO hay
         * carro delante.
         *
         * Así que normalmente solamente el
         * primer carro de la fila usa StopPoint.
         */
        if (stopForLight &&
            stopPoint != null)
        {
            float distanceToStop =
                DistanceAlongLaneToPoint(
                    stopPoint
                );

            if (distanceToStop >= 0f)
            {
                float allowedMovement =
                    distanceToStop -
                    stopDistance;

                if (allowedMovement <= 0f)
                {
                    EmergencyStop();
                    return;
                }

                movementDistance =
                    Mathf.Min(
                        movementDistance,
                        allowedMovement
                    );
            }
        }

        // =================================================
        // MOVER RECTO
        // =================================================

        if (movementDistance <= 0f)
        {
            EmergencyStop();
            return;
        }

        Vector3 nextPosition =
            rb.position +
            laneDirection *
            movementDistance;

        rb.MovePosition(
            nextPosition
        );
    }

    // =====================================================
    // SEMÁFORO
    // =====================================================

    private bool ShouldStopForTrafficLight()
    {
        /*
         * Si ya pasó TrafficExitPoint,
         * no vuelve a obedecer este semáforo.
         */
        if (passedTrafficLight)
            return false;

        if (trafficLight == null ||
            stopPoint == null)
        {
            return false;
        }

        float distanceToStop =
            DistanceAlongLaneToPoint(
                stopPoint
            );

        /*
         * StopPoint ya está detrás.
         * No detener.
         */
        if (distanceToStop < 0f)
            return false;

        /*
         * Todavía está demasiado lejos.
         */
        if (distanceToStop >
            trafficDetectionDistance)
        {
            return false;
        }

        // Solo rojo detiene.
        return trafficLight.CurrentState ==
               TrafficLightController
                   .TrafficState.Red;
    }

    // =====================================================
    // TRAFFIC EXIT POINT
    // =====================================================

    private void CheckTrafficExit()
    {
        if (passedTrafficLight)
            return;

        if (trafficExitPoint == null)
            return;

        Vector3 exitToVehicle =
            rb.position -
            trafficExitPoint.position;

        exitToVehicle.y = 0f;

        float passedAmount =
            Vector3.Dot(
                laneDirection,
                exitToVehicle
            );

        /*
         * Cruzó la línea imaginaria.
         */
        if (passedAmount >= 0f)
        {
            passedTrafficLight = true;
        }
    }

    // =====================================================
    // DISTANCIA LONGITUDINAL
    // =====================================================

    private float DistanceAlongLaneToPoint(
        Transform point)
    {
        if (point == null)
            return Mathf.Infinity;

        Vector3 toPoint =
            point.position -
            rb.position;

        toPoint.y = 0f;

        return Vector3.Dot(
            laneDirection,
            toPoint
        );
    }

    // =====================================================
    // DESTINO FINAL
    // =====================================================

    private void CheckDestination()
    {
        if (destinationPoint == null)
            return;

        float distance =
            HorizontalDistance(
                rb.position,
                destinationPoint.position
            );

        if (distance <=
            destinationDistance)
        {
            FinishRoute();
            return;
        }

        Vector3 destinationToVehicle =
            rb.position -
            destinationPoint.position;

        destinationToVehicle.y = 0f;

        float passedAmount =
            Vector3.Dot(
                laneDirection,
                destinationToVehicle
            );

        if (passedAmount >= 0f)
        {
            FinishRoute();
        }
    }

    // =====================================================
    // STOP
    // =====================================================

    private void EmergencyStop()
    {
        currentSpeed = 0f;

        rb.linearVelocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;
    }

    // =====================================================
    // FINAL
    // =====================================================

    private void FinishRoute()
    {
        EmergencyStop();

        if (destroyAtEnd)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // =====================================================
    // DISTANCIA HORIZONTAL
    // =====================================================

    private float HorizontalDistance(
        Vector3 a,
        Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(
            a,
            b
        );
    }

    // =====================================================
    // MOSTRAR SENSOR EN SCENE
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        if (!showSafetyZone ||
            safetySensorPoint == null)
        {
            return;
        }

        Vector3 forward =
            Application.isPlaying
                ? laneDirection
                : transform.forward;

        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = transform.forward;

        forward.Normalize();

        Vector3 center =
            safetySensorPoint.position +
            forward *
            (safetyLength * 0.5f);

        Matrix4x4 previousMatrix =
            Gizmos.matrix;

        Gizmos.matrix =
            Matrix4x4.TRS(
                center,
                transform.rotation,
                Vector3.one
            );

        Gizmos.DrawWireCube(
            Vector3.zero,
            new Vector3(
                safetyWidth,
                safetyHeight,
                safetyLength
            )
        );

        Gizmos.matrix =
            previousMatrix;
    }
}