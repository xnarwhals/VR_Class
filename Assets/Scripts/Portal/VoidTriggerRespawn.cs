using UnityEngine;

public class VoidTriggerRespawn : MonoBehaviour
{
    [SerializeField] Transform respawnPoint;
    [SerializeField] bool resetVelocity = true;

    void OnTriggerEnter(Collider other)
    {
        if (respawnPoint == null)
        {
            return;
        }

        Rigidbody rb = other.attachedRigidbody;
        Transform target = rb != null ? rb.transform : other.transform;

        target.position = respawnPoint.position;
        target.rotation = respawnPoint.rotation;

        if (resetVelocity && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
