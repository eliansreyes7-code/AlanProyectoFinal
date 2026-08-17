using UnityEngine;

public class FinalRoadCar : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 8f;

    private Transform destination;

    public void Setup(Transform newDestination)
    {
        destination = newDestination;
    }

    private void Update()
    {
        if (destination == null)
            return;

        Vector3 direction =
            destination.position -
            transform.position;

        direction.y = 0f;

        if (direction.magnitude <= 1f)
        {
            Destroy(gameObject);
            return;
        }

        direction.Normalize();

        transform.position +=
            direction *
            speed *
            Time.deltaTime;
    }
}