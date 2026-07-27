using UnityEngine;

public class RouteManager : MonoBehaviour
{
    [Header("Route")]
    [SerializeField] private Checkpoint[] checkpoints;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Route Line")]
    [SerializeField] private LineRenderer routeLine;

    // Altura fija de la guía sobre el suelo.
    // Si el suelo está en Y = 0, 0.03 la deja apenas por encima.
    [SerializeField] private float routeGroundHeight = 0.03f;

    [Header("Route Pulse")]
    [SerializeField] private float pulseSpeed = 2f;

    // Transparencia mínima.
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.08f;

    // Transparencia máxima.
    [Range(0f, 1f)]
    [SerializeField] private float maxAlpha = 0.30f;

    private int currentCheckpointIndex = 0;

    private Material routeMaterial;

    public Checkpoint CurrentCheckpoint
    {
        get
        {
            if (checkpoints == null || checkpoints.Length == 0)
                return null;

            if (currentCheckpointIndex < 0 ||
                currentCheckpointIndex >= checkpoints.Length)
                return null;

            return checkpoints[currentCheckpointIndex];
        }
    }

    private void Start()
    {
        // -----------------------------
        // Verificar checkpoints
        // -----------------------------

        if (checkpoints == null || checkpoints.Length == 0)
        {
            Debug.LogWarning(
                "RouteManager no tiene checkpoints asignados."
            );

            return;
        }

        // -----------------------------
        // Buscar jugador automáticamente
        // -----------------------------

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
                    "RouteManager no encontró al jugador."
                );
            }
        }

        // -----------------------------
        // Preparar Line Renderer
        // -----------------------------

        if (routeLine != null)
        {
            /*
             * .material crea una instancia del material.
             * Así podemos modificar su transparencia sin
             * cambiar el material original del proyecto.
             */
            routeMaterial = routeLine.material;

            routeLine.enabled = true;
        }
        else
        {
            Debug.LogWarning(
                "RouteManager: Route Line no está asignado."
            );
        }

        // -----------------------------
        // Inicializar checkpoints
        // -----------------------------

        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] != null)
            {
                checkpoints[i].Initialize(this, i);
            }
        }

        currentCheckpointIndex = 0;

        ActivateCurrentCheckpoint();
    }

    private void Update()
    {
        UpdateRouteLine();
        UpdateRoutePulse();
    }

    // =====================================================
    // CHECKPOINTS
    // =====================================================

    private void ActivateCurrentCheckpoint()
    {
        if (CurrentCheckpoint == null)
            return;

        CurrentCheckpoint.SetCheckpointActive(true);
    }

    public void ReachCheckpoint(int index)
    {
        // Solo aceptar el checkpoint que corresponde.
        if (index != currentCheckpointIndex)
            return;

        if (CurrentCheckpoint == null)
            return;

        // Ocultar checkpoint completado.
        CurrentCheckpoint.SetCheckpointActive(false);

        // Pasar al siguiente.
        currentCheckpointIndex++;

        // ¿Terminamos la ruta?
        if (currentCheckpointIndex >= checkpoints.Length)
        {
            CompleteRoute();
            return;
        }

        // Activar próximo checkpoint.
        ActivateCurrentCheckpoint();
    }

    // =====================================================
    // GUÍA VISUAL
    // =====================================================

    private void UpdateRouteLine()
    {
        if (routeLine == null)
            return;

        if (player == null || CurrentCheckpoint == null)
        {
            routeLine.enabled = false;
            return;
        }

        routeLine.enabled = true;

        /*
         * Por ahora utilizamos dos puntos:
         *
         * Player -------- Checkpoint
         *
         * Más adelante podemos agregar RoutePoints
         * para trazar curvas y seguir las calles.
         */
        routeLine.positionCount = 2;

        Vector3 startPosition = player.position;

        Vector3 endPosition =
            CurrentCheckpoint.TargetTransform.position;

        /*
         * IMPORTANTE:
         *
         * No utilizamos la altura del centro del Player.
         * Ambos extremos se colocan prácticamente
         * sobre el suelo.
         */
        startPosition.y = routeGroundHeight;
        endPosition.y = routeGroundHeight;

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
    // EFECTO PULSANTE
    // =====================================================

    private void UpdateRoutePulse()
    {
        if (routeMaterial == null)
            return;

        /*
         * Mathf.Sin genera un movimiento entre -1 y 1.
         *
         * Lo transformamos a un rango entre 0 y 1.
         */
        float pulse =
            (Mathf.Sin(Time.time * pulseSpeed) + 1f)
            * 0.5f;

        /*
         * Luego convertimos ese valor al rango
         * de transparencia elegido.
         */
        float alpha =
            Mathf.Lerp(
                minAlpha,
                maxAlpha,
                pulse
            );

        Color color = routeMaterial.color;

        color.a = alpha;

        routeMaterial.color = color;
    }

    // =====================================================
    // FINAL DE RUTA
    // =====================================================

    private void CompleteRoute()
    {
        if (routeLine != null)
        {
            routeLine.enabled = false;
        }

        Debug.Log("Ruta completada.");
    }
}