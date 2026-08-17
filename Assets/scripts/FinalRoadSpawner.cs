using System.Collections;
using UnityEngine;

public class FinalRoadSpawner : MonoBehaviour
{
    [Header("Vehicle")]
    [SerializeField] private GameObject carPrefab;

    [Header("Route")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform endPoint;

    [Header("Spawn Settings")]
    [Tooltip("Tiempo entre cada carro.")]
    [SerializeField] private float spawnInterval = 2f;

    [Tooltip("Crear un carro inmediatamente al iniciar Play.")]
    [SerializeField] private bool spawnImmediately = true;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private Coroutine spawnRoutine;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        // El componente siempre intenta estar habilitado.
        enabled = true;

        if (showDebug)
        {
            Debug.Log(
                "FinalRoadSpawner: AWAKE -> componente activo.",
                gameObject
            );
        }
    }

    // =====================================================
    // ON ENABLE
    // =====================================================

    private void OnEnable()
    {
        /*
         * Si por alguna razón el objeto se vuelve
         * a activar durante el juego, volvemos
         * a iniciar el tráfico automáticamente.
         */

        if (Application.isPlaying)
        {
            TryStartTraffic();
        }
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        if (showDebug)
        {
            Debug.Log(
                "FINAL ROAD SPAWNER -> INICIADO",
                gameObject
            );
        }

        TryStartTraffic();
    }

    // =====================================================
    // INICIAR TRÁFICO
    // =====================================================

    private void TryStartTraffic()
    {
        if (!Application.isPlaying)
            return;

        if (spawnRoutine != null)
            return;

        if (!ValidateConfiguration())
            return;

        spawnRoutine =
            StartCoroutine(
                SpawnRoutine()
            );

        if (showDebug)
        {
            Debug.Log(
                "FinalRoadSpawner: tráfico ACTIVADO.",
                gameObject
            );
        }
    }

    // =====================================================
    // VALIDAR
    // =====================================================

    private bool ValidateConfiguration()
    {
        bool valid = true;

        if (carPrefab == null)
        {
            Debug.LogError(
                "FinalRoadSpawner: falta Car Prefab.",
                gameObject
            );

            valid = false;
        }

        if (spawnPoint == null)
        {
            Debug.LogError(
                "FinalRoadSpawner: falta Spawn Point.",
                gameObject
            );

            valid = false;
        }

        if (endPoint == null)
        {
            Debug.LogError(
                "FinalRoadSpawner: falta End Point.",
                gameObject
            );

            valid = false;
        }

        /*
         * IMPORTANTE:
         * NO usamos enabled = false.
         * El componente permanece activo.
         */

        return valid;
    }

    // =====================================================
    // SPAWN LOOP
    // =====================================================

    private IEnumerator SpawnRoutine()
    {
        // Primer carro inmediatamente.
        if (spawnImmediately)
        {
            SpawnCar();
        }

        while (true)
        {
            yield return new WaitForSeconds(
                Mathf.Max(
                    0.1f,
                    spawnInterval
                )
            );

            SpawnCar();
        }
    }

    // =====================================================
    // CREAR CARRO
    // =====================================================

    private void SpawnCar()
    {
        if (carPrefab == null ||
            spawnPoint == null ||
            endPoint == null)
        {
            return;
        }

        GameObject car =
            Instantiate(
                carPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        if (showDebug)
        {
            Debug.Log(
                "FINAL ROAD -> CARRO CREADO: " +
                car.name +
                " | Tiempo: " +
                Time.time.ToString("F2"),
                car
            );
        }

        FinalRoadCar controller =
            car.GetComponent<FinalRoadCar>();

        if (controller == null)
        {
            controller =
                car.GetComponentInChildren<FinalRoadCar>();
        }

        if (controller == null)
        {
            Debug.LogError(
                "FinalRoadSpawner: el prefab '" +
                car.name +
                "' no tiene FinalRoadCar.",
                car
            );

            Destroy(car);
            return;
        }

        controller.Setup(
            endPoint
        );
    }

    // =====================================================
    // SI SE DESACTIVA EL COMPONENTE
    // =====================================================

    private void OnDisable()
    {
        spawnRoutine = null;

        if (showDebug &&
            Application.isPlaying)
        {
            Debug.LogWarning(
                "FinalRoadSpawner fue desactivado.",
                gameObject
            );
        }
    }
}