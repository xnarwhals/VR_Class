
using UnityEngine;


public class FlipTarget : BaseTarget
{
    [Header("Flip Rules")]
    [SerializeField] private float minimumLaunchDistance = 2.0f;

    [Header("Flip Physics")]
    [SerializeField] private Rigidbody targetBody;
    [SerializeField] private HingeJoint hingeJoint;
    [SerializeField] private float hitPushForce = 5f;
    [SerializeField] private ForceMode hitForceMode = ForceMode.Impulse;
    [SerializeField] private bool resetAngularVelocityOnHit = true;

    private bool _isTargetEnabled = true;

    private void Awake()
    {
        if (targetBody == null)
        {
            targetBody = GetComponent<Rigidbody>();
        }

        if (hingeJoint == null)
        {
            hingeJoint = GetComponent<HingeJoint>();
        }
    }

    public void DisableTarget()
    {
        _isTargetEnabled = false;
    }

    public void EnableTarget()
    {
        _isTargetEnabled = true;
    }

    protected override bool CanBeHit(Arrow arrow, RaycastHit hit)
    {
        if (!base.CanBeHit(arrow, hit) || !_isTargetEnabled)
        {
            return false;
        }

        if (arrow == null || !arrow.HasLaunchPosition)
        {
            return false;
        }

        float launchDistance = Vector3.Distance(arrow.LaunchPosition, hit.point);
        return launchDistance >= minimumLaunchDistance;
    }

    protected override void OnArrowHit(Arrow arrow, RaycastHit hit)
    {
        base.OnArrowHit(arrow, hit);

        if (targetBody == null || hingeJoint == null)
        {
            return;
        }

        if (resetAngularVelocityOnHit)
        {
            targetBody.angularVelocity = Vector3.zero;
        }

        // Push from the arrow direction at impact point so the hinged body flips backward.
        Vector3 pushDirection = arrow != null ? arrow.transform.forward : -hit.normal;
        targetBody.AddForceAtPosition(pushDirection * hitPushForce, hit.point, hitForceMode);
    }
}
