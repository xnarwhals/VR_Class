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

    private IExplosionJumpReceiver _resolvedJumpReceiver;

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

        Vector3 playerOffset = playerTransform.position - explosionPoint;
        if (playerOffset.y < minHeightBelowPlayer)
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

    private void ResolveRocketJumpReferences()
    {
        if (playerTransform == null && !string.IsNullOrEmpty(playerTag))
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }

        if (rocketJumpReceiver != null)
        {
            _resolvedJumpReceiver = rocketJumpReceiver as IExplosionJumpReceiver;
            if (_resolvedJumpReceiver == null)
            {
                Debug.LogWarning(
                    $"Assigned rocket jump receiver on {name} does not implement IExplosionJumpReceiver.",
                    rocketJumpReceiver);
            }
            return;
        }

        _resolvedJumpReceiver = FindJumpReceiverFromTransform(playerTransform);
    }

    private static IExplosionJumpReceiver FindJumpReceiverFromTransform(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            return null;
        }

        MonoBehaviour[] candidateBehaviours = targetTransform.GetComponentsInParent<MonoBehaviour>(true);
        for (int i = 0; i < candidateBehaviours.Length; i++)
        {
            if (candidateBehaviours[i] is IExplosionJumpReceiver receiver)
            {
                return receiver;
            }
        }

        return null;
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
