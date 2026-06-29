using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DoorSceneTransition : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string sceneToLoad = "City";

    [Header("UI")]
    [SerializeField] private GameObject messageObject;
    [SerializeField] private Image flashImage;

    [Header("Flash")]
    [SerializeField] private float flashDuration = 0.7f;
    [SerializeField] private float waitBeforeLoad = 0.2f;

    private bool playerInside;
    private bool isTransitioning;

    private void Start()
    {
        if (messageObject != null)
            messageObject.SetActive(false);

        if (flashImage != null)
        {
            Color color = flashImage.color;
            color.a = 0f;
            flashImage.color = color;
        }
    }

    private void Update()
    {
        if (playerInside && !isTransitioning && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(TransitionToScene());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (messageObject != null)
            messageObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        if (messageObject != null)
            messageObject.SetActive(false);
    }

    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;

        if (messageObject != null)
            messageObject.SetActive(false);

        float timer = 0f;

        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / flashDuration);

            if (flashImage != null)
            {
                Color color = flashImage.color;
                color.a = alpha;
                flashImage.color = color;
            }

            yield return null;
        }

        yield return new WaitForSeconds(waitBeforeLoad);

        SceneManager.LoadScene(sceneToLoad);
    }
}