using System.Collections;
using TMPro;
using UnityEngine;

public class FinalRoadStart : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text messageText;

    [Header("Traffic")]
    [SerializeField] private GameObject finalTrafficSystem;

    private bool activated = false;

    private void Start()
    {
        if (finalTrafficSystem != null)
            finalTrafficSystem.SetActive(false);

        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;

        WheelchairController player =
            other.GetComponentInParent<WheelchairController>();

        if (player == null)
            return;

        activated = true;

        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text =
                "Intenta cruzar la carretera";
        }

        if (finalTrafficSystem != null)
        {
            finalTrafficSystem.SetActive(true);
        }

        StartCoroutine(
            HideMessage()
        );
    }

    private IEnumerator HideMessage()
    {
        yield return new WaitForSeconds(4f);

        if (messageText != null)
        {
            messageText.text = "";
            messageText.gameObject.SetActive(false);
        }
    }
}