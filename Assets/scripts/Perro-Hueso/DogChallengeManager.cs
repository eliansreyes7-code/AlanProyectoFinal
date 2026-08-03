using System.Collections;
using TMPro;
using UnityEngine;

public class DogChallengeManager : MonoBehaviour
{
    [Header("Dog")]
    [SerializeField] private DogAI dog;
    [SerializeField] private Transform dogLookTarget;

    [Header("Trigger")]
    [SerializeField] private DogTrigger dogTrigger;

    [Header("Restart")]
    [SerializeField] private Transform restartPoint;
    [SerializeField] private float restartDelay = 0.4f;

    [Header("Catch")]
    [SerializeField] private float catchDistance = 1.5f;

    [Header("Camera")]
    [SerializeField] private WheelchairCameraLook cameraLook;

    [Header("UI")]
    [SerializeField] private TMP_Text challengeText;

    [Header("White Flash")]
    [SerializeField] private CanvasGroup whiteFlash;

    [SerializeField] private float flashInDuration = 0.25f;
    [SerializeField] private float whiteHoldDuration = 0.35f;
    [SerializeField] private float flashOutDuration = 0.6f;

    [Header("Intro Animation")]
    [SerializeField] private float cameraTurnDuration = 0.7f;
    [SerializeField] private float dogLookDuration = 1.2f;

    private Transform player;

    private WheelchairController wheelchairController;
    private Rigidbody playerRigidbody;

    private bool challengeActive = false;
    private bool dogIsChasing = false;
    private bool restarting = false;

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        // El mensaje del perro NO aparece al comenzar.
        HideChallengeText();

        // Destello completamente invisible.
        if (whiteFlash != null)
        {
            whiteFlash.alpha = 0f;
            whiteFlash.interactable = false;
            whiteFlash.blocksRaycasts = false;
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

        if (restarting)
            return;

        CheckIfDogCaughtPlayer();
    }

    // =====================================================
    // EMPEZAR RETO
    // =====================================================

    public void StartDogChallenge(Transform newPlayer)
    {
        if (challengeActive || restarting)
            return;

        if (newPlayer == null)
            return;

        player = newPlayer;

        wheelchairController =
            player.GetComponent<WheelchairController>();

        if (wheelchairController == null)
        {
            wheelchairController =
                player.GetComponentInParent<WheelchairController>();
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

        StartCoroutine(IntroSequence());
    }

    // =====================================================
    // INTRO
    // =====================================================

    private IEnumerator IntroSequence()
    {
        Debug.Log("DOG CHALLENGE: iniciando.");

        // Detenemos la silla durante la cinemática.
        if (wheelchairController != null)
            wheelchairController.enabled = false;

        ShowChallengeText("¡Cuidado!");

        // Mirar al perro + zoom.
        if (cameraLook != null && dogLookTarget != null)
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
            "¡Escapa! Llega lo más rápido posible al siguiente punto."
        );

        // Devolver movimiento.
        if (wheelchairController != null)
            wheelchairController.enabled = true;

        // Comenzar persecución.
        if (dog != null)
        {
            dog.ChasePlayer(player);
            dogIsChasing = true;
        }
    }

    // =====================================================
    // DETECTAR CAPTURA
    // =====================================================

    private void CheckIfDogCaughtPlayer()
    {
        if (dog == null || player == null)
            return;

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
    // PERRO ATRAPA AL JUGADOR
    // =====================================================

    private IEnumerator RestartChallengeSequence()
    {
        if (restarting)
            yield break;

        restarting = true;

        challengeActive = false;
        dogIsChasing = false;

        // Detener perro.
        if (dog != null)
            dog.StopDog();

        // Bloquear jugador.
        if (wheelchairController != null)
            wheelchairController.enabled = false;

        if (cameraLook != null)
            cameraLook.SetLookEnabled(false);

        ShowChallengeText(
            "El perro te alcanzó"
        );

        // Pequeña pausa para que se perciba.
        yield return new WaitForSeconds(
            restartDelay
        );

        // ==========================================
        // DESTELLO BLANCO
        // ==========================================

        yield return StartCoroutine(
            FadeWhite(
                0f,
                1f,
                flashInDuration
            )
        );

        /*
         * En este momento la pantalla está
         * completamente blanca.
         *
         * Aquí hacemos el teletransporte para que
         * el usuario no vea el cambio de posición.
         */

        TeleportPlayerToRestartPoint();

        // Reiniciar perro.
        if (dog != null)
            dog.ResetDog();

        // Permitir que el trigger vuelva a activarse.
        if (dogTrigger != null)
            dogTrigger.ResetTrigger();

        HideChallengeText();

        yield return new WaitForSeconds(
            whiteHoldDuration
        );

        // ==========================================
        // QUITAR DESTELLO
        // ==========================================

        yield return StartCoroutine(
            FadeWhite(
                1f,
                0f,
                flashOutDuration
            )
        );

        // Reactivar controles.
        if (wheelchairController != null)
            wheelchairController.enabled = true;

        if (cameraLook != null)
            cameraLook.SetLookEnabled(true);

        restarting = false;

        Debug.Log(
            "DOG CHALLENGE: jugador reiniciado."
        );
    }

    // =====================================================
    // TELEPORT PLAYER
    // =====================================================

    private void TeleportPlayerToRestartPoint()
    {
        if (player == null || restartPoint == null)
            return;

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
            player.position =
                restartPoint.position;

            player.rotation =
                restartPoint.rotation;
        }
    }

    // =====================================================
    // DESTELLO BLANCO
    // =====================================================

    private IEnumerator FadeWhite(
        float from,
        float to,
        float duration)
    {
        if (whiteFlash == null)
            yield break;

        float timer = 0f;

        whiteFlash.alpha = from;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            t = Mathf.SmoothStep(
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

        challengeText.gameObject.SetActive(true);
        challengeText.text = message;
    }

    private void HideChallengeText()
    {
        if (challengeText == null)
            return;

        challengeText.text = "";
        challengeText.gameObject.SetActive(false);
    }

    // =====================================================
    // HUESO
    // =====================================================

    public void BoneThrown(
        Transform boneTransform)
    {
        if (boneTransform == null)
            return;

        dogIsChasing = false;

        if (dog != null)
        {
            dog.GoToBone(
                boneTransform
            );
        }

        ShowChallengeText(
            "¡Bien! El perro fue distraído."
        );

        challengeActive = false;
    }
}