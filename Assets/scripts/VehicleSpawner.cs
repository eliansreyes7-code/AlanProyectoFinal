using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    // =====================================================
    // SOLO PUEDE EXISTIR UN SPAWNER ACTIVO
    // =====================================================

    private static VehicleSpawner activeSpawner;

    // =====================================================
    // VEHÍCULO
    // =====================================================

    [Header("Vehicle")]
    [SerializeField] private GameObject vehiclePrefab;

    // =====================================================
    // SEMÁFORO
    // =====================================================

    [Header("Traffic System")]
    [SerializeField] private TrafficLightController trafficLight;
    [SerializeField] private Transform stopPoint;
    [SerializeField] private Transform trafficExitPoint;

    // =====================================================
    // PLAYER HIT / RESPAWN
    // =====================================================

    [Header("Player Hit / Respawn")]
    [SerializeField] private Transform playerRespawnPoint;
    [SerializeField] private CanvasGroup whiteFlash;

    // =====================================================
    // CARRIL 01
    // =====================================================

    [Header("Lane 01")]
    [SerializeField] private Transform spawnPoint01;
    [SerializeField] private Transform endPoint01;

    // =====================================================
    // CARRIL 02
    // =====================================================

    [Header("Lane 02")]
    [SerializeField] private Transform spawnPoint02;
    [SerializeField] private Transform endPoint02;

    // =====================================================
    // SPAWN
    // =====================================================

    [Header("Spawn Settings")]

    [Tooltip("Tiempo exacto entre un intento de spawn y el siguiente.")]
    [SerializeField] private float spawnInterval = 4f;

    [Tooltip("El último carro del carril debe alejarse esta distancia antes de permitir otro.")]
    [SerializeField] private float minimumSpawnDistance = 8f;

    [Tooltip("Cantidad máxima de carros vivos simultáneamente.")]
    [SerializeField] private int maxVehicles = 6;

    [Tooltip("Crear un primer carro al comenzar.")]
    [SerializeField] private bool spawnImmediately = true;

    // =====================================================
    // IDENTIFICACIÓN
    // =====================================================

    [Header("Vehicle Identification")]
    [SerializeField] private string vehicleTag = "Carro";
    [SerializeField] private string vehicleLayerName = "car";

    // =====================================================
    // DEBUG
    // =====================================================

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    // =====================================================
    // VARIABLES INTERNAS
    // =====================================================

    private readonly List<GameObject> activeVehicles =
        new List<GameObject>();

    private GameObject lastVehicleLane01;
    private GameObject lastVehicleLane02;

    private int vehicleLayer = -1;

    // true  = próximo intento Lane 01
    // false = próximo intento Lane 02
    private bool nextLaneIs01 = true;

    private Coroutine spawnCoroutine;

    private int totalSpawned = 0;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        /*
         * PROTECCIÓN ABSOLUTA:
         *
         * Si existe otro VehicleSpawner activo,
         * este se desactiva.
         *
         * Esto también protege si accidentalmente
         * VehicleSpawner terminó dentro del prefab
         * del carro.
         */

        if (activeSpawner != null &&
            activeSpawner != this)
        {
            Debug.LogWarning(
                "VehicleSpawner DUPLICADO DESACTIVADO -> " +
                gameObject.name,
                gameObject
            );

            enabled = false;
            return;
        }

        activeSpawner = this;
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        if (!enabled)
            return;

        if (!ValidateConfiguration())
        {
            enabled = false;
            return;
        }

        vehicleLayer =
            LayerMask.NameToLayer(
                vehicleLayerName
            );

        if (vehicleLayer == -1)
        {
            Debug.LogError(
                "VehicleSpawner: no existe la Layer '" +
                vehicleLayerName +
                "'."
            );

            enabled = false;
            return;
        }

        /*
         * Una sola Coroutine.
         */
        if (spawnCoroutine == null)
        {
            spawnCoroutine =
                StartCoroutine(
                    SpawnRoutine()
                );
        }
    }

    // =====================================================
    // VALIDACIÓN
    // =====================================================

    private bool ValidateConfiguration()
    {
        if (vehiclePrefab == null)
        {
            Debug.LogError(
                "VehicleSpawner: falta Vehicle Prefab."
            );

            return false;
        }

        if (spawnPoint01 == null)
        {
            Debug.LogError(
                "VehicleSpawner: falta Spawn Point 01."
            );

            return false;
        }

        if (endPoint01 == null)
        {
            Debug.LogError(
                "VehicleSpawner: falta End Point 01."
            );

            return false;
        }

        return true;
    }

    // =====================================================
    // CORRUTINA
    // =====================================================

    private IEnumerator SpawnRoutine()
    {
        // =================================================
        // PRIMER CARRO
        // =================================================

        if (spawnImmediately)
        {
            TrySpawnCurrentLane();
        }

        // =================================================
        // RESTO DE LOS CARROS
        // =================================================

        while (true)
        {
            /*
             * SIEMPRE espera el intervalo completo.
             */
            yield return new WaitForSeconds(
                spawnInterval
            );

            CleanVehicleList();

            if (activeVehicles.Count >= maxVehicles)
            {
                if (showDebugMessages)
                {
                    Debug.Log(
                        "NO SPAWN -> máximo alcanzado: " +
                        activeVehicles.Count
                    );
                }

                continue;
            }

            /*
             * Una llamada.
             *
             * Esta llamada puede generar
             * como máximo UN carro.
             */
            TrySpawnCurrentLane();
        }
    }

    // =====================================================
    // INTENTAR EL CARRIL QUE TOCA
    // =====================================================

    private void TrySpawnCurrentLane()
    {
        bool lane02Exists =
            spawnPoint02 != null &&
            endPoint02 != null;

        // =================================================
        // SOLO HAY LANE 01
        // =================================================

        if (!lane02Exists)
        {
            TrySpawn(
                spawnPoint01,
                endPoint01,
                ref lastVehicleLane01,
                "Lane 01"
            );

            return;
        }

        // =================================================
        // TOCA LANE 01
        // =================================================

        if (nextLaneIs01)
        {
            bool spawned =
                TrySpawn(
                    spawnPoint01,
                    endPoint01,
                    ref lastVehicleLane01,
                    "Lane 01"
                );

            /*
             * Alternamos aunque el spawn no se haya
             * podido realizar.
             *
             * Así jamás intenta Lane 02
             * durante este mismo ciclo.
             */
            nextLaneIs01 = false;

            if (!spawned &&
                showDebugMessages)
            {
                Debug.Log(
                    "Lane 01 ocupada. " +
                    "Esperaremos al próximo intervalo."
                );
            }

            return;
        }

        // =================================================
        // TOCA LANE 02
        // =================================================

        bool lane02Spawned =
            TrySpawn(
                spawnPoint02,
                endPoint02,
                ref lastVehicleLane02,
                "Lane 02"
            );

        nextLaneIs01 = true;

        if (!lane02Spawned &&
            showDebugMessages)
        {
            Debug.Log(
                "Lane 02 ocupada. " +
                "Esperaremos al próximo intervalo."
            );
        }
    }

    // =====================================================
    // INTENTAR CREAR UN ÚNICO CARRO
    // =====================================================

    private bool TrySpawn(
        Transform spawnPoint,
        Transform endPoint,
        ref GameObject lastVehicle,
        string laneName)
    {
        if (spawnPoint == null ||
            endPoint == null)
        {
            return false;
        }

        CleanVehicleList();

        if (activeVehicles.Count >= maxVehicles)
        {
            return false;
        }

        // =================================================
        // COMPROBAR ÚLTIMO CARRO DEL CARRIL
        // =================================================

        if (lastVehicle != null)
        {
            Vector3 spawnPosition =
                spawnPoint.position;

            Vector3 lastVehiclePosition =
                lastVehicle.transform.position;

            spawnPosition.y = 0f;
            lastVehiclePosition.y = 0f;

            float distance =
                Vector3.Distance(
                    spawnPosition,
                    lastVehiclePosition
                );

            if (distance <
                minimumSpawnDistance)
            {
                if (showDebugMessages)
                {
                    Debug.Log(
                        laneName +
                        " -> NO SPAWN. Último carro a " +
                        distance.ToString("F2") +
                        "m"
                    );
                }

                return false;
            }
        }

        // =================================================
        // ÚNICO INSTANTIATE
        // =================================================

        GameObject vehicle =
            Instantiate(
                vehiclePrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        if (vehicle == null)
            return false;

        totalSpawned++;

        // =================================================
        // TAG
        // =================================================

        try
        {
            vehicle.tag =
                vehicleTag;
        }
        catch (UnityException)
        {
            Debug.LogError(
                "No existe el Tag '" +
                vehicleTag +
                "'."
            );

            Destroy(vehicle);

            return false;
        }

        // =================================================
        // LAYER
        // =================================================

        SetLayerRecursively(
            vehicle.transform,
            vehicleLayer
        );

        // =================================================
        // VEHICLE CONTROLLER
        // =================================================

        VehicleController controller =
            vehicle.GetComponent<VehicleController>();

        if (controller == null)
        {
            Debug.LogError(
                vehicle.name +
                " no tiene VehicleController."
            );

            Destroy(vehicle);

            return false;
        }

        controller.Setup(
            endPoint,
            trafficLight,
            stopPoint,
            trafficExitPoint
        );

        // =================================================
        // PLAYER HIT
        // =================================================

        VehiclePlayerHit playerHit =
            vehicle.GetComponentInChildren
            <VehiclePlayerHit>(true);

        if (playerHit != null)
        {
            playerHit.SetupHitSystem(
                playerRespawnPoint,
                whiteFlash
            );
        }

        // =================================================
        // REGISTRAR
        // =================================================

        activeVehicles.Add(
            vehicle
        );

        lastVehicle =
            vehicle;

        if (showDebugMessages)
        {
            Debug.Log(
                "CARRO #" +
                totalSpawned +
                " CREADO | " +
                laneName +
                " | Tiempo: " +
                Time.time.ToString("F2") +
                " | Activos: " +
                activeVehicles.Count,
                vehicle
            );
        }

        return true;
    }

    // =====================================================
    // ASIGNAR LAYER
    // =====================================================

    private void SetLayerRecursively(
        Transform root,
        int newLayer)
    {
        if (root == null)
            return;

        root.gameObject.layer =
            newLayer;

        foreach (Transform child in root)
        {
            SetLayerRecursively(
                child,
                newLayer
            );
        }
    }

    // =====================================================
    // LIMPIAR VEHÍCULOS DESTRUIDOS
    // =====================================================

    private void CleanVehicleList()
    {
        activeVehicles.RemoveAll(
            vehicle =>
                vehicle == null
        );
    }

    // =====================================================
    // CUANDO SE DESTRUYE EL SPAWNER
    // =====================================================

    private void OnDestroy()
    {
        if (activeSpawner == this)
        {
            activeSpawner = null;
        }
    }
}