using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Vehicles")]
    [Tooltip("Prefabs de vehículos que pueden aparecer aleatoriamente.")]
    [SerializeField] private GameObject[] vehiclePrefabs;

    [Header("Traffic System")]
    [SerializeField] private TrafficLightController trafficLight;
    [SerializeField] private Transform stopPoint;
    [SerializeField] private Transform trafficExitPoint;

    [Header("Route")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform[] route;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 4f;
    [SerializeField] private float minimumSpawnDistance = 6f;
    [SerializeField] private int maxVehicles = 5;

    [Header("Initial Spawn")]
    [SerializeField] private bool spawnImmediately = true;

    private readonly List<GameObject> activeVehicles =
        new List<GameObject>();

    private void Start()
    {
        if (!ValidateConfiguration())
        {
            enabled = false;
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    private bool ValidateConfiguration()
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0)
        {
            Debug.LogError(
                "VehicleSpawner: no hay prefabs de vehículos asignados.",
                this
            );

            return false;
        }

        for (int i = 0; i < vehiclePrefabs.Length; i++)
        {
            if (vehiclePrefabs[i] == null)
            {
                Debug.LogError(
                    "VehicleSpawner: el elemento " + i +
                    " de Vehicle Prefabs está vacío.",
                    this
                );

                return false;
            }

            if (vehiclePrefabs[i].GetComponent<VehicleController>() == null)
            {
                Debug.LogError(
                    "VehicleSpawner: el prefab " +
                    vehiclePrefabs[i].name +
                    " no tiene VehicleController en el objeto raíz.",
                    vehiclePrefabs[i]
                );

                return false;
            }
        }

        if (trafficLight == null)
        {
            Debug.LogError("VehicleSpawner: falta Traffic Light.", this);
            return false;
        }

        if (stopPoint == null)
        {
            Debug.LogError("VehicleSpawner: falta Stop Point.", this);
            return false;
        }

        if (trafficExitPoint == null)
        {
            Debug.LogError("VehicleSpawner: falta Traffic Exit Point.", this);
            return false;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("VehicleSpawner: falta Spawn Point.", this);
            return false;
        }

        if (route == null || route.Length == 0)
        {
            Debug.LogError("VehicleSpawner: la ruta está vacía.", this);
            return false;
        }

        for (int i = 0; i < route.Length; i++)
        {
            if (route[i] == null)
            {
                Debug.LogError(
                    "VehicleSpawner: el punto " + i +
                    " de la ruta está vacío.",
                    this
                );

                return false;
            }
        }

        return true;
    }

    private IEnumerator SpawnRoutine()
    {
        if (spawnImmediately)
            TrySpawnVehicle();

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            CleanVehicleList();

            if (activeVehicles.Count < maxVehicles)
                TrySpawnVehicle();
        }
    }

    private void TrySpawnVehicle()
    {
        if (activeVehicles.Count >= maxVehicles)
            return;

        if (!CanSpawn())
            return;

        int randomIndex = Random.Range(0, vehiclePrefabs.Length);
        GameObject selectedPrefab = vehiclePrefabs[randomIndex];

        GameObject vehicle = Instantiate(
            selectedPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        VehicleController controller =
            vehicle.GetComponent<VehicleController>();

        if (controller == null)
        {
            Debug.LogError(
                "VehicleSpawner: el vehículo generado no tiene " +
                "VehicleController en el objeto raíz.",
                vehicle
            );

            Destroy(vehicle);
            return;
        }

        controller.Setup(
            route,
            trafficLight,
            stopPoint,
            trafficExitPoint
        );

        activeVehicles.Add(vehicle);
    }

    private bool CanSpawn()
    {
        foreach (GameObject vehicle in activeVehicles)
        {
            if (vehicle == null)
                continue;

            Vector3 spawnPosition = spawnPoint.position;
            Vector3 vehiclePosition = vehicle.transform.position;

            spawnPosition.y = 0f;
            vehiclePosition.y = 0f;

            float distance = Vector3.Distance(
                spawnPosition,
                vehiclePosition
            );

            if (distance < minimumSpawnDistance)
                return false;
        }

        return true;
    }

    private void CleanVehicleList()
    {
        activeVehicles.RemoveAll(
            vehicle => vehicle == null
        );
    }
}