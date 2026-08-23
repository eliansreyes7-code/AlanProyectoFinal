using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // =====================================================
    // ESCENAS
    // =====================================================

    [Header("ESCENAS")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string firstGameScene;

    // =====================================================
    // PANELES
    // =====================================================

    [Header("PANELES")]
    [SerializeField] private CanvasGroup mainMenuPanel;
    [SerializeField] private CanvasGroup pausePanel;
    [SerializeField] private CanvasGroup endPanel;

    [Tooltip("Panel exclusivo para mostrar el mensaje de reflexión final.")]
    [SerializeField] private CanvasGroup reflectionPanel;

    // =====================================================
    // FADE
    // =====================================================

    [Header("FADE")]
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private float fadeDuration = 0.6f;

    // =====================================================
    // VARIABLES
    // =====================================================

    private bool isPaused = false;
    private bool isTransitioning = false;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        /*
         * IMPORTANTE:
         * UIManager YA NO es persistente.
         *
         * Cada escena utiliza su propio UI_Global / UIManager.
         * Así, al volver a MainMenu, usamos el EndPanel REAL
         * de esa escena y no una copia antigua de otra escena.
         */
        Instance = this;
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;
        isTransitioning = false;

        Scene currentScene =
            SceneManager.GetActiveScene();

        // =================================================
        // SI EL JUEGO EMPIEZA DIRECTAMENTE EN MAIN MENU
        // =================================================

        if (currentScene.name == mainMenuScene)
        {
            // =============================================
            // ENTRADA NORMAL AL MAIN MENU
            // =============================================

            if (!FinalGameState.showReflectionPanel)
            {
                HideImmediately(pausePanel);
                HideImmediately(endPanel);
                HideImmediately(reflectionPanel);

                ShowImmediately(mainMenuPanel);

                UnlockCursor();

                Debug.Log(
                    "UIManager -> Inicio normal: MainMenuPanel."
                );
            }

            // =============================================
            // VENIMOS DESDE OFFICE2
            // =============================================

            else
            {
                HideImmediately(mainMenuPanel);
                HideImmediately(pausePanel);
                HideImmediately(endPanel);

                ForceShowPanel(
                    reflectionPanel,
                    "ReflectionPanel"
                );

                UnlockCursor();

                Debug.Log(
                    "UIManager -> Final: ReflectionPanel."
                );

                // Consumir el estado una vez mostrado.
                FinalGameState.showReflectionPanel = false;
            }
        }

        // =================================================
        // CUALQUIER ESCENA DE GAMEPLAY
        // =================================================

        else
        {
            HideImmediately(mainMenuPanel);
            HideImmediately(pausePanel);
            HideImmediately(endPanel);
            HideImmediately(reflectionPanel);

            LockCursor();
        }

        // =================================================
        // FADE INICIAL
        // =================================================

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);

            fadeOverlay.alpha = 1f;
            fadeOverlay.interactable = false;
            fadeOverlay.blocksRaycasts = true;

            StartCoroutine(
                Fade(
                    fadeOverlay,
                    0f
                )
            );
        }
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (isTransitioning)
            return;

        if (SceneManager.GetActiveScene().name == mainMenuScene)
            return;

        if (endPanel != null && endPanel.gameObject.activeSelf)
            return;

#if ENABLE_INPUT_SYSTEM

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

#elif ENABLE_LEGACY_INPUT_MANAGER

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

#endif
    }

    // =====================================================
    // CONFIGURAR ESCENA DESPUÉS DE UN FRAME
    // =====================================================

    private IEnumerator ConfigureLoadedScene(
        Scene scene)
    {
        yield return null;

        HideImmediately(pausePanel);

        // =================================================
        // MAIN MENU
        // =================================================

        if (scene.name == mainMenuScene)
        {
            UnlockCursor();

            bool showReflection =
                FinalGameState.showReflectionPanel;

            Debug.Log(
                "UIManager -> MainMenu cargado | Mostrar reflexión: " +
                showReflection
            );

            // =============================================
            // VENIMOS DEL FINAL DEL JUEGO
            // =============================================

            if (showReflection)
            {
                // Ocultar todo lo demás.
                HideImmediately(mainMenuPanel);
                HideImmediately(pausePanel);
                HideImmediately(endPanel);

                // Mostrar SOLO el panel de reflexión.
                ForceShowPanel(
                    reflectionPanel,
                    "ReflectionPanel"
                );

                // Evitar que el overlay tape el mensaje.
                if (fadeOverlay != null)
                {
                    fadeOverlay.gameObject.SetActive(true);
                    fadeOverlay.alpha = 0f;
                    fadeOverlay.interactable = false;
                    fadeOverlay.blocksRaycasts = false;
                }

                Debug.Log(
                    "UIManager -> REFLECTION PANEL ACTIVADO."
                );

                // Consumimos el estado después de mostrarlo.
                FinalGameState.showReflectionPanel = false;

                yield break;
            }

            // =============================================
            // ENTRADA NORMAL AL MENÚ
            // =============================================

            HideImmediately(pausePanel);
            HideImmediately(endPanel);
            HideImmediately(reflectionPanel);

            ShowImmediately(mainMenuPanel);

            if (fadeOverlay != null)
            {
                fadeOverlay.gameObject.SetActive(true);
                fadeOverlay.alpha = 0f;
                fadeOverlay.interactable = false;
                fadeOverlay.blocksRaycasts = false;
            }

            Debug.Log(
                "UIManager -> MainMenuPanel activado."
            );

            yield break;
        }

        // =================================================
        // GAMEPLAY
        // =================================================

        HideImmediately(mainMenuPanel);
        HideImmediately(endPanel);
        HideImmediately(reflectionPanel);

        LockCursor();
    }

    // =====================================================
    // INICIAR JUEGO
    // =====================================================

    public void StartGame()
    {
        if (string.IsNullOrEmpty(firstGameScene))
        {
            Debug.LogError(
                "No has colocado la primera escena del juego en UIManager."
            );

            return;
        }

        FinalGameState.showReflectionPanel = false;

        LoadScene(firstGameScene);
    }

    // =====================================================
    // SALIR DEL JUEGO
    // =====================================================

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // =====================================================
    // PAUSA
    // =====================================================

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;
        Time.timeScale = 0f;

        UnlockCursor();

        StartCoroutine(
            ShowPanel(pausePanel)
        );
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        StartCoroutine(
            ResumeRoutine()
        );
    }

    private IEnumerator ResumeRoutine()
    {
        yield return HidePanel(pausePanel);

        Time.timeScale = 1f;
        isPaused = false;

        LockCursor();
    }

    // =====================================================
    // REINICIAR ESCENA
    // =====================================================

    public void RestartScene()
    {
        Time.timeScale = 1f;
        isPaused = false;

        string currentScene =
            SceneManager.GetActiveScene().name;

        LoadScene(currentScene);
    }

    // =====================================================
    // VOLVER AL MAIN MENU NORMAL
    // =====================================================

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        // Si el jugador usa un botón normal para volver,
        // queremos el menú principal, no el mensaje final.
        FinalGameState.showReflectionPanel = false;

        LoadScene(mainMenuScene);
    }

    // =====================================================
    // MOSTRAR END PANEL EN LA ESCENA ACTUAL
    // =====================================================

    public void ShowEndMenu()
    {
        if (endPanel == null)
            return;

        Time.timeScale = 0f;
        isPaused = false;

        UnlockCursor();

        HideImmediately(mainMenuPanel);
        HideImmediately(reflectionPanel);

        StartCoroutine(
            ShowPanel(endPanel)
        );
    }

    // =====================================================
    // CARGAR ESCENA
    // =====================================================

    public void LoadScene(
        string sceneName)
    {
        if (isTransitioning)
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError(
                "Intentaste cargar una escena sin nombre."
            );

            return;
        }

        StartCoroutine(
            LoadSceneWithFade(sceneName)
        );
    }

    // =====================================================
    // CARGAR ESCENA CON FADE
    // =====================================================

    private IEnumerator LoadSceneWithFade(
        string sceneName)
    {
        if (isTransitioning)
            yield break;

        isTransitioning = true;
        Time.timeScale = 1f;

        // Fade a negro antes de abandonar la escena actual.
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);

            yield return Fade(
                fadeOverlay,
                1f
            );
        }

        /*
         * Al cargar la nueva escena, este UIManager se destruye
         * normalmente. El UIManager de la nueva escena hará su
         * propio fade inicial desde negro.
         */
        SceneManager.LoadScene(sceneName);
    }

    // =====================================================
    // MOSTRAR PANEL CON FADE
    // =====================================================

    private IEnumerator ShowPanel(
        CanvasGroup panel)
    {
        if (panel == null)
            yield break;

        EnsureParentsActive(panel.transform);

        panel.gameObject.SetActive(true);
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        yield return Fade(
            panel,
            1f
        );

        panel.interactable = true;
        panel.blocksRaycasts = true;
    }

    // =====================================================
    // OCULTAR PANEL CON FADE
    // =====================================================

    private IEnumerator HidePanel(
        CanvasGroup panel)
    {
        if (panel == null)
            yield break;

        panel.interactable = false;
        panel.blocksRaycasts = false;

        yield return Fade(
            panel,
            0f
        );

        panel.gameObject.SetActive(false);
    }

    // =====================================================
    // FADE
    // =====================================================

    private IEnumerator Fade(
        CanvasGroup group,
        float targetAlpha)
    {
        if (group == null)
            yield break;

        float startingAlpha = group.alpha;
        float elapsedTime = 0f;

        if (group == fadeOverlay)
        {
            group.blocksRaycasts = true;
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float percentage =
                Mathf.Clamp01(
                    elapsedTime / fadeDuration
                );

            group.alpha =
                Mathf.Lerp(
                    startingAlpha,
                    targetAlpha,
                    percentage
                );

            yield return null;
        }

        group.alpha = targetAlpha;

        if (group == fadeOverlay &&
            targetAlpha == 0f)
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }

    // =====================================================
    // OCULTAR INMEDIATAMENTE
    // =====================================================

    private void HideImmediately(
        CanvasGroup group)
    {
        if (group == null)
            return;

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        group.gameObject.SetActive(false);
    }

    // =====================================================
    // MOSTRAR INMEDIATAMENTE
    // =====================================================

    private void ShowImmediately(
        CanvasGroup group)
    {
        if (group == null)
            return;

        EnsureParentsActive(group.transform);

        group.gameObject.SetActive(true);
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    // =====================================================
    // FORZAR PANEL FINAL VISIBLE
    // =====================================================

    private void ForceShowPanel(
        CanvasGroup panel,
        string panelName)
    {
        if (panel == null)
        {
            Debug.LogError(
                "UIManager: " + panelName + " no está asignado."
            );

            return;
        }

        /*
         * Si UI_Global o cualquier padre estuviera
         * desactivado, lo volvemos a activar.
         */
        EnsureParentsActive(
            panel.transform
        );

        panel.gameObject.SetActive(true);

        panel.alpha = 1f;
        panel.interactable = true;
        panel.blocksRaycasts = true;

        Debug.Log(
            "UIManager -> ForceShowPanel: " +
            panel.gameObject.name +
            " | activeInHierarchy = " +
            panel.gameObject.activeInHierarchy +
            " | alpha = " +
            panel.alpha
        );
    }

    // =====================================================
    // ASEGURAR PADRES ACTIVOS
    // =====================================================

    private void EnsureParentsActive(
        Transform child)
    {
        if (child == null)
            return;

        Transform parent =
            child.parent;

        while (parent != null)
        {
            if (!parent.gameObject.activeSelf)
            {
                parent.gameObject.SetActive(true);
            }

            parent = parent.parent;
        }
    }

    // =====================================================
    // CURSOR
    // =====================================================

    private void UnlockCursor()
    {
        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible =
            true;
    }

    private void LockCursor()
    {
        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible =
            false;
    }

    // =====================================================
    // CLEANUP
    // =====================================================

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}