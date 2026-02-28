using UnityEngine;
using UnityEngine.Events;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private Transform teleportDestination;
    [SerializeField] private UnityEvent onTeleport;

    private void Start()
    {
        if (teleportDestination == null)
        {
            Debug.LogError("Teleport destination is not assigned.", this);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Transform playerTransform = other.transform;
            playerTransform.position = teleportDestination.position;
            playerTransform.rotation = teleportDestination.rotation;
        }
    }
}
