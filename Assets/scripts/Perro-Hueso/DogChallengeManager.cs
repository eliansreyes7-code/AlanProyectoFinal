using System.Collections;
using TMPro;
using UnityEngine;

public class DogChallengeManager : MonoBehaviour
{
    // =====================================================
    // PERRO
    // =====================================================

    [Header("Dog")]
    [SerializeField] private DogAI dog;
    [SerializeField] private Transform dogLookTarget;

    [Header("Dog Final Target")]
    [Tooltip("Lugar hacia donde correrá el perro después de tomar/lanzar el hueso.")]
    [SerializeField] private Transform boneThrowTarget;

    // =====================================================
    // HUESO
    // =====================================================

    [Header("Bone")]
    [SerializeField] private Bone bone;

    // =====================================================
    // TRIGGER
    // =====================================================

    [Header("Trigger")]
    [SerializeField] private DogTrigger dogTrigger;

    // =====================================================
    // RUTA FINAL
    // =====================================================

    [Header("Final Route")]

    [Tooltip("RouteManager que controla Checkpoint 1, Checkpoint 2 y FinalRoadCheckpoint.")]
    [SerializeField] private RouteManager routeManager;

    [Tooltip("Transición que lleva al jugador desde FinalRoadCheckpoint hasta la carretera.")]
    [SerializeField] private FinalDogTransition finalDogTransition;

    // =====================================================
    // RESTART
    // =====================================================

    [Header("Restart")]
    [SerializeField] private Transform restartPoint;

    [Tooltip("Tiempo antes de comenzar el reinicio cuando el perro atrapa al jugador.")]
    [SerializeField] private float restartDelay = 0.4f;

    // =====================================================
    // ATRAPAR PLAYER
    // =====================================================

    [Header("Catch")]
    [SerializeField] private float catchDistance = 1.5f;

    // =====================================================
    // CÁMARA
    // =====================================================

    [Header("Camera")]
    [SerializeField] private WheelchairCameraLook cameraLook;

    // =====================================================
    // UI
    // =====================================================

    [Header("UI")]
    [SerializeField] private TMP_Text challengeText;

    [Header("Messages")]

    [TextArea]
    [SerializeField] private string dangerMessage =
        "¡Cuidado!";

    [TextArea]
    [SerializeField] private string escapeMessage =
        "¡Escapa! Llega lo más rápido posible al siguiente punto.";

    [TextArea]
    [SerializeField] private string distractDogMessage =
        "Trata de distraer al perro para poder escapar.";

    [TextArea]
    [SerializeField] private string boneMessage =
        "Presiona F para tomar el hueso.";

    [TextArea]
    [SerializeField] private string distractedMessage =
        "Distrajiste al perro.";

    [TextArea]
    [SerializeField] private string finalPointMessage =
        "Ve al siguiente punto.";

    [TextArea]
    [SerializeField] private string caughtMessage =
        "El perro te alcanzó.";

    // =====================================================
    // FLASH
    // =====================================================

    [Header("White Flash")]
    [SerializeField] private CanvasGroup whiteFlash;
    [SerializeField] private float flashInDuration = 0.25f;
    [SerializeField] private float whiteHoldDuration = 0.35f;
    [SerializeField] private float flashOutDuration = 0.6f;

    // =====================================================
    // INTRO
    // =====================================================

    [Header("Intro Animation")]
    [SerializeField] private float cameraTurnDuration = 0.7f;
    [SerializeField] private float dogLookDuration = 1.2f;

    // =====================================================
    // HUESO CINEMÁTICA
    // =====================================================

    [Header("Bone Cinematic")]
    [SerializeField] private float boneCameraTurnDuration = 0.6f;
    [SerializeField] private float dogRunWatchTime = 2.5f;

    // =====================================================
    // VARIABLES
    // =====================================================

    private Transform player;

    private WheelchairController wheelchairController;
    private Rigidbody playerRigidbody;

    private bool challengeActive = false;
    private bool dogIsChasing = false;
    private bool restarting = false;
    private bool challengeCompleted = false;

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        HideChallengeText();

        if (whiteFlash != null)
        {
            whiteFlash.alpha = 0f;
            whiteFlash.interactable = false;
            whiteFlash.blocksRaycasts = false;
        }

        if (routeManager == null)
        {
            routeManager =
                FindFirstObjectByType<RouteManager>();
        }

        if (finalDogTransition == null)
        {
            finalDogTransition =
                FindFirstObjectByType<FinalDogTransition>();
        }

        if (routeManager == null)
        {
            Debug.LogWarning(
                "DogChallengeManager: no encontró RouteManager."
            );
        }

        if (finalDogTransition == null)
        {
            Debug.LogWarning(
                "DogChallengeManager: no encontró FinalDogTransition."
            );
        }
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (!challengeActive)
            return;

        if (!dogIsChasing)
            return;

        if (restarting ||
            challengeCompleted)
        {
            return;
        }

        CheckIfDogCaughtPlayer();
    }

    // =====================================================
    // INICIAR RETO
    // =====================================================

    public void StartDogChallenge(
        Transform newPlayer)
    {
        if (challengeActive ||
            restarting ||
            challengeCompleted)
        {
            return;
        }

        if (newPlayer == null)
            return;

        player = newPlayer;

        wheelchairController =
            player.GetComponent<WheelchairController>();

        if (wheelchairController == null)
        {
            wheelchairController =
                player.GetComponentInParent
                <WheelchairController>();
        }

        playerRigidbody =
            player.GetComponent<Rigidbody>();

        if (playerRigidbody == null)
        {
            playerRigidbody =
                player.GetComponentInParent<Rigidbody>();
        }

        challengeActive = true;
        dogIsChasing = false;

        StartCoroutine(
            IntroSequence()
        );
    }

    // =====================================================
    // INTRO
    // =====================================================

    private IEnumerator IntroSequence()
    {
        if (wheelchairController != null)
        {
            wheelchairController.enabled = false;
        }

        ShowChallengeText(
            dangerMessage
        );

        if (cameraLook != null &&
            dogLookTarget != null)
        {
            yield return StartCoroutine(
                cameraLook.LookAtTargetWithZoom(
                    dogLookTarget,
                    cameraTurnDuration,
                    dogLookDuration
                )
            );
        }

        ShowChallengeText(
            escapeMessage
        );

        if (wheelchairController != null)
        {
            wheelchairController.enabled = true;
        }

        if (dog != null &&
            player != null)
        {
            dog.ChasePlayer(
                player
            );

            dogIsChasing = true;
        }
    }

    // =====================================================
    // MENSAJE CHECKPOINT 2
    // =====================================================

    public void ShowDistractDogMessage()
    {
        ShowChallengeText(
            distractDogMessage
        );
    }

    // =====================================================
    // MENSAJE PUNTO FINAL
    // =====================================================

    public void ShowGoToFinalPointMessage()
    {
        ShowChallengeText(
            finalPointMessage
        );
    }

    // =====================================================
    // HUESO CERCA
    // =====================================================

    public void ShowBonePrompt()
    {
        if (!challengeActive ||
            challengeCompleted)
        {
            return;
        }

        ShowChallengeText(
            boneMessage
        );
    }

    public void HideBonePrompt()
    {
        if (!challengeActive ||
            challengeCompleted)
        {
            return;
        }

        if (dogIsChasing)
        {
            ShowChallengeText(
                escapeMessage
            );
        }
    }

    // =====================================================
    // HUESO TOMADO
    // =====================================================

    public void BoneTaken()
    {
        if (!challengeActive ||
            challengeCompleted)
        {
            return;
        }

        StartCoroutine(
            BoneSequence()
        );
    }

    // =====================================================
    // SECUENCIA DEL HUESO
    // =====================================================

    private IEnumerator BoneSequence()
    {
        challengeCompleted = true;
        dogIsChasing = false;

        // =================================================
        // BLOQUEAR PLAYER
        // =================================================

        if (wheelchairController != null)
        {
            wheelchairController.StopMovement();
            wheelchairController.enabled = false;
        }

        ShowChallengeText(
            distractedMessage
        );

        // =================================================
        // PERRO HACIA HUESO
        // =================================================

        if (dog != null &&
            boneThrowTarget != null)
        {
            dog.GoToBone(
                boneThrowTarget
            );
        }

        // =================================================
        // CÁMARA
        // =================================================

        if (cameraLook != null &&
            dogLookTarget != null)
        {
            yield return StartCoroutine(
                cameraLook.LookAtTargetWithZoom(
                    dogLookTarget,
                    boneCameraTurnDuration,
                    dogRunWatchTime
                )
            );
        }
        else
        {
            yield return new WaitForSeconds(
                dogRunWatchTime
            );
        }

        // =================================================
        // TERMINAR RETO
        // =================================================

        challengeActive = false;

        // =================================================
        // ACTIVAR FINAL ROAD CHECKPOINT
        // =================================================

        if (routeManager != null)
        {
            routeManager.BoneWasThrown();
        }
        else
        {
            Debug.LogError(
                "DogChallengeManager: falta RouteManager."
            );
        }

        // =================================================
        // ACTIVAR TRANSICIÓN FINAL
        // =================================================

        if (finalDogTransition != null)
        {
            finalDogTransition
                .EnableFinalTransition();
        }

        // =================================================
        // MENSAJE FINAL
        // =================================================

        ShowGoToFinalPointMessage();

        // =================================================
        // DEVOLVER CONTROL
        // =================================================

        if (wheelchairController != null)
        {
            wheelchairController.enabled = true;
        }

        if (cameraLook != null)
        {
            cameraLook.SetLookEnabled(true);
        }

        Debug.Log(
            "DogChallengeManager: reto completado."
        );
    }

    // =====================================================
    // PERRO ATRAPA PLAYER
    // =====================================================

    private void CheckIfDogCaughtPlayer()
    {
        if (dog == null ||
            player == null)
        {
            return;
        }

        Vector3 dogPosition =
            dog.transform.position;

        Vector3 playerPosition =
            player.position;

        dogPosition.y = 0f;
        playerPosition.y = 0f;

        float distance =
            Vector3.Distance(
                dogPosition,
                playerPosition
            );

        if (distance <= catchDistance)
        {
            StartCoroutine(
                RestartChallengeSequence()
            );
        }
    }

    // =====================================================
    // REINICIO CUANDO EL PERRO ATRAPA AL PLAYER
    // =====================================================

    private IEnumerator RestartChallengeSequence()
    {
        if (restarting)
            yield break;

        restarting = true;

        challengeActive = false;
        dogIsChasing = false;

        // =================================================
        // DETENER PERRO
        // =================================================

        if (dog != null)
        {
            dog.StopDog();
        }

        // =================================================
        // DETENER PLAYER
        // =================================================

        if (wheelchairController != null)
        {
            wheelchairController.StopMovement();
            wheelchairController.enabled = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector3.zero;

            playerRigidbody.angularVelocity =
                Vector3.zero;
        }

        // =================================================
        // BLOQUEAR CÁMARA
        // =================================================

        if (cameraLook != null)
        {
            cameraLook.SetLookEnabled(false);
        }

        // =================================================
        // MENSAJE
        // =================================================

        ShowChallengeText(
            caughtMessage
        );

        // =================================================
        // ESPERA
        // =================================================

        yield return new WaitForSeconds(
            restartDelay
        );

        // =================================================
        // FADE BLANCO
        // =================================================

        yield return StartCoroutine(
            FadeWhite(
                0f,
                1f,
                flashInDuration
            )
        );

        // =================================================
        // TELETRANSPORTAR PLAYER
        // =================================================

        TeleportPlayerToRestartPoint();

        // =================================================
        // REINICIAR PERRO
        // =================================================

        if (dog != null)
        {
            dog.ResetDog();
        }

        // =================================================
        // REINICIAR HUESO
        // =================================================

        if (bone != null)
        {
            bone.ResetBone();
        }

        // =================================================
        // REINICIAR DOG TRIGGER
        // =================================================

        if (dogTrigger != null)
        {
            dogTrigger.ResetTrigger();
        }

        // =================================================
        // REINICIAR RUTA COMPLETA
        // =================================================

        /*
         * ESTE ES EL CAMBIO IMPORTANTE.
         *
         * Ahora una mordida hace lo mismo
         * que un atropello:
         *
         * Checkpoint 1 vuelve a ser el objetivo.
         */
        if (routeManager != null)
        {
            routeManager.ResetRouteToStart();
        }
        else
        {
            Debug.LogWarning(
                "DogChallengeManager: no hay RouteManager para reiniciar."
            );
        }

        // =================================================
        // REINICIAR ESTADOS DEL RETO
        // =================================================

        challengeCompleted = false;
        challengeActive = false;
        dogIsChasing = false;

        // =================================================
        // LIMPIAR UI
        // =================================================

        HideChallengeText();

        // =================================================
        // MANTENER BLANCO UN MOMENTO
        // =================================================

        yield return new WaitForSeconds(
            whiteHoldDuration
        );

        // =================================================
        // SALIR DEL BLANCO
        // =================================================

        yield return StartCoroutine(
            FadeWhite(
                1f,
                0f,
                flashOutDuration
            )
        );

        // =================================================
        // DEVOLVER PLAYER
        // =================================================

        if (wheelchairController != null)
        {
            wheelchairController.StopMovement();
            wheelchairController.enabled = true;
        }

        // =================================================
        // DEVOLVER CÁMARA
        // =================================================

        if (cameraLook != null)
        {
            cameraLook.SetLookEnabled(true);
        }

        restarting = false;

        Debug.Log(
            "DogChallengeManager: mordida -> " +
            "reto y ruta reiniciados completamente."
        );
    }

    // =====================================================
    // TELEPORT RESTART
    // =====================================================

    private void TeleportPlayerToRestartPoint()
    {
        if (player == null)
            return;

        if (restartPoint == null)
        {
            Debug.LogError(
                "DogChallengeManager: Restart Point no está asignado."
            );

            return;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector3.zero;

            playerRigidbody.angularVelocity =
                Vector3.zero;

            playerRigidbody.position =
                restartPoint.position;

            playerRigidbody.rotation =
                restartPoint.rotation;
        }
        else
        {
            player.SetPositionAndRotation(
                restartPoint.position,
                restartPoint.rotation
            );
        }
    }

    // =====================================================
    // FADE
    // =====================================================

    private IEnumerator FadeWhite(
        float from,
        float to,
        float duration)
    {
        if (whiteFlash == null)
            yield break;

        if (duration <= 0f)
        {
            whiteFlash.alpha = to;
            yield break;
        }

        float timer = 0f;

        whiteFlash.alpha = from;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            whiteFlash.alpha =
                Mathf.Lerp(
                    from,
                    to,
                    t
                );

            yield return null;
        }

        whiteFlash.alpha = to;
    }

    // =====================================================
    // UI
    // =====================================================

    private void ShowChallengeText(
        string message)
    {
        if (challengeText == null)
            return;

        challengeText.gameObject
            .SetActive(true);

        challengeText.text =
            message;
    }

    private void HideChallengeText()
    {
        if (challengeText == null)
            return;

        challengeText.text = "";

        challengeText.gameObject
            .SetActive(false);
    }

    // =====================================================
    // REINICIO DESDE VEHICLE PLAYER HIT
    // =====================================================

    public void ResetChallengeFromVehicleHit()
    {
        /*
         * Este método se llama desde
         * VehiclePlayerHit.
         */

        StopAllCoroutines();

        challengeActive = false;
        dogIsChasing = false;
        restarting = false;
        challengeCompleted = false;

        // =================================================
        // PERRO
        // =================================================

        if (dog != null)
        {
            dog.ResetDog();
        }

        // =================================================
        // HUESO
        // =================================================

        if (bone != null)
        {
            bone.ResetBone();
        }

        // =================================================
        // TRIGGER
        // =================================================

        if (dogTrigger != null)
        {
            dogTrigger.ResetTrigger();
        }

        // =================================================
        // UI
        // =================================================

        HideChallengeText();

        // =================================================
        // CÁMARA
        // =================================================

        if (cameraLook != null)
        {
            cameraLook.SetLookEnabled(true);
        }

        Debug.Log(
            "DogChallengeManager: reto reiniciado por atropello."
        );
    }
}