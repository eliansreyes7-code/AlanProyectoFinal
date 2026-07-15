using UnityEngine;

public class WheelchairCollision : MonoBehaviour
{
    [SerializeField] private float impactThreshold = 2f;

    private WheelchairController controller;

    private void Start()
    {
        controller = GetComponent<WheelchairController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Obstacle"))
            return;

        float impact = collision.relativeVelocity.magnitude;

        if (impact >= impactThreshold)
        {
            Debug.Log("Golpe fuerte");

            controller.ReduceSpeed(0.5f);
        }
    }
}