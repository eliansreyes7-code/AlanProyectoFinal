using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Vehicle")]
    [SerializeField] private GameObject vehiclePrefab;

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
        if (vehiclePrefab == null)
        {
            Debug.LogError("VehicleSpawner: falta Vehicle Prefab.");
            return false;
        }

        if (trafficLight == null)
        {
            Debug.LogError("VehicleSpawner: falta Traffic Light.");
            return false;
        }

        if (stopPoint == null)
        {
            Debug.LogError("VehicleSpawner: falta Stop Point.");
            return false;
        }

        if (trafficExitPoint == null)
        {
            Debug.LogError("VehicleSpawner: falta Traffic Exit Point.");
            return false;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("VehicleSpawner: falta Spawn Point.");
            return false;
        }

        if (route == null || route.Length == 0)
        {
            Debug.LogError("VehicleSpawner: la ruta está vacía.");
            return false;
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

        GameObject vehicle = Instantiate(
            vehiclePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        VehicleController controller =
            vehicle.GetComponent<VehicleController>();

        if (controller == null)
        {
            Debug.LogError(
                "VehicleSpawner: el prefab no tiene VehicleController."
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