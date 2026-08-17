using UnityEngine;

public class FinalTrafficAlwaysOn : MonoBehaviour
{
    [Header("Final Traffic")]
    [SerializeField] private GameObject finalTrafficSystem;

    private void Awake()
    {
        ActivateTraffic();
    }

    private void Start()
    {
        ActivateTraffic();
    }

    private void Update()
    {
        /*
         * Si cualquier otro script intenta apagar
         * el tráfico, lo volvemos a encender.
         */
        if (finalTrafficSystem != null &&
            !finalTrafficSystem.activeSelf)
        {
            finalTrafficSystem.SetActive(true);

            Debug.Log(
                "FinalTrafficAlwaysOn: tráfico reactivado."
            );
        }
    }

    private void ActivateTraffic()
    {
        if (finalTrafficSystem == null)
        {
            Debug.LogError(
                "FinalTrafficAlwaysOn: falta asignar Final Traffic System."
            );

            return;
        }

        finalTrafficSystem.SetActive(true);
    }
}