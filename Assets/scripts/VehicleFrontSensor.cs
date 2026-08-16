using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class VehicleFrontSensor : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Máxima diferencia lateral para considerar que otro carro está en el mismo carril.")]
    [SerializeField] private float laneTolerance = 1.6f;

    private BoxCollider sensorCollider;
    private VehicleController myVehicle;

    private bool vehicleDetected;
    private VehicleController vehicleAhead;

    private float distanceToVehicle =
        Mathf.Infinity;

    public bool VehicleDetected =>
        vehicleDetected;

    public VehicleController VehicleAhead =>
        vehicleAhead;

    public float DistanceToVehicle =>
        distanceToVehicle;

    private void Awake()
    {
        sensorCollider =
            GetComponent<BoxCollider>();

        myVehicle =
            GetComponentInParent<VehicleController>();

        if (sensorCollider != null)
        {
            sensorCollider.isTrigger = true;
        }
    }

    private void FixedUpdate()
    {
        DetectVehicleAhead();
    }

    // =====================================================
    // DETECCIÓN
    // =====================================================

    private void DetectVehicleAhead()
    {
        vehicleDetected = false;
        vehicleAhead = null;
        distanceToVehicle = Mathf.Infinity;

        if (sensorCollider == null ||
            myVehicle == null)
        {
            return;
        }

        Vector3 worldCenter =
            transform.TransformPoint(
                sensorCollider.center
            );

        Vector3 scale =
            transform.lossyScale;

        Vector3 halfExtents =
            new Vector3(
                sensorCollider.size.x *
                Mathf.Abs(scale.x) * 0.5f,

                sensorCollider.size.y *
                Mathf.Abs(scale.y) * 0.5f,

                sensorCollider.size.z *
                Mathf.Abs(scale.z) * 0.5f
            );

        Collider[] hits =
            Physics.OverlapBox(
                worldCenter,
                halfExtents,
                transform.rotation,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide
            );

        float closestDistance =
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

            // Ignorar nuestro propio carro.
            if (other == myVehicle)
                continue;

            // Posición del otro carro respecto al nuestro.
            Vector3 localPosition =
                myVehicle.transform
                    .InverseTransformPoint(
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

            /*
             * Distancia hacia delante.
             * No usamos Vector3.Distance general porque
             * queremos la separación longitudinal.
             */
            float forwardDistance =
                localPosition.z;

            if (forwardDistance <
                closestDistance)
            {
                closestDistance =
                    forwardDistance;

                vehicleAhead =
                    other;
            }
        }

        if (vehicleAhead != null)
        {
            vehicleDetected = true;

            distanceToVehicle =
                closestDistance;
        }
    }
}