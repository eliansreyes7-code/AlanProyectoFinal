using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalRoadHit : MonoBehaviour
{
    [SerializeField] private string officeSceneName =
        "office";

    private bool hit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hit)
            return;

        WheelchairController player =
            other.GetComponentInParent
                <WheelchairController>();

        if (player == null)
            return;

        hit = true;

        SceneManager.LoadScene(
            officeSceneName
        );
    }
}