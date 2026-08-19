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
    // ALTURA DE LA CALLE
    // =====================================================

    [Header("Road Height")]

    [Tooltip("Layer de la superficie de la carretera.")]
    [SerializeField] private LayerMask roadLayer;

    [Tooltip("Altura desde donde comienzan los Raycasts.")]
    [SerializeField] private float roadRayHeight = 6f;

    [Tooltip("Distancia máxima de búsqueda hacia abajo.")]
    [SerializeField] private float roadRayDistance = 20f;

    [Tooltip("Pequeña separación entre el carro y la calle.")]
    [SerializeField] private float roadOffset = 0.03f;

    [Tooltip("Distancia de las muestras delante y detrás del carro.")]
    [SerializeField] private float roadSampleDistance = 1.5f;

    [Tooltip("Cambios de altura menores que este valor son ignorados para evitar temblores.")]
    [SerializeField] private float roadHeightDeadZone = 0.04f;

    [Tooltip("Velocidad con la que el vehículo sigue las subidas y bajadas.")]
    [SerializeField] private float roadHeightFollowSpeed = 10f;

    [Tooltip("Mostrar los Raycasts de suelo en Scene.")]
    [SerializeField] private bool showRoadRay = true;

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

    // =====================================================
    // DEBUG
    // =====================================================

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
    private Collider vehicleCollider;

    private float currentSpeed = 0f;

    private bool passedTrafficLight = false;

    private Vector3 laneDirection;

    private bool vehicleAheadDetected = false;

    private VehicleController vehicleAhead = null;

    /*
     * Distancia desde el pivot del prefab
     * hasta la parte inferior de su collider.
     */
    private float pivotToColliderBottom = 0f;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        vehicleCollider =
            GetComponent<Collider>();

        // =================================================
        // RIGIDBODY
        // =================================================

        /*
         * IMPORTANTE:
         *
         * Lo dejamos dinámico porque así estaba
         * avanzando correctamente.
         */
        rb.useGravity = false;
        rb.isKinematic = false;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        /*
         * NO congelamos Y porque el carro
         * necesita subir y bajar con la carretera.
         */
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        // =================================================
        // ALTURA DEL COLLIDER
        // =================================================

        if (vehicleCollider != null)
        {
            pivotToColliderBottom =
                transform.position.y -
                vehicleCollider.bounds.min.y;
        }

        // =================================================
        // BUSCAR SAFETY SENSOR
        // =================================================

        if (safetySensorPoint == null)
        {
            Transform found =
                transform.Find("SafetySensorPoint");

            if (found != null)
            {
                safetySensorPoint =
                    found;
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
    // SETUP DESDE VEHICLE SPAWNER
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

        // =================================================
        // DIRECCIÓN DEL CARRIL
        // =================================================

        laneDirection =
            transform.forward;

        laneDirection.y = 0f;

        if (laneDirection.sqrMagnitude >
            0.001f)
        {
            laneDirection.Normalize();
        }
        else
        {
            laneDirection =
                Vector3.forward;
        }

        // =================================================
        // COLOCAR EL CARRO SOBRE LA CALLE AL NACER
        // =================================================

        SnapImmediatelyToRoad();
    }

    // =====================================================
    // FIXED UPDATE
    // =====================================================

    private void FixedUpdate()
    {
        if (destinationPoint == null)
            return;

        /*
         * IMPORTANTE:
         *
         * Ya NO hacemos SnapImmediatelyToRoad()
         * en cada FixedUpdate.
         *
         * Solo se hace cuando nace el carro.
         */

        // 1. Revisar salida del semáforo.
        CheckTrafficExit();

        // 2. Buscar carros delante.
        DetectVehicleAhead();

        // 3. Movimiento + altura.
        MoveVehicle();

        // 4. Revisar destino.
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
                hit.GetComponentInParent
                <VehicleController>();

            if (other == null)
                continue;

            // No detectarse a sí mismo.
            if (other == this)
                continue;

            Vector3 localPosition =
                transform.InverseTransformPoint(
                    other.transform.position
                );

            // Está detrás.
            if (localPosition.z <= 0f)
                continue;

            // Está en otro carril.
            if (Mathf.Abs(localPosition.x) >
                laneTolerance)
            {
                continue;
            }

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
        // PRIORIDAD:
        // VEHÍCULO DELANTE
        // =================================================

        if (vehicleAheadDetected)
        {
            EmergencyStop();
            return;
        }

        // =================================================
        // SEMÁFORO
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
        // SEGUNDA REVISIÓN DE CARRO DELANTE
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
        // SIN MOVIMIENTO
        // =================================================

        if (movementDistance <= 0f)
        {
            EmergencyStop();
            return;
        }

        // =================================================
        // CALCULAR POSICIÓN X / Z
        // =================================================

        Vector3 nextPosition =
            rb.position +
            laneDirection *
            movementDistance;

        // =================================================
        // CALCULAR ALTURA DE LA CALLE
        // =================================================

        float roadY;

        if (TryGetRoadHeight(
            nextPosition,
            out roadY))
        {
            float targetY =
                roadY +
                pivotToColliderBottom +
                roadOffset;

            float difference =
                targetY -
                rb.position.y;

            // =============================================
            // ZONA MUERTA ANTI-TEMBLOR
            // =============================================

            /*
             * Si la diferencia es muy pequeña,
             * no modificamos Y.
             *
             * Esto evita que el carro reaccione
             * a pequeñas imperfecciones del Mesh.
             */
            if (Mathf.Abs(difference) <
                roadHeightDeadZone)
            {
                nextPosition.y =
                    rb.position.y;
            }
            else
            {
                // =========================================
                // SEGUIR UNA SUBIDA O BAJADA REAL
                // =========================================

                nextPosition.y =
                    Mathf.MoveTowards(
                        rb.position.y,
                        targetY,
                        roadHeightFollowSpeed *
                        Time.fixedDeltaTime
                    );
            }
        }
        else
        {
            /*
             * Si por algún motivo no encontró
             * carretera, conserva la altura actual.
             */
            nextPosition.y =
                rb.position.y;
        }

        // =================================================
        // MOVER UNA SOLA VEZ
        // =================================================

        rb.MovePosition(
            nextPosition
        );
    }

    // =====================================================
    // ALTURA PROMEDIO DE LA CARRETERA
    // =====================================================

    private bool TryGetRoadHeight(
        Vector3 position,
        out float roadY)
    {
        roadY =
            position.y;

        float totalHeight =
            0f;

        int validSamples =
            0;

        // =================================================
        // MUESTRA 1: CENTRO
        // =================================================

        if (TryGetSingleRoadHeight(
            position,
            out float centerHeight))
        {
            totalHeight +=
                centerHeight;

            validSamples++;
        }

        // =================================================
        // MUESTRA 2: DELANTE
        // =================================================

        Vector3 frontPosition =
            position +
            laneDirection *
            roadSampleDistance;

        if (TryGetSingleRoadHeight(
            frontPosition,
            out float frontHeight))
        {
            totalHeight +=
                frontHeight;

            validSamples++;
        }

        // =================================================
        // MUESTRA 3: DETRÁS
        // =================================================

        Vector3 backPosition =
            position -
            laneDirection *
            roadSampleDistance;

        if (TryGetSingleRoadHeight(
            backPosition,
            out float backHeight))
        {
            totalHeight +=
                backHeight;

            validSamples++;
        }

        // =================================================
        // NINGUNA MUESTRA ENCONTRÓ CALLE
        // =================================================

        if (validSamples == 0)
        {
            return false;
        }

        // =================================================
        // PROMEDIO
        // =================================================

        roadY =
            totalHeight /
            validSamples;

        return true;
    }

    // =====================================================
    // UNA SOLA MUESTRA DE ALTURA
    // =====================================================

    private bool TryGetSingleRoadHeight(
        Vector3 position,
        out float roadY)
    {
        roadY =
            position.y;

        Vector3 rayOrigin =
            position +
            Vector3.up *
            roadRayHeight;

        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            roadRayHeight +
            roadRayDistance,
            roadLayer,
            QueryTriggerInteraction.Ignore))
        {
            roadY =
                hit.point.y;

            return true;
        }

        return false;
    }

    // =====================================================
    // COLOCAR SOBRE LA CALLE AL NACER
    // =====================================================

    private void SnapImmediatelyToRoad()
    {
        if (rb == null)
            return;

        float roadY;

        if (!TryGetRoadHeight(
            rb.position,
            out roadY))
        {
            return;
        }

        Vector3 correctedPosition =
            rb.position;

        correctedPosition.y =
            roadY +
            pivotToColliderBottom +
            roadOffset;

        rb.position =
            correctedPosition;
    }

    // =====================================================
    // SEMÁFORO
    // =====================================================

    private bool ShouldStopForTrafficLight()
    {
        /*
         * Después de TrafficExitPoint
         * ignora el semáforo.
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

        // StopPoint está detrás.
        if (distanceToStop < 0f)
            return false;

        // Todavía está demasiado lejos.
        if (distanceToStop >
            trafficDetectionDistance)
        {
            return false;
        }

        // SOLO ROJO DETIENE.
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

        /*
         * La altura no afecta la lógica
         * del semáforo.
         */
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
        currentSpeed =
            0f;

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
            Destroy(
                gameObject
            );
        }
        else
        {
            gameObject.SetActive(
                false
            );
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
    // GIZMOS
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        // =================================================
        // SENSOR DE VEHÍCULOS
        // =================================================

        if (showSafetyZone &&
            safetySensorPoint != null)
        {
            Vector3 forward =
                Application.isPlaying
                    ? laneDirection
                    : transform.forward;

            forward.y = 0f;

            if (forward.sqrMagnitude <
                0.001f)
            {
                forward =
                    transform.forward;
            }

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

        // =================================================
        // RAYCASTS DE LA CALLE
        // =================================================

        if (showRoadRay)
        {
            Vector3 forward =
                Application.isPlaying
                    ? laneDirection
                    : transform.forward;

            forward.y = 0f;

            if (forward.sqrMagnitude <
                0.001f)
            {
                forward =
                    transform.forward;
            }

            forward.Normalize();

            // CENTRO
            DrawRoadRay(
                transform.position
            );

            // DELANTE
            DrawRoadRay(
                transform.position +
                forward *
                roadSampleDistance
            );

            // DETRÁS
            DrawRoadRay(
                transform.position -
                forward *
                roadSampleDistance
            );
        }
    }

    // =====================================================
    // DIBUJAR RAY
    // =====================================================

    private void DrawRoadRay(
        Vector3 position)
    {
        Vector3 origin =
            position +
            Vector3.up *
            roadRayHeight;

        Vector3 end =
            origin +
            Vector3.down *
            (roadRayHeight +
             roadRayDistance);

        Gizmos.DrawLine(
            origin,
            end
        );
    }
}