using UnityEngine;

public class ArrowTrajectoryIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PullInteraction pullInteraction;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Trajectory")]
    [SerializeField] private float arrowSpeed = 6.0f;
    [SerializeField] private float fallbackArrowMass = 1.0f;
    [SerializeField] private int segmentCount = 30;
    [SerializeField] private float timeStep = 0f;
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private int ignoredCollisionLayer = 8;
    [SerializeField] private float minPullToShow = 0.05f;

    private void Awake()
    {
        if (lineRenderer == null) {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (lineRenderer != null) {
            lineRenderer.enabled = false;
        }
    }

    private void OnEnable()
    {
        PullInteraction.pullActionReleased += HideTrajectory;
    }

    private void OnDisable()
    {
        PullInteraction.pullActionReleased -= HideTrajectory;
    }

    private void Update()
    {
        if (pullInteraction == null || launchPoint == null || lineRenderer == null) {
            return;
        }

        float pullAmount = pullInteraction.pullAmmount;
        if (!pullInteraction.isSelected || pullAmount < minPullToShow) {
            HideTrajectory(0f);
            return;
        }

        DrawTrajectory(pullAmount);
    }

    private void DrawTrajectory(float pullAmount)
    {
        lineRenderer.enabled = true;

        ResolveLaunchState(
            pullAmount,
            out Vector3 startPosition,
            out Vector3 initialVelocity);

        Vector3 gravity = Physics.gravity;
        float dt = timeStep > 0f ? timeStep : Time.fixedDeltaTime;
        dt = Mathf.Max(0.001f, dt);

        int writtenPoints = 1;
        lineRenderer.positionCount = segmentCount;
        lineRenderer.SetPosition(0, startPosition);

        Vector3 previousPosition = startPosition;

        for (int i = 1; i < segmentCount; i++)
        {
            float t = i * dt;
            Vector3 nextPosition = startPosition + (initialVelocity * t) + (0.5f * gravity * t * t);

            if (Physics.Linecast(previousPosition, nextPosition, out RaycastHit hit, collisionMask, QueryTriggerInteraction.UseGlobal))
            {
                if (hit.transform.gameObject.layer != ignoredCollisionLayer)
                {
                    lineRenderer.SetPosition(i, hit.point);
                    writtenPoints = i + 1;
                    break;
                }
            }

            lineRenderer.SetPosition(i, nextPosition);
            writtenPoints = i + 1;
            previousPosition = nextPosition;
        }

        lineRenderer.positionCount = writtenPoints;
    }

    private void ResolveLaunchState(float pullAmount, out Vector3 position, out Vector3 velocity)
    {
        position = launchPoint.position;
        Vector3 direction = launchPoint.forward;
        float speed = arrowSpeed;
        float mass = Mathf.Max(0.001f, fallbackArrowMass);

        Arrow notchedArrow = launchPoint.GetComponentInChildren<Arrow>();
        if (notchedArrow != null)
        {
            speed = notchedArrow.speed;
            direction = notchedArrow.transform.forward;

            if (notchedArrow.tip != null)
            {
                position = notchedArrow.tip.position;
            }

            Rigidbody arrowRb = notchedArrow.GetComponent<Rigidbody>();
            if (arrowRb != null)
            {
                mass = Mathf.Max(0.001f, arrowRb.mass);
                velocity = arrowRb.linearVelocity + (direction * (pullAmount * speed / mass));
                return;
            }
        }

        velocity = direction * (pullAmount * speed / mass);
    }

    private void HideTrajectory(float _)
    {
        if (lineRenderer == null) {
            return;
        }

        lineRenderer.enabled = false;
        lineRenderer.positionCount = 0;
    }
}
