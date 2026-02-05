using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class MonsterRoam : MonoBehaviour
{
    private enum MonsterState { Idle, Roam }

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    [Header("Roam Points")]
    [SerializeField] private List<Transform> roamPoints = new();

    [Header("Timing")]
    [SerializeField] private Vector2 roamDurationRange = new Vector2(4f, 10f);
    [SerializeField] private Vector2 idleDurationRange = new Vector2(1f, 2.5f);

    [Header("Roam Rules")]
    [SerializeField] private float minMoveDistance = 1.0f;
    [SerializeField] private float arrivalThreshold = 0.4f;

    [Header("Animation Params")]
    [SerializeField] private string walkBoolName = "IsWalking";

    private MonsterState state = MonsterState.Idle;
    private float stateTimer;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        EnterIdle();
    }

    private void OnEnable()
    {
        // If re-enabled mid-game, restart the roam loop immediately.
        ForceStartRoam();
    }

    private void Update()
    {
        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case MonsterState.Idle:
                if (stateTimer <= 0f) EnterRoam();
                break;

            case MonsterState.Roam:
                if (HasArrived() || stateTimer <= 0f)
                    EnterIdle();
                break;
        }
    }

    private void EnterIdle()
    {
        state = MonsterState.Idle;
        stateTimer = Random.Range(idleDurationRange.x, idleDurationRange.y);

        if (agent && agent.enabled)
            agent.ResetPath();

        SetWalking(false);
    }

    private void EnterRoam()
    {
        state = MonsterState.Roam;
        stateTimer = Random.Range(roamDurationRange.x, roamDurationRange.y);

        if (agent && agent.enabled)
        {
            Vector3 target = ChooseRoamTarget();
            agent.SetDestination(target);
        }

        SetWalking(true);
    }

    private bool HasArrived()
    {
        if (!agent || !agent.enabled) return true;
        if (agent.pathPending) return false;

        if (agent.hasPath && agent.remainingDistance <= Mathf.Max(arrivalThreshold, agent.stoppingDistance + 0.05f))
            return true;

        return agent.hasPath && Vector3.Distance(transform.position, agent.destination) <= arrivalThreshold;
    }

    private Vector3 ChooseRoamTarget()
    {
        if (roamPoints == null || roamPoints.Count == 0)
            return RandomNavmeshPoint(transform.position, 3f);

        Transform point = roamPoints[Random.Range(0, roamPoints.Count)];
        Vector3 p = point ? point.position : transform.position;

        if (NavMesh.SamplePosition(p, out var hit, 1.0f, NavMesh.AllAreas))
            p = hit.position;

        if (Vector3.Distance(transform.position, p) < minMoveDistance)
            p = RandomNavmeshPoint(transform.position, minMoveDistance * 2f);

        return p;
    }

    private Vector3 RandomNavmeshPoint(Vector3 center, float radius)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 random = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(random, out var hit, 1.5f, NavMesh.AllAreas))
                return hit.position;
        }
        return center;
    }

    private void SetWalking(bool walking)
    {
        if (animator != null && !string.IsNullOrEmpty(walkBoolName))
            animator.SetBool(walkBoolName, walking);
    }

    public void ForceStartRoam()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        if (agent && agent.enabled)
        {
            agent.isStopped = false;
        }

        EnterRoam();
    }
}
