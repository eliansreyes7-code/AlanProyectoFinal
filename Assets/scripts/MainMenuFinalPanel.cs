using System.Collections;
using UnityEngine;

public class MainMenuFinalPanel : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject reflectionPanel;

    [Header("Overlay")]
    [Tooltip("FadeOverlay del MainMenu. Se apagará para que no tape el mensaje.")]
    [SerializeField] private GameObject fadeOverlay;

    private IEnumerator Start()
    {
        // Esperar a que todos los scripts del menú inicialicen.
        yield return null;

        if (FinalGameState.showReflectionPanel)
        {
            ShowReflectionPanel();
        }
        else
        {
            ShowNormalMenu();
        }
    }

    private void ShowReflectionPanel()
    {
        // Quitar cualquier overlay que pueda tapar el mensaje.
        if (fadeOverlay != null)
        {
            fadeOverlay.SetActive(false);
        }

        // Ocultar menú normal.
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        // Mostrar mensaje final.
        if (reflectionPanel != null)
        {
            reflectionPanel.SetActive(true);
        }

        Debug.Log("FINAL DEL JUEGO -> EndPanel ACTIVADO.");

        // Consumir el estado.
        FinalGameState.showReflectionPanel = false;
    }

    private void ShowNormalMenu()
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (reflectionPanel != null)
        {
            reflectionPanel.SetActive(false);
        }
    }
}