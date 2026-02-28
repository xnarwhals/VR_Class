using UnityEngine;

public class AntiClipZone : MonoBehaviour
{
    [SerializeField] private Transform safePosition;
    private Collider trigger;

    private void Start()
    {
        trigger = GetComponent<Collider>();
        if (safePosition == null)
        {
            Debug.LogError("Safe position not set. Please assign a safe position in the inspector.");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TeleportPlayerToSafePosition(other.gameObject);
        }
    }

    public void TeleportPlayerToSafePosition(GameObject player)
    {
        if (safePosition == null)
        {
            Debug.LogError("Cannot teleport player because safePosition is not assigned.", this);
            return;
        }

        player.transform.position = safePosition.position;
        player.transform.rotation = safePosition.rotation;
    }

    public void DisableZone()
    {
        if (trigger == null)
        {
            trigger = GetComponent<Collider>();
        }

        if (trigger != null)
        {
            trigger.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);

        Collider zoneCollider = trigger != null ? trigger : GetComponent<Collider>();
        if (zoneCollider != null)
        {
            Bounds bounds = zoneCollider.bounds;
            Gizmos.DrawCube(bounds.center, bounds.size);
            return;
        }

        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
