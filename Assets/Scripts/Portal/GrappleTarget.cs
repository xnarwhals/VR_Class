using UnityEngine;
using System.Collections;

public class GrappleTarget : BaseTarget
{
    [Header("Grapple Settings")]
    [SerializeField] private Transform grapplePoint;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float launchSpeed = 14f;
    [SerializeField] private float characterControllerPullDuration = 0.25f;
    [SerializeField] private bool debugLogs = false;

    private Coroutine _pullRoutine;

    protected override bool CanBeHit(Arrow arrow, RaycastHit hit)
    {
        return base.CanBeHit(arrow, hit) && arrow is GrappleArrow;
    }

    protected override void OnArrowHit(Arrow arrow, RaycastHit hit)
    {
        ResolvePlayerTransform();
        if (playerTransform == null)
        {
            if (debugLogs)
            {
                Debug.LogWarning("GrappleTarget: playerTransform is not assigned and no player was found by tag.", this);
            }

            return;
        }

        Vector3 targetPoint = grapplePoint != null ? grapplePoint.position : transform.position;
        Vector3 launchDirection = (targetPoint - playerTransform.position).normalized;
        if (launchDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Rigidbody playerRb = playerTransform.GetComponentInParent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = launchDirection * launchSpeed;
            return;
        }

        CharacterController controller = playerTransform.GetComponentInParent<CharacterController>();
        if (controller != null)
        {
            if (_pullRoutine != null)
            {
                StopCoroutine(_pullRoutine);
            }

            _pullRoutine = StartCoroutine(PullCharacterController(controller, launchDirection));
            return;
        }

        if (debugLogs)
        {
            Debug.LogWarning("GrappleTarget: no Rigidbody or CharacterController found on player hierarchy.", this);
        }
    }

    private IEnumerator PullCharacterController(CharacterController controller, Vector3 direction)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, characterControllerPullDuration);

        while (elapsed < duration)
        {
            if (controller == null)
            {
                yield break;
            }

            controller.Move(direction * (launchSpeed * Time.deltaTime));
            elapsed += Time.deltaTime;
            yield return null;
        }

        _pullRoutine = null;
    }

    private void ResolvePlayerTransform()
    {
        if (playerTransform != null)
        {
            return;
        }

        if (string.IsNullOrEmpty(playerTag))
        {
            return;
        }

        GameObject playerObject = GameObject.FindWithTag(playerTag);
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }
}
