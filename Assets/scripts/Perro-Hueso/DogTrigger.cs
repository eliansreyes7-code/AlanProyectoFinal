using UnityEngine;

public class DogTrigger : MonoBehaviour
{
    [SerializeField]
    private DogChallengeManager challengeManager;

    private bool activated = false;

    private void OnTriggerEnter(
        Collider other)
    {
        if (activated)
            return;

        WheelchairController player =
            other.GetComponentInParent
            <WheelchairController>();

        if (player == null)
            return;

        activated = true;

        Debug.Log(
            "DOG TRIGGER ACTIVADO."
        );

        challengeManager.StartDogChallenge(
            player.transform
        );
    }

    public void ResetTrigger()
    {
        activated = false;
    }
}