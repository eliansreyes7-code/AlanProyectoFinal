using System.Collections;
using TMPro;
using UnityEngine;

public class DogChallengeManager : MonoBehaviour
{
    [Header("Dog")]
    [SerializeField] private DogAI dog;
    [SerializeField] private Transform dogLookTarget;

    [Header("Dog Final Target")]
    [Tooltip("Lugar hacia donde correrá el perro después de tomar el hueso.")]
    [SerializeField] private Transform boneThrowTarget;

    [Header("Bone")]
    [SerializeField] private Bone bone;

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

    [Header("Bone Cinematic")]
    [SerializeField] private float boneCameraTurnDuration = 0.6f;
    [SerializeField] private float dogRunWatchTime = 2.5f;

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

        if (restarting || challengeCompleted)
            return;

        CheckIfDogCaughtPlayer();
    }


    // =====================================================
    // INICIAR RETO
    // =====================================================

    public void StartDogChallenge(Transform newPlayer)
    {
        if (challengeActive ||
            restarting ||
            challengeCompleted)
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
        if (wheelchairController != null)
            wheelchairController.enabled = false;

        ShowChallengeText("¡Cuidado!");

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
            "¡Escapa! Llega lo más rápido posible al siguiente punto."
        );

        if (wheelchairController != null)
            wheelchairController.enabled = true;

        if (dog != null)
        {
            dog.ChasePlayer(player);
            dogIsChasing = true;
        }
    }


    // =====================================================
    // HUESO - PLAYER CERCA
    // =====================================================

    public void ShowBonePrompt()
    {
        if (!challengeActive ||
            challengeCompleted)
            return;

        ShowChallengeText(
            "Presiona F para tomar el hueso"
        );
    }

    public void HideBonePrompt()
    {
        if (!challengeActive ||
            challengeCompleted)
            return;

        // Si todavía lo persigue el perro,
        // volvemos a mostrar el mensaje de escape.
        if (dogIsChasing)
        {
            ShowChallengeText(
                "¡Escapa! Llega lo más rápido posible al siguiente punto."
            );
        }
    }


    // =====================================================
    // HUESO RECOGIDO
    // =====================================================

    public void BoneTaken()
    {
        if (!challengeActive ||
            challengeCompleted)
            return;

        StartCoroutine(
            BoneSequence()
        );
    }

    private IEnumerator BoneSequence()
    {
        challengeCompleted = true;
        dogIsChasing = false;

        // Bloqueamos brevemente la silla para que
        // el jugador pueda ver qué ocurre.
        if (wheelchairController != null)
            wheelchairController.enabled = false;

        ShowChallengeText(
            "Distrajiste al perro"
        );

        // El perro deja al Player y corre
        // hacia el punto donde simulamos
        // que fue lanzado el hueso.
        if (dog != null &&
            boneThrowTarget != null)
        {
            dog.GoToBone(
                boneThrowTarget
            );
        }

        /*
         * Miramos al perro.
         *
         * DogLookTarget es hijo del perro,
         * así que se moverá junto con él.
         * Esto permite seguir visualmente
         * al perro mientras corre.
         */
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

        // Terminó el reto.
        HideChallengeText();

        challengeActive = false;

        // Devolver movimiento.
        if (wheelchairController != null)
            wheelchairController.enabled = true;

        /*
         * Ahora el usuario puede continuar
         * hacia el siguiente checkpoint.
         */
    }


    // =====================================================
    // PERRO ATRAPA AL PLAYER
    // =====================================================

    private void CheckIfDogCaughtPlayer()
    {
        if (dog == null ||
            player == null)
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
    // REINICIO
    // =====================================================

    private IEnumerator RestartChallengeSequence()
    {
        if (restarting)
            yield break;

        restarting = true;

        challengeActive = false;
        dogIsChasing = false;

        if (dog != null)
            dog.StopDog();

        if (wheelchairController != null)
            wheelchairController.enabled = false;

        if (cameraLook != null)
            cameraLook.SetLookEnabled(false);

        ShowChallengeText(
            "El perro te alcanzó"
        );

        yield return new WaitForSeconds(
            restartDelay
        );

        // Pantalla a blanco.
        yield return StartCoroutine(
            FadeWhite(
                0f,
                1f,
                flashInDuration
            )
        );

        // Teletransportar mientras no se ve.
        TeleportPlayerToRestartPoint();

        // Reiniciar perro.
        if (dog != null)
            dog.ResetDog();

        // Reiniciar hueso.
        if (bone != null)
            bone.ResetBone();

        // Reiniciar trigger.
        if (dogTrigger != null)
            dogTrigger.ResetTrigger();

        challengeCompleted = false;

        HideChallengeText();

        yield return new WaitForSeconds(
            whiteHoldDuration
        );

        // Volver del blanco.
        yield return StartCoroutine(
            FadeWhite(
                1f,
                0f,
                flashOutDuration
            )
        );

        if (wheelchairController != null)
            wheelchairController.enabled = true;

        if (cameraLook != null)
            cameraLook.SetLookEnabled(true);

        restarting = false;
    }


    // =====================================================
    // TELEPORT
    // =====================================================

    private void TeleportPlayerToRestartPoint()
    {
        if (player == null ||
            restartPoint == null)
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
    // WHITE FLASH
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
}