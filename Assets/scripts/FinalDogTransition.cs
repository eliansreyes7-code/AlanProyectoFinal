using System.Collections;
using UnityEngine;

public class FinalDogTransition : MonoBehaviour
{
    // =====================================================
    // DESTINO DEL PLAYER
    // =====================================================

    [Header("Road Destination")]
    [Tooltip("Punto donde aparecerá el jugador en la carretera.")]
    [SerializeField] private Transform roadSpawnPoint;

    // =====================================================
    // PERRO
    // =====================================================

    [Header("Final Dog")]

    [Tooltip("DogAI del perro.")]
    [SerializeField] private DogAI dogAI;

    [Tooltip("Objeto raíz del perro.")]
    [SerializeField] private Transform dogTransform;

    [Tooltip("Punto donde aparecerá el perro en la escena final.")]
    [SerializeField] private Transform finalDogPosition;

    [Tooltip("Punto exacto del perro que mirará la cámara. Preferiblemente en la cabeza.")]
    [SerializeField] private Transform dogLookTarget;

    // =====================================================
    // CÁMARA
    // =====================================================

    [Header("Camera")]

    [Tooltip("Script de cámara del jugador.")]
    [SerializeField] private WheelchairCameraLook cameraLook;

    [Tooltip("Transform real de la cámara. Si queda vacío, se toma del CameraLook.")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("Tiempo que tarda en girar hasta mirar al perro.")]
    [SerializeField] private float cameraTurnDuration = 0.8f;

    [Tooltip("Tiempo que permanece mirando al perro mientras se acerca.")]
    [SerializeField] private float lookAtDogHoldTime = 1f;

    // =====================================================
    // LADRIDO
    // =====================================================

    [Header("Dog Bark")]

    [SerializeField] private AudioSource barkAudioSource;
    [SerializeField] private AudioClip barkSound;

    [Tooltip("Tiempo después del ladrido antes del fade.")]
    [SerializeField] private float waitAfterBark = 0.4f;

    // =====================================================
    // FADE
    // =====================================================

    [Header("White Transition")]

    [SerializeField] private CanvasGroup whiteFlash;

    [SerializeField] private float fadeToWhiteDuration = 0.45f;
    [SerializeField] private float whiteHoldDuration = 0.35f;
    [SerializeField] private float fadeFromWhiteDuration = 0.8f;

    // =====================================================
    // ACTIVACIÓN
    // =====================================================

    [Header("Trigger")]

    [Tooltip("Se habilita al terminar el reto del hueso.")]
    [SerializeField] private bool transitionEnabled = false;

    // =====================================================
    // DEBUG
    // =====================================================

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    // =====================================================
    // VARIABLES
    // =====================================================

    private bool transitionStarted = false;

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        if (cameraTransform == null &&
            cameraLook != null)
        {
            cameraTransform =
                cameraLook.transform;
        }

        if (dogAI == null &&
            dogTransform != null)
        {
            dogAI =
                dogTransform.GetComponent<DogAI>();
        }

        if (whiteFlash != null)
        {
            whiteFlash.interactable = false;
            whiteFlash.blocksRaycasts = false;
        }
    }

    // =====================================================
    // ACTIVAR TRANSICIÓN
    // =====================================================

    public void EnableFinalTransition()
    {
        transitionEnabled = true;

        if (showDebugMessages)
        {
            Debug.Log(
                "FinalDogTransition: transición habilitada."
            );
        }
    }

    // =====================================================
    // TRIGGER
    // =====================================================

    private void OnTriggerEnter(Collider other)
    {
        if (showDebugMessages)
        {
            Debug.Log(
                "FinalDogTransition: entró " +
                other.name
            );
        }

        if (!transitionEnabled)
            return;

        if (transitionStarted)
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

        transitionStarted = true;

        StartCoroutine(
            FinalSequence(player)
        );
    }

    // =====================================================
    // SECUENCIA FINAL
    // =====================================================

    private IEnumerator FinalSequence(
        WheelchairController player)
    {
        Rigidbody playerRb =
            player.GetComponent<Rigidbody>();

        // =================================================
        // 1. DETENER PLAYER
        // =================================================

        player.StopMovement();
        player.enabled = false;

        if (playerRb != null)
        {
            playerRb.linearVelocity =
                Vector3.zero;

            playerRb.angularVelocity =
                Vector3.zero;
        }

        // =================================================
        // 2. BLOQUEAR CONTROL MANUAL DE CÁMARA
        // =================================================

        if (cameraLook != null)
        {
            cameraLook.SetLookEnabled(false);
        }

        // =================================================
        // 3. MOVER EL PERRO A FINAL DOG POSITION
        // =================================================

        if (dogTransform != null &&
            finalDogPosition != null)
        {
            Rigidbody dogRb =
                dogTransform.GetComponent<Rigidbody>();

            if (dogRb != null)
            {
                dogRb.linearVelocity =
                    Vector3.zero;

                dogRb.angularVelocity =
                    Vector3.zero;

                dogRb.position =
                    finalDogPosition.position;

                dogRb.rotation =
                    finalDogPosition.rotation;
            }
            else
            {
                dogTransform.SetPositionAndRotation(
                    finalDogPosition.position,
                    finalDogPosition.rotation
                );
            }

            yield return new WaitForFixedUpdate();
            yield return null;

            if (showDebugMessages)
            {
                Debug.Log(
                    "FinalDogTransition: perro colocado en FinalDogPosition."
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "FinalDogTransition: falta Dog Transform o Final Dog Position."
            );
        }

        // =================================================
        // 4. HACER QUE EL PERRO CORRA HACIA EL PLAYER
        // =================================================

        if (dogAI != null)
        {
            dogAI.ChasePlayer(
                player.transform
            );

            if (showDebugMessages)
            {
                Debug.Log(
                    "FinalDogTransition: perro persiguiendo al jugador."
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "FinalDogTransition: falta DogAI."
            );
        }

        // =================================================
        // 5. GIRAR CÁMARA HACIA EL PERRO
        // =================================================

        if (cameraTransform != null &&
            dogLookTarget != null)
        {
            yield return StartCoroutine(
                RotateCameraToTarget(
                    cameraTransform,
                    dogLookTarget,
                    cameraTurnDuration
                )
            );
        }
        else
        {
            Debug.LogWarning(
                "FinalDogTransition: falta Camera Transform o Dog Look Target."
            );
        }

        // =================================================
        // 6. ESPERAR MIRANDO CÓMO SE ACERCA EL PERRO
        // =================================================

        yield return new WaitForSeconds(
            lookAtDogHoldTime
        );

        // =================================================
        // 7. LADRIDO
        // =================================================

        if (barkAudioSource != null &&
            barkSound != null)
        {
            barkAudioSource.PlayOneShot(
                barkSound
            );
        }

        if (waitAfterBark > 0f)
        {
            yield return new WaitForSeconds(
                waitAfterBark
            );
        }

        // =================================================
        // 8. DETENER AL PERRO ANTES DEL CAMBIO
        // =================================================

        if (dogAI != null)
        {
            dogAI.StopDog();
        }

        // =================================================
        // 9. FADE A BLANCO
        // =================================================

        yield return StartCoroutine(
            FadeWhite(
                0f,
                1f,
                fadeToWhiteDuration
            )
        );

        yield return new WaitForSeconds(
            whiteHoldDuration
        );

        // =================================================
        // 10. COMPROBAR ROAD SPAWN
        // =================================================

        if (roadSpawnPoint == null)
        {
            Debug.LogError(
                "FinalDogTransition: falta Road Spawn Point."
            );

            player.enabled = true;

            if (cameraLook != null)
            {
                cameraLook.SetLookEnabled(true);
            }

            transitionStarted = false;

            yield break;
        }

        // =================================================
        // 11. TELETRANSPORTAR PLAYER
        // =================================================

        if (playerRb != null)
        {
            playerRb.linearVelocity =
                Vector3.zero;

            playerRb.angularVelocity =
                Vector3.zero;

            playerRb.position =
                roadSpawnPoint.position;

            playerRb.rotation =
                roadSpawnPoint.rotation;
        }
        else
        {
            player.transform.SetPositionAndRotation(
                roadSpawnPoint.position,
                roadSpawnPoint.rotation
            );
        }

        yield return new WaitForFixedUpdate();

        // =================================================
        // 12. FADE DE SALIDA
        // =================================================

        yield return StartCoroutine(
            FadeWhite(
                1f,
                0f,
                fadeFromWhiteDuration
            )
        );

        // =================================================
        // 13. DEVOLVER CONTROL
        // =================================================

        player.enabled = true;

        if (cameraLook != null)
        {
            cameraLook.SetLookEnabled(true);
        }

        if (showDebugMessages)
        {
            Debug.Log(
                "FinalDogTransition: transición final completada."
            );
        }
    }

    // =====================================================
    // GIRAR CÁMARA HACIA TARGET
    // =====================================================

    private IEnumerator RotateCameraToTarget(
        Transform cam,
        Transform target,
        float duration)
    {
        if (cam == null ||
            target == null)
        {
            yield break;
        }

        Quaternion startRotation =
            cam.rotation;

        Vector3 direction =
            target.position -
            cam.position;

        if (direction.sqrMagnitude <
            0.0001f)
        {
            yield break;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );

        if (duration <= 0f)
        {
            cam.rotation =
                targetRotation;

            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    duration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            cam.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        cam.rotation =
            targetRotation;
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
}