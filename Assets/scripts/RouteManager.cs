using UnityEngine;

public class RouteManager : MonoBehaviour
{
    // =====================================================
    // CHECKPOINTS
    // =====================================================

    [Header("Checkpoints")]
    [SerializeField] private Checkpoint checkpoint1;
    [SerializeField] private Checkpoint checkpoint2;
    [SerializeField] private Checkpoint finalRoadCheckpoint;

    // =====================================================
    // DOG CHALLENGE
    // =====================================================

    [Header("Dog Challenge")]
    [SerializeField] private DogChallengeManager dogChallengeManager;

    // =====================================================
    // PLAYER
    // =====================================================

    [Header("Player")]
    [SerializeField] private Transform player;

    // =====================================================
    // ROUTE LINE
    // =====================================================

    [Header("Route Line")]
    [SerializeField] private LineRenderer routeLine;

    [SerializeField] private float routeGroundHeight = 0.03f;

    // =====================================================
    // PULSE
    // =====================================================

    [Header("Route Pulse")]
    [SerializeField] private float pulseSpeed = 2f;

    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.08f;

    [Range(0f, 1f)]
    [SerializeField] private float maxAlpha = 0.30f;

    // =====================================================
    // VARIABLES
    // =====================================================

    private Checkpoint[] checkpoints;

    private int currentCheckpointIndex = 0;

    private bool routePaused = false;
    private bool waitingForBone = false;
    private bool routeCompleted = false;

    private Material routeMaterial;

    // =====================================================
    // CURRENT CHECKPOINT
    // =====================================================

    public Checkpoint CurrentCheckpoint
    {
        get
        {
            if (checkpoints == null ||
                checkpoints.Length == 0)
            {
                return null;
            }

            if (currentCheckpointIndex < 0 ||
                currentCheckpointIndex >= checkpoints.Length)
            {
                return null;
            }

            return checkpoints[currentCheckpointIndex];
        }
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        // Crear internamente la ruta exacta.
        checkpoints = new Checkpoint[]
        {
            checkpoint1,
            checkpoint2,
            finalRoadCheckpoint
        };

        // =================================================
        // VALIDAR
        // =================================================

        if (checkpoint1 == null)
        {
            Debug.LogError(
                "RouteManager: falta Checkpoint 1."
            );

            return;
        }

        if (checkpoint2 == null)
        {
            Debug.LogError(
                "RouteManager: falta Checkpoint 2."
            );

            return;
        }

        if (finalRoadCheckpoint == null)
        {
            Debug.LogError(
                "RouteManager: falta Final Road Checkpoint."
            );

            return;
        }

        // =================================================
        // PLAYER
        // =================================================

        if (player == null)
        {
            WheelchairController wheelchair =
                FindFirstObjectByType<WheelchairController>();

            if (wheelchair != null)
            {
                player = wheelchair.transform;
            }
            else
            {
                Debug.LogError(
                    "RouteManager: no encontró al jugador."
                );
            }
        }

        // =================================================
        // DOG MANAGER
        // =================================================

        if (dogChallengeManager == null)
        {
            dogChallengeManager =
                FindFirstObjectByType<DogChallengeManager>();
        }

        if (dogChallengeManager == null)
        {
            Debug.LogWarning(
                "RouteManager: no encontró DogChallengeManager."
            );
        }

        // =================================================
        // ROUTE LINE
        // =================================================

        if (routeLine != null)
        {
            routeMaterial =
                routeLine.material;

            routeLine.enabled = true;
        }
        else
        {
            Debug.LogWarning(
                "RouteManager: falta Route Line."
            );
        }

        // =================================================
        // INICIALIZAR CHECKPOINTS
        // =================================================

        for (int i = 0;
             i < checkpoints.Length;
             i++)
        {
            if (checkpoints[i] == null)
                continue;

            checkpoints[i].Initialize(
                this,
                i
            );

            checkpoints[i]
                .SetCheckpointActive(false);
        }

        // =================================================
        // EMPEZAR
        // =================================================

        currentCheckpointIndex = 0;

        routePaused = false;
        waitingForBone = false;
        routeCompleted = false;

        ActivateCurrentCheckpoint();
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (routeCompleted)
        {
            HideRouteLine();
            return;
        }

        if (routePaused)
        {
            HideRouteLine();
            return;
        }

        UpdateRouteLine();
        UpdateRoutePulse();
    }

    // =====================================================
    // ACTIVAR CHECKPOINT
    // =====================================================

    private void ActivateCurrentCheckpoint()
    {
        if (CurrentCheckpoint == null)
            return;

        CurrentCheckpoint
            .SetCheckpointActive(true);

        if (routeLine != null)
        {
            routeLine.enabled = true;
        }

        Debug.Log(
            "RouteManager: checkpoint activo = " +
            currentCheckpointIndex
        );
    }

    // =====================================================
    // REACH CHECKPOINT
    // =====================================================

    public void ReachCheckpoint(
        int index)
    {
        if (routeCompleted)
            return;

        if (index != currentCheckpointIndex)
            return;

        if (CurrentCheckpoint == null)
            return;

        // Ocultarlo.
        CurrentCheckpoint
            .SetCheckpointActive(false);

        // =================================================
        // CHECKPOINT 1
        // =================================================

        if (currentCheckpointIndex == 0)
        {
            currentCheckpointIndex = 1;

            ActivateCurrentCheckpoint();

            Debug.Log(
                "Checkpoint 1 completado -> Checkpoint 2."
            );

            return;
        }

        // =================================================
        // CHECKPOINT 2
        // =================================================

        if (currentCheckpointIndex == 1)
        {
            /*
             * Dejamos preparado internamente
             * FinalRoadCheckpoint.
             *
             * Pero NO lo mostramos todavía.
             */

            currentCheckpointIndex = 2;

            routePaused = true;
            waitingForBone = true;

            HideRouteLine();

            // =============================================
            // MENSAJE DEL PERRO
            // =============================================

            if (dogChallengeManager != null)
            {
                dogChallengeManager
                    .ShowDistractDogMessage();
            }

            Debug.Log(
                "Checkpoint 2 completado -> " +
                "distrae al perro."
            );

            return;
        }

        // =================================================
        // FINAL ROAD CHECKPOINT
        // =================================================

        if (currentCheckpointIndex == 2)
        {
            CompleteRoute();

            return;
        }
    }

    // =====================================================
    // EL HUESO YA FUE USADO
    // =====================================================

    public void BoneWasThrown()
    {
        if (!waitingForBone)
            return;

        waitingForBone = false;
        routePaused = false;

        /*
         * currentCheckpointIndex ya vale 2.
         *
         * Activamos:
         * FinalRoadCheckpoint.
         */

        ActivateCurrentCheckpoint();

        if (dogChallengeManager != null)
        {
            dogChallengeManager
                .ShowGoToFinalPointMessage();
        }

        Debug.Log(
            "Hueso completado -> " +
            "FinalRoadCheckpoint ACTIVADO."
        );
    }

    // =====================================================
    // REINICIAR RUTA POR ATROPELLO
    // =====================================================

    public void ResetRouteToStart()
    {
        /*
         * Ocultar absolutamente todos los checkpoints.
         */

        if (checkpoint1 != null)
        {
            checkpoint1
                .SetCheckpointActive(false);
        }

        if (checkpoint2 != null)
        {
            checkpoint2
                .SetCheckpointActive(false);
        }

        if (finalRoadCheckpoint != null)
        {
            finalRoadCheckpoint
                .SetCheckpointActive(false);
        }

        // =================================================
        // REINICIAR ESTADOS
        // =================================================

        currentCheckpointIndex = 0;

        routePaused = false;
        waitingForBone = false;
        routeCompleted = false;

        // =================================================
        // VOLVER AL CHECKPOINT 1
        // =================================================

        if (checkpoint1 != null)
        {
            checkpoint1
                .SetCheckpointActive(true);
        }

        // =================================================
        // VOLVER A MOSTRAR LA GUÍA
        // =================================================

        if (routeLine != null)
        {
            routeLine.enabled = true;
        }

        Debug.Log(
            "RouteManager: ruta reiniciada -> Checkpoint 1."
        );
    }

    // =====================================================
    // ROUTE LINE
    // =====================================================

    private void UpdateRouteLine()
    {
        if (routeLine == null)
            return;

        if (player == null ||
            CurrentCheckpoint == null)
        {
            routeLine.enabled = false;
            return;
        }

        if (routePaused)
        {
            routeLine.enabled = false;
            return;
        }

        Transform target =
            CurrentCheckpoint.TargetTransform;

        if (target == null)
        {
            routeLine.enabled = false;
            return;
        }

        routeLine.enabled = true;

        routeLine.positionCount = 2;

        Vector3 startPosition =
            player.position;

        Vector3 endPosition =
            target.position;

        startPosition.y =
            routeGroundHeight;

        endPosition.y =
            routeGroundHeight;

        routeLine.SetPosition(
            0,
            startPosition
        );

        routeLine.SetPosition(
            1,
            endPosition
        );
    }

    // =====================================================
    // PULSE
    // =====================================================

    private void UpdateRoutePulse()
    {
        if (routeMaterial == null)
            return;

        float pulse =
            (
                Mathf.Sin(
                    Time.time *
                    pulseSpeed
                )
                + 1f
            )
            * 0.5f;

        float alpha =
            Mathf.Lerp(
                minAlpha,
                maxAlpha,
                pulse
            );

        Color color =
            routeMaterial.color;

        color.a = alpha;

        routeMaterial.color =
            color;
    }

    // =====================================================
    // HIDE LINE
    // =====================================================

    private void HideRouteLine()
    {
        if (routeLine != null)
        {
            routeLine.enabled = false;
        }
    }

    // =====================================================
    // COMPLETE
    // =====================================================

    private void CompleteRoute()
    {
        routeCompleted = true;
        routePaused = true;

        if (finalRoadCheckpoint != null)
        {
            finalRoadCheckpoint
                .SetCheckpointActive(false);
        }

        HideRouteLine();

        Debug.Log(
            "FinalRoadCheckpoint alcanzado."
        );
    }
}