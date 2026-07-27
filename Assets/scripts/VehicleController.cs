using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [Header("Route")]
    [SerializeField] private Transform[] waypoints;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float acceleration = 4f;
    [SerializeField] private float braking = 8f;
    [SerializeField] private float waypointDistance = 1f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Traffic Light")]
    [SerializeField] private TrafficLightController trafficLight;

    [Tooltip("Punto donde el vehículo debe detenerse.")]
    [SerializeField] private Transform stopPoint;

    [Tooltip("Punto después del cruce. Al alcanzarlo deja de obedecer el semáforo.")]
    [SerializeField] private Transform trafficExitPoint;

    [Tooltip("Distancia desde la que el vehículo empieza a reaccionar al semáforo.")]
    [SerializeField] private float trafficDetectionDistance = 12f;

    [Tooltip("Distancia mínima al Stop Point para detener completamente el vehículo.")]
    [SerializeField] private float stopDistance = 1.25f;

    [Tooltip("Distancia para considerar alcanzado el Traffic Exit Point.")]
    [SerializeField] private float exitPointDistance = 2f;

    [Header("Route End")]
    [SerializeField] private bool destroyAtEnd = true;

    private int currentWaypoint = 0;
    private float currentSpeed = 0f;

    // Cuando sea true, el vehículo ya cruzó la intersección
    // y deja de responder al semáforo.
    private bool passedTrafficLight = false;


    // =====================================================
    // CONFIGURACIÓN DESDE VEHICLE SPAWNER
    // =====================================================

    public void Setup(
        Transform[] newWaypoints,
        TrafficLightController newTrafficLight,
        Transform newStopPoint,
        Transform newTrafficExitPoint)
    {
        waypoints = newWaypoints;

        trafficLight = newTrafficLight;
        stopPoint = newStopPoint;
        trafficExitPoint = newTrafficExitPoint;

        currentWaypoint = 0;
        currentSpeed = 0f;

        passedTrafficLight = false;
    }


    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (currentWaypoint < 0 ||
            currentWaypoint >= waypoints.Length)
            return;

        CheckTrafficExit();

        MoveVehicle();
    }


    // =====================================================
    // MOVIMIENTO
    // =====================================================

    private void MoveVehicle()
    {
        Transform target = waypoints[currentWaypoint];

        if (target == null)
            return;


        bool shouldStop =
            ShouldStopForTrafficLight();


        float targetSpeed =
            shouldStop ? 0f : maxSpeed;


        float changeRate =
            shouldStop ? braking : acceleration;


        // Aceleración y frenado progresivos.
        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            changeRate * Time.deltaTime
        );


        // Dirección hacia el waypoint actual.
        Vector3 direction =
            target.position -
            transform.position;

        direction.y = 0f;


        if (direction.sqrMagnitude > 0.01f)
        {
            direction.Normalize();


            // Rotación hacia el waypoint.
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);


            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );


            // =================================================
            // DETENCIÓN EN ROJO
            // =================================================

            if (shouldStop && stopPoint != null)
            {
                float distanceToStop =
                    HorizontalDistance(
                        transform.position,
                        stopPoint.position
                    );


                /*
                 * Si ya estamos prácticamente en la línea,
                 * detenemos completamente el vehículo.
                 */
                if (distanceToStop <= stopDistance)
                {
                    currentSpeed = 0f;
                    return;
                }
            }


            // Movimiento.
            transform.position +=
                direction *
                currentSpeed *
                Time.deltaTime;
        }


        CheckWaypoint(target);
    }


    // =====================================================
    // SEMÁFORO
    // =====================================================

    private bool ShouldStopForTrafficLight()
    {
        /*
         * Si ya cruzó TrafficExitPoint,
         * el semáforo deja de afectar al vehículo.
         */
        if (passedTrafficLight)
            return false;


        if (trafficLight == null ||
            stopPoint == null)
            return false;


        float distanceToStop =
            HorizontalDistance(
                transform.position,
                stopPoint.position
            );


        /*
         * Si todavía está lejos del semáforo,
         * continúa normalmente.
         */
        if (distanceToStop > trafficDetectionDistance)
            return false;


        // =================================================
        // VERDE
        // =================================================

        if (trafficLight.CurrentState ==
            TrafficLightController.TrafficState.Green)
        {
            return false;
        }


        // =================================================
        // AMARILLO
        // =================================================
        //
        // El vehículo NO frena.
        // Continúa normalmente.
        // =================================================

        if (trafficLight.CurrentState ==
            TrafficLightController.TrafficState.Yellow)
        {
            return false;
        }


        // =================================================
        // ROJO
        // =================================================
        //
        // Esta es la ÚNICA luz que hace frenar al vehículo.
        // =================================================

        if (trafficLight.CurrentState ==
            TrafficLightController.TrafficState.Red)
        {
            return true;
        }


        return false;
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


        float distance =
            HorizontalDistance(
                transform.position,
                trafficExitPoint.position
            );


        /*
         * Cuando el vehículo llega a TrafficExitPoint,
         * consideramos que terminó de cruzar.
         */
        if (distance <= exitPointDistance)
        {
            passedTrafficLight = true;
        }
    }


    // =====================================================
    // WAYPOINTS
    // =====================================================

    private void CheckWaypoint(Transform target)
    {
        float distance =
            HorizontalDistance(
                transform.position,
                target.position
            );


        if (distance > waypointDistance)
            return;


        // Si es el último waypoint:
        if (currentWaypoint ==
            waypoints.Length - 1)
        {
            FinishRoute();
            return;
        }


        // Pasamos al siguiente waypoint.
        currentWaypoint++;
    }


    // =====================================================
    // DISTANCIA HORIZONTAL
    // =====================================================

    private float HorizontalDistance(
        Vector3 positionA,
        Vector3 positionB)
    {
        positionA.y = 0f;
        positionB.y = 0f;


        return Vector3.Distance(
            positionA,
            positionB
        );
    }


    // =====================================================
    // FINAL DE LA RUTA
    // =====================================================

    private void FinishRoute()
    {
        currentSpeed = 0f;


        if (destroyAtEnd)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}