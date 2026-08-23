using UnityEngine;
using UnityEngine.SceneManagement;

public class Office2Exit : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;

        WheelchairController player =
            other.GetComponentInParent<WheelchairController>();

        if (player == null)
            return;

        activated = true;

        // Avisamos que venimos del final del juego.
        FinalGameState.showReflectionPanel = true;

        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }
}