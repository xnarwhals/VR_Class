using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrappleTarget : BaseTarget
{
    [Header("Grapple Settings")]
    [SerializeField] private Transform grapplePoint;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool useArrowArcTrajectory = true;
    [SerializeField] private float launchDurationMultiplier = 1f;
    [SerializeField] private float minLaunchDuration = 0.2f;
    [SerializeField] private float maxLaunchDuration = 2.5f;
    [SerializeField] private float fallbackLaunchForce = 8f;
    [SerializeField] private float fallbackUpwardForceMultiplier = 1.25f;
    [SerializeField] private float fallbackCharacterControllerLaunchDistance = 3.5f;
    [SerializeField] private float lineVisibleDuration = 0.12f;
    [SerializeField] private LineRenderer grappleLine;
    [SerializeField] private bool debugLogs = false;

    private bool _lineVisible;
    private float _lineHideTime;
    private Coroutine _characterControllerLaunchRoutine;

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
        if (!TryLaunchPlayer(arrow, hit, targetPoint))
        {
            return;
        }

        ShowLine(targetPoint, GetLaunchOriginPosition());
    }

    protected override void Awake()
    {
        base.Awake();

        if (grappleLine == null)
        {
            grappleLine = GetComponent<LineRenderer>();
        }

        if (grappleLine != null)
        {
            grappleLine.enabled = false;
        }
    }

    private void OnDisable()
    {
        if (_characterControllerLaunchRoutine != null)
        {
            StopCoroutine(_characterControllerLaunchRoutine);
            _characterControllerLaunchRoutine = null;
        }

        DisableLine();
    }

    private void Update()
    {
        if (!_lineVisible)
        {
            return;
        }

        if (Time.time >= _lineHideTime)
        {
            DisableLine();
        }
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

    private bool TryLaunchPlayer(Arrow arrow, RaycastHit hit, Vector3 targetPoint)
    {
        if (useArrowArcTrajectory && TryLaunchPlayerAlongArrowArc(arrow, hit, targetPoint))
        {
            return true;
        }

        return TryLaunchPlayerFallback(targetPoint);
    }

    private bool TryLaunchPlayerAlongArrowArc(Arrow arrow, RaycastHit hit, Vector3 targetPoint)
    {
        if (arrow == null || !arrow.HasLaunchPosition)
        {
            return false;
        }

        IReadOnlyList<Vector3> arrowSamples = arrow.FlightSamples;
        if (arrowSamples == null || arrowSamples.Count < 2)
        {
            return false;
        }

        Rigidbody playerBody = playerTransform.GetComponentInParent<Rigidbody>();
        CharacterController controller = playerTransform.GetComponentInParent<CharacterController>();
        Transform launchRoot = ResolveLaunchRoot(playerBody, controller);
        if (launchRoot == null)
        {
            return false;
        }

        Vector3 launchStart = launchRoot.position;
        if ((targetPoint - launchStart).sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        List<Vector3> mappedPath = BuildMappedArcPath(arrow.LaunchPosition, hit.point, arrowSamples, launchStart, targetPoint);
        if (mappedPath.Count < 2)
        {
            return false;
        }

        float sourceDuration = arrow.FlightDuration > 0f ? arrow.FlightDuration : Time.fixedDeltaTime * (arrowSamples.Count - 1);
        float duration = Mathf.Clamp(sourceDuration * Mathf.Max(0.01f, launchDurationMultiplier), minLaunchDuration, maxLaunchDuration);

        if (controller != null)
        {
            if (_characterControllerLaunchRoutine != null)
            {
                StopCoroutine(_characterControllerLaunchRoutine);
            }

            _characterControllerLaunchRoutine = StartCoroutine(LaunchCharacterControllerAlongPath(controller, launchRoot, mappedPath, duration));
            return true;
        }

        if (playerBody != null)
        {
            float firstStepDuration = Mathf.Max(0.01f, duration / (mappedPath.Count - 1));
            Vector3 launchVelocity = (mappedPath[1] - mappedPath[0]) / firstStepDuration;
            playerBody.linearVelocity = launchVelocity;
            return true;
        }

        return false;
    }

    private List<Vector3> BuildMappedArcPath(
        Vector3 sourceStart,
        Vector3 sourceEnd,
        IReadOnlyList<Vector3> sourceSamples,
        Vector3 destinationStart,
        Vector3 destinationEnd)
    {
        List<Vector3> mappedPath = new List<Vector3>(sourceSamples.Count);
        int sampleCount = sourceSamples.Count;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = sampleCount == 1 ? 1f : i / (sampleCount - 1f);
            Vector3 sourceLinePoint = Vector3.Lerp(sourceStart, sourceEnd, t);
            Vector3 destinationLinePoint = Vector3.Lerp(destinationStart, destinationEnd, t);
            Vector3 arcOffset = sourceSamples[i] - sourceLinePoint;

            mappedPath.Add(destinationLinePoint + arcOffset);
        }

        mappedPath[0] = destinationStart;
        mappedPath[mappedPath.Count - 1] = destinationEnd;
        return mappedPath;
    }

    private IEnumerator LaunchCharacterControllerAlongPath(
        CharacterController controller,
        Transform launchRoot,
        List<Vector3> pathPoints,
        float duration)
    {
        if (controller == null || launchRoot == null || pathPoints == null || pathPoints.Count < 2)
        {
            yield break;
        }

        float safeDuration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;
        int segmentCount = pathPoints.Count - 1;

        while (elapsed < safeDuration)
        {
            float normalizedTime = Mathf.Clamp01(elapsed / safeDuration);
            float scaledSegment = normalizedTime * segmentCount;
            int segmentIndex = Mathf.Min(segmentCount - 1, Mathf.FloorToInt(scaledSegment));
            float segmentT = scaledSegment - segmentIndex;

            Vector3 targetPosition = Vector3.Lerp(pathPoints[segmentIndex], pathPoints[segmentIndex + 1], segmentT);
            Vector3 delta = targetPosition - launchRoot.position;
            controller.Move(delta);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 finalDelta = pathPoints[pathPoints.Count - 1] - launchRoot.position;
        controller.Move(finalDelta);
        _characterControllerLaunchRoutine = null;
    }

    private bool TryLaunchPlayerFallback(Vector3 targetPoint)
    {
        Vector3 launchOrigin = GetLaunchOriginPosition();
        Vector3 toTarget = targetPoint - launchOrigin;
        if (toTarget.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        Vector3 direction = toTarget.normalized;
        if (direction.y > 0f)
        {
            direction.y *= Mathf.Max(1f, fallbackUpwardForceMultiplier);
            direction.Normalize();
        }

        Rigidbody playerBody = playerTransform.GetComponentInParent<Rigidbody>();
        if (playerBody != null)
        {
            playerBody.AddForce(direction * Mathf.Max(0f, fallbackLaunchForce), ForceMode.VelocityChange);
            return true;
        }

        CharacterController controller = playerTransform.GetComponentInParent<CharacterController>();
        if (controller != null)
        {
            Vector3 displacement = direction * Mathf.Max(0f, fallbackCharacterControllerLaunchDistance);
            controller.Move(displacement);
            return true;
        }

        if (debugLogs)
        {
            Debug.LogWarning("GrappleTarget: no Rigidbody or CharacterController found on player hierarchy.", this);
        }

        return false;
    }

    private Transform ResolveLaunchRoot(Rigidbody playerBody, CharacterController controller)
    {
        if (playerBody != null)
        {
            return playerBody.transform;
        }

        if (controller != null)
        {
            return controller.transform;
        }

        return playerTransform;
    }

    private Vector3 GetLaunchOriginPosition()
    {
        Rigidbody playerBody = playerTransform != null ? playerTransform.GetComponentInParent<Rigidbody>() : null;
        if (playerBody != null)
        {
            return playerBody.position;
        }

        CharacterController controller = playerTransform != null ? playerTransform.GetComponentInParent<CharacterController>() : null;
        if (controller != null)
        {
            return controller.transform.position;
        }

        return playerTransform != null ? playerTransform.position : transform.position;
    }

    private void ShowLine(Vector3 targetPoint, Vector3 playerPoint)
    {
        if (grappleLine == null)
        {
            return;
        }

        grappleLine.enabled = true;
        grappleLine.positionCount = 2;
        grappleLine.SetPosition(0, targetPoint);
        grappleLine.SetPosition(1, playerPoint);
        _lineVisible = true;
        _lineHideTime = Time.time + Mathf.Max(0.01f, lineVisibleDuration);
    }

    private void DisableLine()
    {
        _lineVisible = false;

        if (grappleLine != null)
        {
            grappleLine.enabled = false;
        }
    }
}
