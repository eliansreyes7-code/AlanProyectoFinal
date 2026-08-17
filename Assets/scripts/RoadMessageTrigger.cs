using System.Collections;
using TMPro;
using UnityEngine;

public class RoadMessageTrigger : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    [SerializeField] private float duration = 4f;

    private bool activated = false;

    private void Start()
    {
        if (messageText != null)
        {
            messageText.text = "";
            messageText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;

        WheelchairController player =
            other.GetComponentInParent
                <WheelchairController>();

        if (player == null)
            return;

        activated = true;

        StartCoroutine(
            ShowMessage()
        );
    }

    private IEnumerator ShowMessage()
    {
        if (messageText == null)
            yield break;

        messageText.gameObject.SetActive(true);

        messageText.text =
            "Intenta cruzar la carretera";

        yield return new WaitForSeconds(
            duration
        );

        messageText.text = "";

        messageText.gameObject.SetActive(false);
    }
}