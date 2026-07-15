using UnityEngine;

public class WheelchairController : MonoBehaviour
{
    [Header("Wheel Push")]
    [SerializeField] private float pushForce = 0.45f;
    [SerializeField] private float maxForwardSpeed = 2.0f;
    [SerializeField] private float maxBackwardSpeed = 1.0f;
    [SerializeField] private float naturalDeceleration = 1.8f;

    [Header("Brake")]
    [SerializeField] private float brakeDeceleration = 5.5f;
    [SerializeField] private KeyCode brakeKey = KeyCode.Space;

    [Header("Rotation")]
    [SerializeField] private float turnForce = 55f;
    [SerializeField] private float maxTurnSpeed = 90f;
    [SerializeField] private float turnDeceleration = 3.5f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 1.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;

    private float currentSpeed;
    private float currentTurnSpeed;

    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("WheelchairController necesita un Rigidbody en el mismo objeto.");
            enabled = false;
            return;
        }

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        CheckGround();

        bool reverseMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.Q))
            PushLeftWheel(reverseMode);

        if (Input.GetKeyDown(KeyCode.E))
            PushRightWheel(reverseMode);
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyRotation();
        ApplyDeceleration();
    }

    private void CheckGround()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );
    }

    private void PushLeftWheel(bool reverse)
    {
        if (!isGrounded)
            return;

        float direction = reverse ? -1f : 1f;

        currentSpeed += pushForce * direction;
        currentTurnSpeed += turnForce * direction;
    }

    private void PushRightWheel(bool reverse)
    {
        if (!isGrounded)
            return;

        float direction = reverse ? -1f : 1f;

        currentSpeed += pushForce * direction;
        currentTurnSpeed -= turnForce * direction;
    }

    private void ApplyMovement()
    {
        currentSpeed = Mathf.Clamp(currentSpeed, -maxBackwardSpeed, maxForwardSpeed);

        Vector3 movement = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }

    private void ApplyRotation()
    {
        currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -maxTurnSpeed, maxTurnSpeed);

        Quaternion rotation = Quaternion.Euler(
            0f,
            currentTurnSpeed * Time.fixedDeltaTime,
            0f
        );

        rb.MoveRotation(rb.rotation * rotation);
    }

    private void ApplyDeceleration()
    {
        bool braking = Input.GetKey(brakeKey);

        float speedDeceleration = braking ? brakeDeceleration : naturalDeceleration;

        float rotationDeceleration = braking
            ? brakeDeceleration * maxTurnSpeed
            : turnDeceleration * maxTurnSpeed;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            0f,
            speedDeceleration * Time.fixedDeltaTime
        );

        currentTurnSpeed = Mathf.MoveTowards(
            currentTurnSpeed,
            0f,
            rotationDeceleration * Time.fixedDeltaTime
        );
    }

    public void ReduceSpeed(float multiplier)
    {
        currentSpeed *= multiplier;
        currentTurnSpeed *= multiplier;
    }
}