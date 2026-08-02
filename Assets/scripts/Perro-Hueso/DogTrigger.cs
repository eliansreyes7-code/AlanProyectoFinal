using UnityEngine;

public class DogTrigger : MonoBehaviour
{
    [SerializeField] private DogAI dog;

    private bool activated;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entró: " + other.name);
        Debug.Log("Tag detectado: " + other.tag);

        if (activated)
            return;

        if (!other.CompareTag("Player"))
            return;

        Debug.Log("¡Player detectado!");

        activated = true;
        dog.ActivateDog(other.transform);
    }
}