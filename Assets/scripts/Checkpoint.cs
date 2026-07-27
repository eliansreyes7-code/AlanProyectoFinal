using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private GameObject visualMarker;

    [Header("Floating Animation")]
    [SerializeField] private float floatHeight = 0.35f;
    [SerializeField] private float floatSpeed = 2f;

    private RouteManager routeManager;
    private int checkpointIndex;

    private Vector3 markerStartPosition;
    private bool isActiveCheckpoint;

    public Transform TargetTransform => transform;

    private void Awake()
    {
        if (visualMarker != null)
        {
            markerStartPosition = visualMarker.transform.localPosition;
        }
    }

    public void Initialize(RouteManager manager, int index)
    {
        routeManager = manager;
        checkpointIndex = index;

        SetCheckpointActive(false);
    }

    public void SetCheckpointActive(bool state)
    {
        isActiveCheckpoint = state;

        if (visualMarker != null)
        {
            visualMarker.SetActive(state);

            // Regresa el marcador a su posición inicial
            if (state)
                visualMarker.transform.localPosition = markerStartPosition;
        }
    }

    private void Update()
    {
        if (!isActiveCheckpoint || visualMarker == null)
            return;

        AnimateMarker();
    }

    private void AnimateMarker()
    {
        float offset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        Vector3 position = markerStartPosition;
        position.y += offset;

        visualMarker.transform.localPosition = position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveCheckpoint)
            return;

        WheelchairController player =
            other.GetComponentInParent<WheelchairController>();

        if (player == null)
            player = other.GetComponent<WheelchairController>();

        if (player == null)
            return;

        if (routeManager == null)
            return;

        routeManager.ReachCheckpoint(checkpointIndex);
    }
}