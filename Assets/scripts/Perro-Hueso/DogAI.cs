using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DogAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float stoppingDistance = 1.5f;
    [SerializeField] private float rotationSpeed = 8f;

    private Rigidbody rb;
    private Transform player;
    private bool isFollowing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ |
            RigidbodyConstraints.FreezePositionY;
    }

    private void FixedUpdate()
    {
        if (!isFollowing || player == null)
            return;

        FollowPlayer();
    }

    private void FollowPlayer()
    {
        Vector3 targetPosition = new Vector3(
            player.position.x,
            rb.position.y,
            player.position.z
        );

        Vector3 direction = targetPosition - rb.position;
        float distance = direction.magnitude;

        if (distance <= stoppingDistance)
            return;

        direction.Normalize();

        Vector3 nextPosition =
            rb.position +
            direction * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction, Vector3.up);

            Quaternion smoothRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

            rb.MoveRotation(smoothRotation);
        }
    }

    public void ActivateDog(Transform target)
    {
        if (target == null)
        {
            Debug.LogError("DogAI recibió un objetivo vacío.");
            return;
        }

        player = target;
        isFollowing = true;

        Debug.Log("Perro activado y persiguiendo a: " + player.name);
    }
}