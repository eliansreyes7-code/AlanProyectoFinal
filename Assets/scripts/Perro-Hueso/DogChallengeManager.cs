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

    [Header("Player")]
    [SerializeField] private Transform restartPoint;

    [Header("Camera")]
    [SerializeField] private WheelchairCameraLook cameraLook;

    [Header("UI")]
    [SerializeField] private TMP_Text challengeText;

    [Header("Intro")]
    [SerializeField] private float cameraTurnDuration = 0.6f;
    [SerializeField] private float dogLookTime = 0.8f;

    [Header("Restart")]
    [SerializeField] private float restartDelay = 1.2f;

    private Transform player;

    private WheelchairController wheelchairController;
    private Rigidbody playerRigidbody;

    private bool challengeActive;
    private bool restarting;

    // =====================================================
    // INICIAR RETO
    // =====================================================

    public void StartDogChallenge(
        Transform newPlayer)
    {
        if (challengeActive ||
            restarting)
            return;

        if (newPlayer == null)
            return;

        player = newPlayer;

        wheelchairController =
            player.GetComponent
            <WheelchairController>();

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
                player.GetComponentInParent
                <Rigidbody>();
        }

        challengeActive = true;

        StartCoroutine(
            StartChallengeSequence()
        );
    }

    // =====================================================
    // INTRO
    // =====================================================

    private IEnumerator StartChallengeSequence()
    {
        // Bloqueamos brevemente la silla.
        if (wheelchairController != null)
            wheelchairController.enabled = false;

        if (challengeText != null)
        {
            challengeText.text =
                "¡Cuidado!";
        }

        // Cámara gira + zoom.
        if (cameraLook != null &&
            dogLookTarget != null)
        {
            cameraLook.LookAtTargetWithZoom(
                dogLookTarget,
                cameraTurnDuration,
                dogLookTime
            );

            yield return new WaitForSeconds(
                (cameraTurnDuration * 2f) +
                dogLookTime
            );
        }

        // Devolvemos movimiento.
        if (wheelchairController != null)
            wheelchairController.enabled = true;

        if (challengeText != null)
        {
            challengeText.text =
                "¡Escapa! Llega al siguiente punto.";
        }

        if (dog != null)
        {
            dog.ChasePlayer(player);
        }
    }

    // =====================================================
    // PERRO ATRAPA AL PLAYER
    // =====================================================

    public void PlayerCaught()
    {
        if (!challengeActive ||
            restarting)
            return;

        StartCoroutine(
            PlayerCaughtSequence()
        );
    }

    private IEnumerator PlayerCaughtSequence()
    {
        restarting = true;
        challengeActive = false;

        // Detener silla.
        if (wheelchairController != null)
            wheelchairController.enabled = false;

        if (dog != null)
            dog.StopDog();

        if (challengeText != null)
        {
            challengeText.text =
                "El perro te alcanzó.";
        }

        yield return new WaitForSeconds(
            restartDelay
        );

        // =================================================
        // REAPARECER
        // =================================================

        if (restartPoint != null &&
            player != null)
        {
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

        // Reiniciar perro.
        if (dog != null)
            dog.ResetDog();

        // Permitir que el trigger vuelva a funcionar.
        if (dogTrigger != null)
            dogTrigger.ResetTrigger();

        if (challengeText != null)
            challengeText.text = "";

        // Reactivar jugador.
        if (wheelchairController != null)
            wheelchairController.enabled = true;

        restarting = false;
    }

    // =====================================================
    // HUESO
    // =====================================================

    public void BoneThrown(
        Transform boneTransform)
    {
        if (boneTransform == null)
            return;

        if (dog != null)
        {
            dog.GoToBone(
                boneTransform
            );
        }

        if (challengeText != null)
        {
            challengeText.text =
                "¡Bien! El perro fue distraído.";
        }

        challengeActive = false;
    }
}