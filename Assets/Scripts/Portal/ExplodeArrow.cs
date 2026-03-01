using UnityEngine;

public class ExplodeArrow : Arrow
{
    [SerializeField] private AudioClip explosionHitSound;
    [SerializeField] private float explosionVolumeScale = 1.6f;

    [Header("Rocket Jump")]
    [SerializeField] private bool enableRocketJump = true;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private MonoBehaviour rocketJumpReceiver;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float rocketJumpImpulse = 7f;
    [SerializeField] private float minHeightBelowPlayer = 0.25f;
    [SerializeField] private float maxHorizontalDistance = 3f;
    [SerializeField] private float fallbackPlayerHeightOffset = 1f;

    private IExplosionJumpReceiver _resolvedJumpReceiver;
    private bool _hasWarnedInvalidJumpReceiver;
    private bool _hasWarnedMissingPlayerTag;

    protected override void Awake()
    {
        base.Awake();
        ResolveRocketJumpReferences();
    }

    protected override void OnArrowHit(RaycastHit hit, BaseTarget target)
    {
        PlayExplosionSound();
        TryApplyRocketJump(hit.point);

        Fracture fracture = hit.transform.GetComponent<Fracture>();
        if (fracture == null)
        {
            fracture = hit.transform.GetComponentInParent<Fracture>();
        }

        if (fracture != null)
        {
            if (fracture.TryGetComponent(out FractureExplosionEffect explosionEffect))
            {
                explosionEffect.CauseExplosionFracture(hit.point);
            }
            else
            {
                fracture.CauseFracture();
            }
        }
    }

    private void TryApplyRocketJump(Vector3 explosionPoint)
    {
        if (!enableRocketJump)
        {
            return;
        }

        ResolveRocketJumpReferences();
        if (playerTransform == null || _resolvedJumpReceiver == null)
        {
            return;
        }

        Vector3 playerReferencePosition = GetPlayerReferencePosition();
        Vector3 playerOffset = playerReferencePosition - explosionPoint;
        float verticalSeparation = Vector3.Dot(playerOffset, Vector3.up);
        if (verticalSeparation < minHeightBelowPlayer)
        {
            return;
        }

        Vector2 horizontalOffset = new Vector2(playerOffset.x, playerOffset.z);
        if (horizontalOffset.sqrMagnitude > maxHorizontalDistance * maxHorizontalDistance)
        {
            return;
        }

        _resolvedJumpReceiver.ApplyRocketJump(rocketJumpImpulse, explosionPoint);
    }

    private Vector3 GetPlayerReferencePosition()
    {
        if (_resolvedJumpReceiver is Component receiverComponent)
        {
            if (receiverComponent.TryGetComponent(out CharacterController controller))
            {
                return controller.bounds.center;
            }
        }

        if (playerTransform != null)
        {
            Camera cameraInRig = playerTransform.GetComponentInChildren<Camera>(true);
            if (cameraInRig != null)
            {
                return cameraInRig.transform.position;
            }

            return playerTransform.position + Vector3.up * fallbackPlayerHeightOffset;
        }

        return Vector3.zero;
    }

    private void ResolveRocketJumpReferences()
    {
        if (playerTransform == null && !string.IsNullOrEmpty(playerTag))
        {
            if (TryFindPlayerByTag(playerTag, out GameObject playerObject))
            {
                playerTransform = playerObject.transform;
            }
        }

        if (rocketJumpReceiver != null)
        {
            _resolvedJumpReceiver = rocketJumpReceiver as IExplosionJumpReceiver;
            if (_resolvedJumpReceiver == null)
            {
                if (!_hasWarnedInvalidJumpReceiver)
                {
                    Debug.LogWarning(
                        $"Assigned rocket jump receiver on {name} does not implement IExplosionJumpReceiver. Falling back to player search.",
                        rocketJumpReceiver);
                    _hasWarnedInvalidJumpReceiver = true;
                }
            }
            else
            {
                _hasWarnedInvalidJumpReceiver = false;
                return;
            }
        }

        _resolvedJumpReceiver = FindJumpReceiverFromTransform(playerTransform);
        if (_resolvedJumpReceiver == null)
        {
            _resolvedJumpReceiver = FindAnyJumpReceiverInScene();
        }

        if (playerTransform == null && _resolvedJumpReceiver is Component receiverComponent)
        {
            playerTransform = receiverComponent.transform;
        }
    }

    private static IExplosionJumpReceiver FindJumpReceiverFromTransform(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            return null;
        }

        MonoBehaviour[] localBehaviours = targetTransform.GetComponents<MonoBehaviour>();
        for (int i = 0; i < localBehaviours.Length; i++)
        {
            if (localBehaviours[i] is IExplosionJumpReceiver localReceiver)
            {
                return localReceiver;
            }
        }

        MonoBehaviour[] childBehaviours = targetTransform.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < childBehaviours.Length; i++)
        {
            if (childBehaviours[i] is IExplosionJumpReceiver childReceiver)
            {
                return childReceiver;
            }
        }

        MonoBehaviour[] parentBehaviours = targetTransform.GetComponentsInParent<MonoBehaviour>(true);
        for (int i = 0; i < parentBehaviours.Length; i++)
        {
            if (parentBehaviours[i] is IExplosionJumpReceiver parentReceiver)
            {
                return parentReceiver;
            }
        }

        return null;
    }

    private static IExplosionJumpReceiver FindAnyJumpReceiverInScene()
    {
        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IExplosionJumpReceiver receiver)
            {
                return receiver;
            }
        }

        return null;
    }

    private bool TryFindPlayerByTag(string tag, out GameObject playerObject)
    {
        try
        {
            playerObject = GameObject.FindGameObjectWithTag(tag);
            return playerObject != null;
        }
        catch (UnityException)
        {
            playerObject = null;

            if (!_hasWarnedMissingPlayerTag)
            {
                Debug.LogWarning($"Rocket jump player tag '{tag}' is not defined. Falling back to receiver lookup.", this);
                _hasWarnedMissingPlayerTag = true;
            }

            return false;
        }
    }

    private void PlayExplosionSound()
    {
        if (explosionHitSound == null)
        {
            return;
        }

        MetarrowAudioManager audioManager = MetarrowAudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.PlaySound(explosionHitSound, explosionVolumeScale);
        }
    }
}
