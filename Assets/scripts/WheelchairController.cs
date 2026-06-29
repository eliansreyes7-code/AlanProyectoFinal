using UnityEngine;

public class WheelchairController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxForwardSpeed = 2.0f;
    [SerializeField] private float maxBackwardSpeed = 1.0f;
    [SerializeField] private float acceleration = 3.5f;
    [SerializeField] private float deceleration = 5.5f;

    [Header("Rotation")]
    [SerializeField] private float maxTurnSpeed = 85f;
    [SerializeField] private float turnAcceleration = 6f;
    [SerializeField] private float idleTurnMultiplier = 0.45f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 1.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;

    private float moveInput;
    private float turnInput;

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
        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");
    }

    private void FixedUpdate()
    {
        CheckGround();
        HandleMovement();
        HandleRotation();
    }

    private void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void HandleMovement()
    {
        if (!isGrounded)
            return;

        float targetSpeed = 0f;

        if (moveInput > 0)
            targetSpeed = maxForwardSpeed;
        else if (moveInput < 0)
            targetSpeed = -maxBackwardSpeed;

        float changeRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            changeRate * Time.fixedDeltaTime
        );

        Vector3 movement = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }

    private void HandleRotation()
    {
        if (!isGrounded)
            return;

        float movementFactor = Mathf.Abs(currentSpeed) > 0.15f ? 1f : idleTurnMultiplier;
        float targetTurnSpeed = turnInput * maxTurnSpeed * movementFactor;

        currentTurnSpeed = Mathf.MoveTowards(
            currentTurnSpeed,
            targetTurnSpeed,
            turnAcceleration * maxTurnSpeed * Time.fixedDeltaTime
        );

        Quaternion turnRotation = Quaternion.Euler(
            0f,
            currentTurnSpeed * Time.fixedDeltaTime,
            0f
        );

        rb.MoveRotation(rb.rotation * turnRotation);
    }
}