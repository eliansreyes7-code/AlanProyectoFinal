using UnityEngine;

public class DogTrigger : MonoBehaviour
{
    [SerializeField]
    private DogChallengeManager challengeManager;

    private bool activated;

    private void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;

        WheelchairController player =
            other.GetComponentInParent
            <WheelchairController>();

        if (player == null)
            return;

        activated = true;

        challengeManager.StartDogChallenge(
            player.transform
        );
    }

    public void ResetTrigger()
    {
        activated = false;
    }
}