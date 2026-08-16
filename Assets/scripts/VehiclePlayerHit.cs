using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VehiclePlayerHit : MonoBehaviour
{
    public enum HitBehaviour
    {
        RespawnPlayer,
        LoadScene
    }

    [Header("Hit Behaviour")]
    [SerializeField] private HitBehaviour hitBehaviour =
        HitBehaviour.RespawnPlayer;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;

    [Tooltip("Tiempo que espera después del choque antes de devolver al jugador.")]
    [SerializeField] private float respawnDelay = 1f;

    [Header("Scene - Carretera final")]
    [SerializeField] private string sceneToLoad = "office";

    [Header("Screen Effect")]
    [SerializeField] private CanvasGroup whiteFlash;

    [SerializeField] private float flashInDuration = 0.25f;
    [SerializeField] private float whiteHoldDuration = 0.25f;
    [SerializeField] private float flashOutDuration = 0.6f;

    [Header("Systems To Reset")]
    [SerializeField] private RouteManager routeManager;
    [SerializeField] private DogChallengeManager dogChallengeManager;

    private bool hitInProgress = false;

    // =====================================================
    // CONFIGURACIÓN DESDE SPAWNER
    // =====================================================

    public void SetupHitSystem(
        Transform newRespawnPoint,
        CanvasGroup newWhiteFlash)
    {
        respawnPoint = newRespawnPoint;
        whiteFlash = newWhiteFlash;

        /*
         * Estas referencias pertenecen a la escena,
         * así que las buscamos automáticamente.
         */
        if (routeManager == null)
        {
            routeManager =
                FindFirstObjectByType<RouteManager>();
        }

        if (dogChallengeManager == null)
        {
            dogChallengeManager =
                FindFirstObjectByType<DogChallengeManager>();
        }
    }

    // =====================================================
    // TRIGGER
    // =====================================================

    private void OnTriggerEnter(Collider other)
    {
        if (hitInProgress)
            return;

        WheelchairController player =
            other.GetComponentInParent<WheelchairController>();

        if (player == null)
            return;

        StartCoroutine(
            HandlePlayerHit(player)
        );
    }

    // =====================================================
    // CHOQUE
    // =====================================================

    private IEnumerator HandlePlayerHit(
        WheelchairController player)
    {
        hitInProgress = true;

        Rigidbody playerRb =
            player.GetComponent<Rigidbody>();

        // =================================================
        // PARAR AL PLAYER
        // =================================================

        player.StopMovement();
        player.enabled = false;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        // =================================================
        // ESPERAR 1 SEGUNDO
        // =================================================

        yield return new WaitForSeconds(
            respawnDelay
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

        yield return new WaitForSeconds(
            whiteHoldDuration
        );

        // =================================================
        // RESPawn NORMAL
        // =================================================

        if (hitBehaviour ==
            HitBehaviour.RespawnPlayer)
        {
            // =============================================
            // REINICIAR RETO DEL PERRO
            // =============================================

            if (dogChallengeManager != null)
            {
                dogChallengeManager
                    .ResetChallengeFromVehicleHit();
            }

            // =============================================
            // REINICIAR CHECKPOINTS
            // =============================================

            if (routeManager != null)
            {
                routeManager
                    .ResetRouteToStart();
            }

            // =============================================
            // TELEPORT
            // =============================================

            RespawnPlayer(
                player,
                playerRb
            );

            yield return new WaitForFixedUpdate();

            // =============================================
            // QUITAR BLANCO
            // =============================================

            yield return StartCoroutine(
                FadeWhite(
                    1f,
                    0f,
                    flashOutDuration
                )
            );

            player.enabled = true;

            hitInProgress = false;
        }

        // =================================================
        // CARRETERA FINAL
        // =================================================

        else
        {
            SceneManager.LoadScene(
                sceneToLoad
            );
        }
    }

    // =====================================================
    // TELEPORT
    // =====================================================

    private void RespawnPlayer(
        WheelchairController player,
        Rigidbody rb)
    {
        if (respawnPoint == null)
        {
            Debug.LogError(
                "VehiclePlayerHit: Respawn Point no asignado."
            );

            return;
        }

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;

            rb.position =
                respawnPoint.position;

            rb.rotation =
                respawnPoint.rotation;
        }
        else
        {
            player.transform.SetPositionAndRotation(
                respawnPoint.position,
                respawnPoint.rotation
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
}