using UnityEngine;

public class Bone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DogChallengeManager challengeManager;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private KeyCode pickupKey = KeyCode.F;

    private Transform player;

    private Renderer boneRenderer;
    private Collider boneCollider;

    private bool boneTaken = false;
    private bool playerIsNear = false;

    private void Awake()
    {
        boneRenderer = GetComponent<Renderer>();
        boneCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        WheelchairController wheelchair =
            FindFirstObjectByType<WheelchairController>();

        if (wheelchair != null)
            player = wheelchair.transform;
    }

    private void Update()
    {
        if (boneTaken || player == null)
            return;

        CheckPlayerDistance();

        if (playerIsNear &&
            Input.GetKeyDown(pickupKey))
        {
            TakeBone();
        }
    }

    private void CheckPlayerDistance()
    {
        Vector3 playerPosition = player.position;
        Vector3 bonePosition = transform.position;

        playerPosition.y = 0f;
        bonePosition.y = 0f;

        float distance = Vector3.Distance(
            playerPosition,
            bonePosition
        );

        bool isNearNow =
            distance <= interactionDistance;

        // El jugador acaba de acercarse.
        if (isNearNow && !playerIsNear)
        {
            playerIsNear = true;

            if (challengeManager != null)
                challengeManager.ShowBonePrompt();
        }

        // El jugador se alejó otra vez.
        else if (!isNearNow && playerIsNear)
        {
            playerIsNear = false;

            if (challengeManager != null)
                challengeManager.HideBonePrompt();
        }
    }

    private void TakeBone()
    {
        if (boneTaken)
            return;

        boneTaken = true;
        playerIsNear = false;

        // Ocultamos el hueso.
        if (boneRenderer != null)
            boneRenderer.enabled = false;

        if (boneCollider != null)
            boneCollider.enabled = false;

        // Avisamos al manager.
        if (challengeManager != null)
            challengeManager.BoneTaken();
    }

    public void ResetBone()
    {
        boneTaken = false;
        playerIsNear = false;

        if (boneRenderer != null)
            boneRenderer.enabled = true;

        if (boneCollider != null)
            boneCollider.enabled = true;
    }
}