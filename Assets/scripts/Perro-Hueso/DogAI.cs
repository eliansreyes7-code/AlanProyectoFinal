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
    [SerializeField] private float stoppingDistance = 0.8f;
    [SerializeField] private float rotationSpeed = 8f;

    private Rigidbody rb;

    private Transform currentTarget;

    private DogState currentState =
        DogState.Idle;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public Transform DogTransform => transform;

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

        FollowTarget();
    }

    private void FollowTarget()
    {
        Vector3 targetPosition =
            currentTarget.position;

        targetPosition.y =
            rb.position.y;

        Vector3 direction =
            targetPosition - rb.position;

        float distance =
            direction.magnitude;

        if (distance <= stoppingDistance)
            return;

        direction.Normalize();

        Vector3 newPosition =
            rb.position +
            direction *
            moveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(newPosition);

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction,
                Vector3.up
            );

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed *
                Time.fixedDeltaTime
            )
        );
    }

    public void ChasePlayer(
        Transform player)
    {
        if (player == null)
            return;

        currentTarget = player;

        currentState =
            DogState.ChasingPlayer;
    }

    public void GoToBone(
        Transform bone)
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

    public void ResetDog()
    {
        StopDog();

        rb.linearVelocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;

        rb.position =
            initialPosition;

        rb.rotation =
            initialRotation;
    }
}