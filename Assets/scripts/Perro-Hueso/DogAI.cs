using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DogAI : MonoBehaviour
{
    private enum DogState
    {
        Idle,
        ChasingPlayer,
        GoingToBone
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float stoppingDistance = 1.2f;
    [SerializeField] private float catchDistance = 1.5f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Challenge")]
    [SerializeField] private DogChallengeManager challengeManager;

    private Rigidbody rb;

    private Transform currentTarget;
    private Transform player;

    private DogState currentState =
        DogState.Idle;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private bool playerCaught;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        rb.useGravity = false;
        rb.isKinematic = false;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ |
            RigidbodyConstraints.FreezePositionY;
    }

    private void FixedUpdate()
    {
        if (currentState == DogState.Idle)
            return;

        if (currentTarget == null)
            return;

        if (currentState == DogState.ChasingPlayer)
        {
            CheckPlayerCatch();

            if (playerCaught)
                return;
        }

        FollowTarget();
    }

    // =====================================================
    // MOVIMIENTO
    // =====================================================

    private void FollowTarget()
    {
        Vector3 targetPosition =
            new Vector3(
                currentTarget.position.x,
                rb.position.y,
                currentTarget.position.z
            );

        Vector3 direction =
            targetPosition - rb.position;

        float distance =
            direction.magnitude;

        if (distance <= stoppingDistance)
            return;

        direction.Normalize();

        Vector3 nextPosition =
            rb.position +
            direction *
            moveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction,
                Vector3.up
            );

        Quaternion smoothRotation =
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed *
                Time.fixedDeltaTime
            );

        rb.MoveRotation(smoothRotation);
    }

    // =====================================================
    // ATRAPAR PLAYER
    // =====================================================

    private void CheckPlayerCatch()
    {
        if (player == null || playerCaught)
            return;

        Vector3 dogPosition = transform.position;
        Vector3 playerPosition = player.position;

        dogPosition.y = 0f;
        playerPosition.y = 0f;

        float distance =
            Vector3.Distance(
                dogPosition,
                playerPosition
            );

        if (distance <= catchDistance)
        {
            playerCaught = true;

            currentState = DogState.Idle;
            currentTarget = null;

            if (challengeManager != null)
            {
                challengeManager.PlayerCaught();
            }
        }
    }

    // =====================================================
    // ESTADOS
    // =====================================================

    public void ChasePlayer(Transform target)
    {
        if (target == null)
            return;

        player = target;
        currentTarget = target;

        playerCaught = false;

        currentState =
            DogState.ChasingPlayer;
    }

    public void GoToBone(Transform bone)
    {
        if (bone == null)
            return;

        currentTarget = bone;

        currentState =
            DogState.GoingToBone;
    }

    public void StopDog()
    {
        currentTarget = null;

        currentState =
            DogState.Idle;
    }

    // =====================================================
    // REINICIAR
    // =====================================================

    public void ResetDog()
    {
        playerCaught = false;

        currentTarget = null;
        player = null;

        currentState =
            DogState.Idle;

        rb.position = initialPosition;
        rb.rotation = initialRotation;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}