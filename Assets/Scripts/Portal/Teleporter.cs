using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private Transform teleportDestination;

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
