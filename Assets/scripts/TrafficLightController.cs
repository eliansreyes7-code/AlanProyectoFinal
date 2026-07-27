using System.Collections;
using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    public enum TrafficState
    {
        Green,
        Yellow,
        Red
    }

    [Header("Vehicle Traffic Light")]
    [SerializeField] private Renderer vehicleRedLight;
    [SerializeField] private Renderer vehicleYellowLight;
    [SerializeField] private Renderer vehicleGreenLight;

    [Header("Pedestrian Traffic Light")]
    [SerializeField] private Renderer pedestrianStopLight;
    [SerializeField] private Renderer pedestrianWalkLight;

    [Header("Timing")]
    [SerializeField] private float greenDuration = 8f;
    [SerializeField] private float yellowDuration = 2f;

    // Tiempo disponible para cruzar.
    [SerializeField] private float redDuration = 3f;

    [Header("Visual")]
    [SerializeField] private float emissionIntensity = 3f;
    [SerializeField] private float inactiveIntensity = 0.12f;

    private TrafficState currentState;

    public TrafficState CurrentState => currentState;

    private Material vehicleRedMaterial;
    private Material vehicleYellowMaterial;
    private Material vehicleGreenMaterial;

    private Material pedestrianStopMaterial;
    private Material pedestrianWalkMaterial;

    private void Awake()
    {
        // Materiales del semáforo de vehículos
        if (vehicleRedLight != null)
            vehicleRedMaterial = vehicleRedLight.material;

        if (vehicleYellowLight != null)
            vehicleYellowMaterial = vehicleYellowLight.material;

        if (vehicleGreenLight != null)
            vehicleGreenMaterial = vehicleGreenLight.material;

        // Materiales del semáforo peatonal
        if (pedestrianStopLight != null)
            pedestrianStopMaterial = pedestrianStopLight.material;

        if (pedestrianWalkLight != null)
            pedestrianWalkMaterial = pedestrianWalkLight.material;
    }

    private void Start()
    {
        StartCoroutine(TrafficLightCycle());
    }

    private IEnumerator TrafficLightCycle()
    {
        while (true)
        {
            // VEHÍCULOS AVANZAN
            // PEATÓN NO CRUZA
            SetState(TrafficState.Green);
            yield return new WaitForSeconds(greenDuration);

            // VEHÍCULOS PREPARÁNDOSE PARA DETENERSE
            // PEATÓN TODAVÍA NO CRUZA
            SetState(TrafficState.Yellow);
            yield return new WaitForSeconds(yellowDuration);

            // VEHÍCULOS DETENIDOS
            // PEATÓN PUEDE CRUZAR
            SetState(TrafficState.Red);
            yield return new WaitForSeconds(redDuration);
        }
    }

    private void SetState(TrafficState newState)
    {
        currentState = newState;

        UpdateVehicleLights();
        UpdatePedestrianLights();
    }

    private void UpdateVehicleLights()
    {
        SetLight(
            vehicleRedMaterial,
            Color.red,
            currentState == TrafficState.Red
        );

        SetLight(
            vehicleYellowMaterial,
            Color.yellow,
            currentState == TrafficState.Yellow
        );

        SetLight(
            vehicleGreenMaterial,
            Color.green,
            currentState == TrafficState.Green
        );
    }

    private void UpdatePedestrianLights()
    {
        bool pedestrianCanCross =
            currentState == TrafficState.Red;

        // Cuando los vehículos tienen verde o amarillo:
        // peatón ve rojo.
        SetLight(
            pedestrianStopMaterial,
            Color.red,
            !pedestrianCanCross
        );

        // Cuando los vehículos tienen rojo:
        // peatón ve verde.
        SetLight(
            pedestrianWalkMaterial,
            Color.green,
            pedestrianCanCross
        );
    }

    private void SetLight(
        Material material,
        Color color,
        bool active
    )
    {
        if (material == null)
            return;

        material.EnableKeyword("_EMISSION");

        if (active)
        {
            material.color = color;

            material.SetColor(
                "_EmissionColor",
                color * emissionIntensity
            );
        }
        else
        {
            Color inactiveColor =
                color * inactiveIntensity;

            material.color = inactiveColor;

            material.SetColor(
                "_EmissionColor",
                inactiveColor
            );
        }
    }
}