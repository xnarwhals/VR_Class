using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BattleCatMovement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Transform roamCenter;
    [SerializeField] private float roamRadius = 4f;
    [SerializeField] private int maxSampleAttempts = 8;
    [SerializeField] private float arriveDistance = 0.25f;
    [SerializeField] private float maxMoveDuration = 6f;
    [SerializeField] private bool debugMovement = false;

    private bool _warnedMissingAgent;
    private bool _warnedAgentDisabled;
    private bool _warnedNotOnNavMesh;
    private bool _warnedSamplingFailure;

    private void Awake()
    {
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        if (roamCenter == null)
        {
            roamCenter = transform;
        }
    }

    public IEnumerator MoveToRandomPoint(float moveSpeed)
    {
        if (!CanMove(out string reason))
        {
            if (debugMovement)
            {
                Debug.LogWarning($"BattleCatMovement skipped move: {reason}", this);
            }

            yield break;
        }

        if (!TryGetRandomNavMeshPoint(out Vector3 destination))
        {
            if (debugMovement)
            {
                Vector3 centerPosition = roamCenter != null ? roamCenter.position : transform.position;
                Debug.LogWarning(
                    $"BattleCatMovement failed to sample destination. center={centerPosition}, radius={roamRadius}, attempts={maxSampleAttempts}",
                    this);
            }

            yield break;
        }

        navMeshAgent.speed = moveSpeed;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(destination);

        float elapsed = 0f;
        float stopDistance = Mathf.Max(arriveDistance, navMeshAgent.stoppingDistance);

        while (elapsed < maxMoveDuration)
        {
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= stopDistance)
            {
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        navMeshAgent.isStopped = true;
    }

    public void StopMovement()
    {
        if (navMeshAgent == null)
        {
            return;
        }

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }

    private bool CanMove(out string reason)
    {
        reason = null;

        if (navMeshAgent == null)
        {
            reason = "NavMeshAgent reference is missing.";
            if (!_warnedMissingAgent)
            {
                Debug.LogWarning("BattleCatMovement cannot move because NavMeshAgent is missing.", this);
                _warnedMissingAgent = true;
            }

            return false;
        }

        if (!navMeshAgent.enabled)
        {
            reason = "NavMeshAgent is disabled.";
            if (!_warnedAgentDisabled)
            {
                Debug.LogWarning("BattleCatMovement cannot move because NavMeshAgent is disabled.", this);
                _warnedAgentDisabled = true;
            }

            return false;
        }

        if (!navMeshAgent.isOnNavMesh)
        {
            reason = $"Agent is not on NavMesh. position={transform.position}";
            if (!_warnedNotOnNavMesh)
            {
                Debug.LogWarning($"BattleCatMovement agent is not on NavMesh at position {transform.position}.", this);
                _warnedNotOnNavMesh = true;
            }

            return false;
        }

        return true;
    }

    private bool TryGetRandomNavMeshPoint(out Vector3 point)
    {
        point = transform.position;

        if (roamCenter == null)
        {
            return false;
        }

        Vector3 center = roamCenter.position;
        float sampleDistance = Mathf.Max(1f, roamRadius);

        for (int i = 0; i < maxSampleAttempts; i++)
        {
            Vector2 random2D = Random.insideUnitCircle * roamRadius;
            Vector3 candidate = center + new Vector3(random2D.x, 0f, random2D.y);
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
            {
                continue;
            }

            if ((hit.position - transform.position).sqrMagnitude < 0.04f)
            {
                continue;
            }

            point = hit.position;
            return true;
        }

        if (!_warnedSamplingFailure)
        {
            Debug.LogWarning(
                $"BattleCatMovement could not sample a NavMesh point after {maxSampleAttempts} attempts. center={center}, radius={roamRadius}, sampleDistance={sampleDistance}",
                this);
            _warnedSamplingFailure = true;
        }

        return false;
    }
}
