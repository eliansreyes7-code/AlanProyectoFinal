using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DoorSceneTransition : MonoBehaviour
{
    // =====================================================
    // ESCENA
    // =====================================================

    [Header("Scene")]
    [Tooltip("Nombre exacto de la escena del menú principal.")]
    [SerializeField] private string sceneToLoad = "MainMenu";

    // =====================================================
    // FLASH
    // =====================================================

    [Header("Flash")]
    [Tooltip("Imagen blanca que cubre toda la pantalla.")]
    [SerializeField] private Image flashImage;

    [Tooltip("Tiempo que tarda la pantalla en ponerse blanca.")]
    [SerializeField] private float flashDuration = 0.65f;

    [Tooltip("Tiempo que permanece completamente blanca antes de cargar MainMenu.")]
    [SerializeField] private float whiteHoldDuration = 0.12f;

    // =====================================================
    // VARIABLES
    // =====================================================

    private bool isTransitioning = false;

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        // Preparar el flash completamente transparente.
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(true);

            Color color = flashImage.color;
            color.a = 0f;
            flashImage.color = color;

            flashImage.raycastTarget = false;
        }
    }

    // =====================================================
    // TRIGGER
    // =====================================================

    private void OnTriggerEnter(Collider other)
    {
        if (isTransitioning)
            return;

        WheelchairController player =
            other.GetComponentInParent<WheelchairController>();

        if (player == null)
        {
            player =
                other.GetComponent<WheelchairController>();
        }

        if (player == null)
            return;

        StartCoroutine(
            FinalTransition(player)
        );
    }

    // =====================================================
    // TRANSICIÓN FINAL
    // =====================================================

    private IEnumerator FinalTransition(
        WheelchairController player)
    {
        isTransitioning = true;

        // =================================================
        // DETENER JUGADOR
        // =================================================

        player.StopMovement();
        player.enabled = false;

        Rigidbody playerRb =
            player.GetComponent<Rigidbody>();

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        // =================================================
        // MUY IMPORTANTE:
        // AVISAR AL MAIN MENU QUE DEBE MOSTRAR REFLECTION
        // =================================================

        FinalGameState.showReflectionPanel = true;

        Debug.Log(
            "DoorSceneTransition -> ReflectionPanel solicitado."
        );

        // =================================================
        // FLASH BLANCO
        // =================================================

        if (flashImage != null)
        {
            float timer = 0f;

            Color color =
                flashImage.color;

            color.a = 0f;
            flashImage.color = color;

            while (timer < flashDuration)
            {
                timer += Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        timer / flashDuration
                    );

                // Entrada suave al blanco.
                t =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    );

                color.a = t;
                flashImage.color = color;

                yield return null;
            }

            color.a = 1f;
            flashImage.color = color;
        }

        // =================================================
        // MANTENER BLANCO UN INSTANTE
        // =================================================

        if (whiteHoldDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(
                whiteHoldDuration
            );
        }

        // =================================================
        // CARGAR MAIN MENU
        // =================================================

        SceneManager.LoadScene(
            sceneToLoad
        );
    }
}