using UnityEngine;

public class ArrowTrajectoryIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PullInteraction pullInteraction;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Trajectory")]
    [SerializeField] private float arrowSpeed = 6.0f;
    [SerializeField] private int segmentCount = 30;
    [SerializeField] private float timeStep = 0.05f;
    [SerializeField] private LayerMask collisionMask = ~0;
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

        Vector3 position = launchPoint.position;
        Vector3 velocity = launchPoint.forward * (pullAmount * arrowSpeed);
        Vector3 gravity = Physics.gravity;

        int writtenPoints = 1;
        lineRenderer.positionCount = segmentCount;
        lineRenderer.SetPosition(0, position);

        for (int i = 1; i < segmentCount; i++)
        {
            Vector3 nextPosition = position + velocity * timeStep + 0.5f * gravity * (timeStep * timeStep);

            if (Physics.Linecast(position, nextPosition, out RaycastHit hit, collisionMask, QueryTriggerInteraction.Ignore))
            {
                lineRenderer.SetPosition(i, hit.point);
                writtenPoints = i + 1;
                break;
            }

            lineRenderer.SetPosition(i, nextPosition);
            writtenPoints = i + 1;
            velocity += gravity * timeStep;
            position = nextPosition;
        }

        lineRenderer.positionCount = writtenPoints;
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
