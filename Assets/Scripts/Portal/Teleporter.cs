using UnityEngine;
using UnityEngine.Events;

public class Teleporter : MonoBehaviour
{
    [Header("Access Control")]
    [SerializeField] private bool requireLevelCompletion = true;
    [SerializeField] private bool stayUnlockedOnceUsed = true;

    [Header("Teleport Settings")]
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private Transform teleportDestination;
    [SerializeField] private UnityEvent onTeleport;

    private bool _isPermanentlyUnlocked;

    private void Start()
    {
        if (teleportDestination == null)
        {
            Debug.LogError("Teleport destination is not assigned.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (!CanUseTeleporter())
        {
            return;
        }

        Transform playerTransform = other.transform;
        playerTransform.position = teleportDestination.position;
        playerTransform.rotation = teleportDestination.rotation;
        MetarrowAudioManager.Instance?.PlaySound(teleportSound);
        onTeleport?.Invoke();
    }

    private bool CanUseTeleporter()
    {
        if (!requireLevelCompletion)
        {
            return true;
        }

        if (_isPermanentlyUnlocked)
        {
            return true;
        }

        MetarrowGameManager manager = MetarrowGameManager.Instance;
        if (manager == null || !manager.HasLevelCompleted)
        {
            return false;
        }

        if (stayUnlockedOnceUsed)
        {
            _isPermanentlyUnlocked = true;
        }

        return true;
    }
}
