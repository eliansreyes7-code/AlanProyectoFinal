using UnityEngine;

public class WheelchairAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float animationHoldTime = 0.4f;

    private float moveTimer;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) ||
            Input.GetKeyDown(KeyCode.E))
        {
            moveTimer = animationHoldTime;
        }

        if (moveTimer > 0f)
        {
            moveTimer -= Time.deltaTime;
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }
    }
}