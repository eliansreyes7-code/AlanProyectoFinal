using UnityEngine;

public class Bone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DogChallengeManager challengeManager;
    [SerializeField] private Transform throwTarget;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 2f;

    private Transform player;
    private Renderer boneRenderer;
    private Collider boneCollider;

    private bool hasBone;
    private bool boneThrown;

    private void Awake()
    {
        boneRenderer =
            GetComponent<Renderer>();

        boneCollider =
            GetComponent<Collider>();
    }

    private void Start()
    {
        WheelchairController wheelchair =
            FindFirstObjectByType<WheelchairController>();

        if (wheelchair != null)
            player = wheelchair.transform;
    }

    private void Update()
    {
        if (player == null || boneThrown)
            return;

        if (!hasBone)
        {
            float distance =
                Vector3.Distance(
                    player.position,
                    transform.position
                );

            if (distance <= interactionDistance &&
                Input.GetKeyDown(KeyCode.E))
            {
                PickUpBone();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ThrowBone();
        }
    }

    private void PickUpBone()
    {
        hasBone = true;

        if (boneRenderer != null)
            boneRenderer.enabled = false;

        if (boneCollider != null)
            boneCollider.enabled = false;

        Debug.Log("Hueso recogido.");
    }

    private void ThrowBone()
    {
        if (throwTarget == null)
            return;

        hasBone = false;
        boneThrown = true;

        transform.position =
            throwTarget.position;

        if (boneRenderer != null)
            boneRenderer.enabled = true;

        if (boneCollider != null)
            boneCollider.enabled = true;

        if (challengeManager != null)
        {
            challengeManager.BoneThrown(
                transform
            );
        }

        Debug.Log("Hueso lanzado.");
    }
}