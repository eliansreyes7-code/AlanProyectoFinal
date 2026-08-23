using System.Collections;
using UnityEngine;

public class Office2WakeUp : MonoBehaviour
{
    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("References")]
    [SerializeField] private WheelchairController playerController;

    [Tooltip("Transform real de la cámara.")]
    [SerializeField] private Transform playerCamera;

    [Tooltip("Script normal que controla la cámara.")]
    [SerializeField] private WheelchairCameraLook cameraLook;

    // =====================================================
    // UI
    // =====================================================

    [Header("UI")]

    [Tooltip("Objeto MISION que debe ocultarse durante la animación.")]
    [SerializeField] private GameObject missionUI;

    [Tooltip("Objeto ControlesUI que contiene Q / E / F.")]
    [SerializeField] private GameObject controlsUI;

    [Tooltip("Objeto Salir / Presiona F para salir. Siempre permanecerá oculto.")]
    [SerializeField] private GameObject exitPrompt;

    // =====================================================
    // RELOJ / DESPERTADOR
    // =====================================================

    [Header("Alarm Clock")]

    [Tooltip("Empty colocado sobre el reloj.")]
    [SerializeField] private Transform clockLookTarget;

    [Tooltip("AudioSource del despertador.")]
    [SerializeField] private AudioSource alarmAudioSource;

    [Tooltip("Sonido del despertador.")]
    [SerializeField] private AudioClip alarmSound;

    [Tooltip("Tiempo que tarda en mirar el reloj.")]
    [SerializeField] private float lookAtClockDuration = 2f;

    [Tooltip("Tiempo mirando el reloj.")]
    [SerializeField] private float clockHoldDuration = 1.5f;

    // =====================================================
    // TIEMPOS
    // =====================================================

    [Header("Sequence Timing")]

    [SerializeField] private float initialPause = 0.5f;

    [Tooltip("Reloj -> izquierda.")]
    [SerializeField] private float frontToLeftDuration = 2f;

    [SerializeField] private float leftHoldDuration = 0.8f;

    [Tooltip("Izquierda -> derecha.")]
    [SerializeField] private float leftToRightDuration = 3.2f;

    [SerializeField] private float rightHoldDuration = 0.8f;

    [Tooltip("Derecha -> suelo.")]
    [SerializeField] private float rightToDownDuration = 2f;

    [SerializeField] private float downHoldDuration = 1f;

    [Tooltip("Suelo -> frente.")]
    [SerializeField] private float downToFrontDuration = 2f;

    [SerializeField] private float finalPause = 0.6f;

    // =====================================================
    // ÁNGULOS
    // =====================================================

    [Header("Look Around")]

    [SerializeField] private float leftAngle = -45f;
    [SerializeField] private float rightAngle = 50f;
    [SerializeField] private float sideVerticalAngle = 1.5f;
    [SerializeField] private float downAngle = 35f;

    // =====================================================
    // VARIABLES
    // =====================================================

    private Quaternion frontRotation;

    private bool sequenceRunning = false;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        /*
         * MUY IMPORTANTE:
         *
         * Desde antes del primer frame ocultamos
         * misión y controles.
         *
         * Así no aparecen ni por un instante
         * cuando carga Office2.
         */

        if (missionUI != null)
        {
            missionUI.SetActive(false);
        }

        if (controlsUI != null)
        {
            controlsUI.SetActive(false);
        }

        // "Presiona F para salir" siempre apagado.
        if (exitPrompt != null)
        {
            exitPrompt.SetActive(false);
        }
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        // =================================================
        // BUSCAR PLAYER
        // =================================================

        if (playerController == null)
        {
            playerController =
                GetComponent<WheelchairController>();
        }

        // =================================================
        // BUSCAR CÁMARA
        // =================================================

        if (playerCamera == null)
        {
            Camera mainCamera =
                Camera.main;

            if (mainCamera != null)
            {
                playerCamera =
                    mainCamera.transform;
            }
        }

        if (playerCamera == null)
        {
            Debug.LogError(
                "Office2WakeUp: no se encontró Player Camera."
            );

            return;
        }

        // =================================================
        // BUSCAR CAMERA LOOK
        // =================================================

        if (cameraLook == null)
        {
            cameraLook =
                FindFirstObjectByType<WheelchairCameraLook>();
        }

        // =================================================
        // GUARDAR FRENTE
        // =================================================

        frontRotation =
            playerCamera.localRotation;

        // =================================================
        // COMENZAR CINEMÁTICA
        // =================================================

        StartCoroutine(
            WakeUpSequence()
        );
    }

    // =====================================================
    // SECUENCIA PRINCIPAL
    // =====================================================

    private IEnumerator WakeUpSequence()
    {
        if (sequenceRunning)
            yield break;

        sequenceRunning = true;

        // =================================================
        // ASEGURAR UI APAGADA
        // =================================================

        if (missionUI != null)
        {
            missionUI.SetActive(false);
        }

        if (controlsUI != null)
        {
            controlsUI.SetActive(false);
        }

        if (exitPrompt != null)
        {
            exitPrompt.SetActive(false);
        }

        // =================================================
        // BLOQUEAR PLAYER
        // =================================================

        if (playerController != null)
        {
            playerController.StopMovement();
            playerController.enabled = false;
        }

        // =================================================
        // BLOQUEAR CÁMARA NORMAL
        // =================================================

        if (cameraLook != null)
        {
            cameraLook.enabled = false;
        }

        yield return null;

        playerCamera.localRotation =
            frontRotation;

        // =================================================
        // DESPERTADOR
        // =================================================

        if (alarmAudioSource != null &&
            alarmSound != null)
        {
            alarmAudioSource.clip =
                alarmSound;

            alarmAudioSource.loop =
                true;

            alarmAudioSource.Play();
        }

        yield return new WaitForSeconds(
            initialPause
        );

        // =================================================
        // 1. MIRAR RELOJ
        // =================================================

        if (clockLookTarget != null)
        {
            Quaternion clockRotation =
                GetLocalRotationToTarget(
                    clockLookTarget
                );

            yield return StartCoroutine(
                RotateContinuous(
                    clockRotation,
                    lookAtClockDuration
                )
            );

            yield return StartCoroutine(
                HoldRotation(
                    clockRotation,
                    clockHoldDuration
                )
            );
        }
        else
        {
            Debug.LogWarning(
                "Office2WakeUp: Clock Look Target no asignado."
            );
        }

        // =================================================
        // APAGAR DESPERTADOR
        // =================================================

        if (alarmAudioSource != null)
        {
            alarmAudioSource.Stop();
        }

        // =================================================
        // CALCULAR MIRADAS
        // =================================================

        Quaternion leftRotation =
            frontRotation *
            Quaternion.Euler(
                sideVerticalAngle,
                leftAngle,
                0f
            );

        Quaternion rightRotation =
            frontRotation *
            Quaternion.Euler(
                sideVerticalAngle,
                rightAngle,
                0f
            );

        Quaternion downRotation =
            frontRotation *
            Quaternion.Euler(
                downAngle,
                0f,
                0f
            );

        // =================================================
        // 2. RELOJ -> IZQUIERDA
        // =================================================

        yield return StartCoroutine(
            RotateContinuous(
                leftRotation,
                frontToLeftDuration
            )
        );

        yield return StartCoroutine(
            HoldRotation(
                leftRotation,
                leftHoldDuration
            )
        );

        // =================================================
        // 3. IZQUIERDA -> DERECHA
        // =================================================

        yield return StartCoroutine(
            RotateContinuous(
                rightRotation,
                leftToRightDuration
            )
        );

        yield return StartCoroutine(
            HoldRotation(
                rightRotation,
                rightHoldDuration
            )
        );

        // =================================================
        // 4. DERECHA -> SUELO
        // =================================================

        yield return StartCoroutine(
            RotateContinuous(
                downRotation,
                rightToDownDuration
            )
        );

        yield return StartCoroutine(
            HoldRotation(
                downRotation,
                downHoldDuration
            )
        );

        // =================================================
        // 5. SUELO -> FRENTE
        // =================================================

        yield return StartCoroutine(
            RotateContinuous(
                frontRotation,
                downToFrontDuration
            )
        );

        yield return StartCoroutine(
            HoldRotation(
                frontRotation,
                finalPause
            )
        );

        // =================================================
        // CINEMÁTICA TERMINÓ
        // =================================================

        // Devolver cámara.
        if (cameraLook != null)
        {
            cameraLook.enabled = true;
            cameraLook.SetLookEnabled(true);
        }

        // Devolver movimiento.
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // =================================================
        // MOSTRAR MISIÓN
        // =================================================

        if (missionUI != null)
        {
            missionUI.SetActive(true);
        }

        // =================================================
        // MOSTRAR Q / E / F
        // =================================================

        if (controlsUI != null)
        {
            controlsUI.SetActive(true);

            Debug.Log(
                "Office2WakeUp: ControlesUI ACTIVADO.",
                controlsUI
            );
        }
        else
        {
            Debug.LogError(
                "Office2WakeUp: CONTROLES UI NO ESTÁ ASIGNADO."
            );
        }

        // El mensaje viejo de salida no debe volver a aparecer.
        if (exitPrompt != null)
        {
            exitPrompt.SetActive(false);
        }

        sequenceRunning = false;

        Debug.Log(
            "Office2WakeUp: secuencia completada."
        );
    }

    // =====================================================
    // ROTACIÓN HACIA TARGET
    // =====================================================

    private Quaternion GetLocalRotationToTarget(
        Transform target)
    {
        Vector3 direction =
            target.position -
            playerCamera.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            return playerCamera.localRotation;
        }

        Quaternion worldRotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );

        if (playerCamera.parent != null)
        {
            return
                Quaternion.Inverse(
                    playerCamera.parent.rotation
                ) *
                worldRotation;
        }

        return worldRotation;
    }

    // =====================================================
    // ROTACIÓN SUAVE
    // =====================================================

    private IEnumerator RotateContinuous(
        Quaternion targetRotation,
        float duration)
    {
        Quaternion startRotation =
            playerCamera.localRotation;

        if (duration <= 0f)
        {
            playerCamera.localRotation =
                targetRotation;

            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            float smoothT =
                t * t *
                (3f - 2f * t);

            playerCamera.localRotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    smoothT
                );

            yield return null;
        }

        playerCamera.localRotation =
            targetRotation;
    }

    // =====================================================
    // MANTENER MIRADA
    // =====================================================

    private IEnumerator HoldRotation(
        Quaternion rotation,
        float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            playerCamera.localRotation =
                rotation;

            yield return null;
        }

        playerCamera.localRotation =
            rotation;
    }
}