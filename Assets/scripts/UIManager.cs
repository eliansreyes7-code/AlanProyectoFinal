using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("ESCENAS")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string firstGameScene;

    [Header("PANELES")]
    [SerializeField] private CanvasGroup mainMenuPanel;
    [SerializeField] private CanvasGroup pausePanel;
    [SerializeField] private CanvasGroup endPanel;

    [Header("FADE")]
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private float fadeDuration = 0.6f;

    private bool isPaused = false;
    private bool isTransitioning = false;


    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        // Evitar que existan dos UIManager al cambiar de escena
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Mantener UI_Global entre escenas
        DontDestroyOnLoad(gameObject);
    }


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Ocultamos los paneles que no necesitamos al inicio
        HideImmediately(pausePanel);
        HideImmediately(endPanel);

        // Revisar si estamos en el menú principal
        if (SceneManager.GetActiveScene().name == mainMenuScene)
        {
            ShowImmediately(mainMenuPanel);

            // Mostrar mouse en menú principal
            UnlockCursor();
        }
        else
        {
            HideImmediately(mainMenuPanel);

            // Ocultar mouse durante gameplay
            LockCursor();
        }

        // Empezamos con la pantalla negra
        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.alpha = 1f;
        fadeOverlay.interactable = false;
        fadeOverlay.blocksRaycasts = true;

        // Fade inicial
        StartCoroutine(Fade(fadeOverlay, 0f));
    }


    // =====================================================
    // UPDATE / ESC
    // =====================================================

    private void Update()
    {
        // No permitir pausa mientras cambia de escena
        if (isTransitioning)
            return;

        // No permitir pausa en MainMenu
        if (SceneManager.GetActiveScene().name == mainMenuScene)
            return;

        // No permitir pausa en el final
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
    // CUANDO CARGA UNA ESCENA
    // =====================================================

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Asegurarnos de que el tiempo esté normal
        Time.timeScale = 1f;

        isPaused = false;

        HideImmediately(pausePanel);
        HideImmediately(endPanel);

        // Si cargamos MainMenu
        if (scene.name == mainMenuScene)
        {
            ShowImmediately(mainMenuPanel);

            UnlockCursor();
        }
        else
        {
            HideImmediately(mainMenuPanel);

            LockCursor();
        }
    }


    // =====================================================
    // MENÚ PRINCIPAL
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

        LoadScene(firstGameScene);
    }


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

        // Congelamos el gameplay
        Time.timeScale = 0f;

        // IMPORTANTE:
        // Liberamos el cursor para poder usar los botones
        UnlockCursor();

        // Mostrar PausePanel con fade
        StartCoroutine(ShowPanel(pausePanel));
    }


    public void ResumeGame()
    {
        if (!isPaused)
            return;

        StartCoroutine(ResumeRoutine());
    }


    private IEnumerator ResumeRoutine()
    {
        // Ocultar menú de pausa suavemente
        yield return HidePanel(pausePanel);

        // Reanudar gameplay
        Time.timeScale = 1f;

        isPaused = false;

        // Volver a bloquear el cursor
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
    // VOLVER A MAIN MENU
    // =====================================================

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        isPaused = false;

        LoadScene(mainMenuScene);
    }


    // =====================================================
    // FINAL DE TRYING
    // =====================================================

    public void ShowEndMenu()
    {
        if (endPanel == null)
            return;

        if (endPanel.gameObject.activeSelf)
            return;

        // Congelar gameplay
        Time.timeScale = 0f;

        isPaused = false;

        // Necesitamos mouse para el botón
        UnlockCursor();

        // Mostrar mensaje final
        StartCoroutine(ShowPanel(endPanel));
    }


    // =====================================================
    // CAMBIAR DE ESCENA
    // =====================================================

    public void LoadScene(string sceneName)
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


    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        if (isTransitioning)
            yield break;

        isTransitioning = true;

        // Por seguridad
        Time.timeScale = 1f;

        // Activamos FadeOverlay
        fadeOverlay.gameObject.SetActive(true);

        // Fade hacia negro
        yield return Fade(
            fadeOverlay,
            1f
        );

        // Cargar escena
        AsyncOperation loading =
            SceneManager.LoadSceneAsync(sceneName);

        while (!loading.isDone)
        {
            yield return null;
        }

        // Fade desde negro
        yield return Fade(
            fadeOverlay,
            0f
        );

        isTransitioning = false;
    }


    // =====================================================
    // MOSTRAR PANEL
    // =====================================================

    private IEnumerator ShowPanel(CanvasGroup panel)
    {
        if (panel == null)
            yield break;

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
    // OCULTAR PANEL
    // =====================================================

    private IEnumerator HidePanel(CanvasGroup panel)
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
    // SISTEMA DE FADE
    // =====================================================

    private IEnumerator Fade(
        CanvasGroup group,
        float targetAlpha
    )
    {
        if (group == null)
            yield break;

        float startingAlpha = group.alpha;

        float elapsedTime = 0f;

        // Mientras FadeOverlay esté haciendo transición,
        // bloquea los clicks
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

        // Cuando FadeOverlay vuelve a ser transparente,
        // deja de bloquear los botones
        if (group == fadeOverlay &&
            targetAlpha == 0f)
        {
            group.blocksRaycasts = false;
        }
    }


    // =====================================================
    // OCULTAR INMEDIATAMENTE
    // =====================================================

    private void HideImmediately(CanvasGroup group)
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

    private void ShowImmediately(CanvasGroup group)
    {
        if (group == null)
            return;

        group.gameObject.SetActive(true);

        group.alpha = 1f;

        group.interactable = true;
        group.blocksRaycasts = true;
    }


    // =====================================================
    // CURSOR
    // =====================================================

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    // =====================================================
    // CLEANUP
    // =====================================================

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}