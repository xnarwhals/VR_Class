using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatRoam : MonoBehaviour
{
    public enum CatState { Roam, Idle, Recover }

    [Header("References")]
    public UnityEngine.AI.NavMeshAgent agent;

    [Header("Roam Points")]
    public List<RoamPoint> roamPoints = new();


    [Header("Timing")]
    public Vector2 roamDurationRange = new Vector2(4f, 10f);
    public Vector2 idleDurationRange = new Vector2(1f, 4f);


    [Header("Roam Rules")]
    public float minMoveDistance = 1.0f;        // don't pick super close targets
    public int lastPointsMemory = 2;            // avoid immediate repeats
    public float arrivalThreshold = 0.35f;      // what counts as "arrived" at a point


    [Header("Stuck Detection")]
    public float stuckCheckInterval = 1.0f;
    public float stuckMinProgress = 0.05f;      // meters per interval
    public float maxTimeWithoutProgress = 3.0f;

    [Header("Speed Variation")]
    public float baseSpeed = 1.3f;
    public float speedJitter = 0.4f;
    [Range(0f, 1f)] public float sprintChance = 0.08f;
    public float sprintMultiplier = 1.8f;

    private CatState state = CatState.Idle;
    private Queue<int> recentPointIndices = new();
    private float stateTimer;

    // stuck detection
    private float stuckTimer;
    private Vector3 lastPos;
    private float lastStuckCheckTime;

    [Serializable]
    public class RoamPoint
    {
        public Transform transform;
        [Range(0.1f, 10f)] public float weight = 1f;
    }

    void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        lastPos = transform.position;
        agent.autoBraking = true;
    }

    void Start()
    {
        EnterIdle();
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case CatState.Idle:
                if (stateTimer <= 0f) EnterRoam();
                break;

            case CatState.Roam:
                CheckStuck();

                if (HasArrived() || stateTimer <= 0f)
                    EnterIdle();
                break;

            case CatState.Recover:
                // After a short recover, try roaming again
                if (stateTimer <= 0f) EnterRoam();
                break;
        }
    }

    void EnterIdle()
    {
        state = CatState.Idle;
        stateTimer = Random.Range(idleDurationRange.x, idleDurationRange.y);

        if (agent.enabled)
        {
            agent.ResetPath();
        }
    }


    void EnterRoam()
    {
        state = CatState.Roam;
        stateTimer = Random.Range(roamDurationRange.x, roamDurationRange.y);

        // speed variation
        float speed = baseSpeed + Random.Range(-speedJitter, speedJitter);
        if (Random.value < sprintChance) speed *= sprintMultiplier;
        agent.speed = Mathf.Max(0.1f, speed);

        Vector3 target = ChooseRoamTarget();
        agent.SetDestination(target);

        // reset stuck tracking
        stuckTimer = 0f;
        lastPos = transform.position;
        lastStuckCheckTime = Time.time;
    }

    void EnterRecover()
    {
        state = CatState.Recover;
        stateTimer = 0.6f + Random.Range(0f, 0.6f);

        if (agent.enabled)
        {
            agent.ResetPath();
        }
    }

    bool HasArrived()
    {
        if (!agent.enabled) return true;
        if (agent.pathPending) return false;

        // agent.remainingDistance can be unreliable if no path
        if (agent.hasPath && agent.remainingDistance <= Mathf.Max(arrivalThreshold, agent.stoppingDistance + 0.05f))
            return true;

        // fallback distance check
        return agent.hasPath && Vector3.Distance(transform.position, agent.destination) <= arrivalThreshold;
    }

    Vector3 ChooseRoamTarget()
    {
        if (roamPoints == null || roamPoints.Count == 0)
        {
            // fallback: pick a random point on navmesh near current position
            return RandomNavmeshPoint(transform.position, 3f);
        }

        int chosenIndex = WeightedPickIndexAvoidingRecent();
        if (chosenIndex >= 0)
        {
            RememberIndex(chosenIndex);

            Vector3 p = roamPoints[chosenIndex].transform.position;

            // Ensure it's on NavMesh (in case the point is slightly off)
            if (NavMesh.SamplePosition(p, out var hit, 1.0f, NavMesh.AllAreas))
                p = hit.position;

            // If too close, pick a nearby navmesh point instead
            if (Vector3.Distance(transform.position, p) < minMoveDistance)
                p = RandomNavmeshPoint(transform.position, minMoveDistance * 2f);

            return p;
        }

        return RandomNavmeshPoint(transform.position, 3f);
    }

    int WeightedPickIndexAvoidingRecent()
    {
        // Build candidates
        List<int> candidates = new();
        float totalWeight = 0f;

        for (int i = 0; i < roamPoints.Count; i++)
        {
            if (!roamPoints[i].transform) continue;
            if (recentPointIndices.Contains(i)) continue;

            float w = Mathf.Max(0.001f, roamPoints[i].weight);
            candidates.Add(i);
            totalWeight += w;
        }

        // If everything is "recent", allow all
        if (candidates.Count == 0)
        {
            for (int i = 0; i < roamPoints.Count; i++)
            {
                if (!roamPoints[i].transform) continue;
                float w = Mathf.Max(0.001f, roamPoints[i].weight);
                candidates.Add(i);
                totalWeight += w;
            }
        }

        float r = Random.value * totalWeight;
        float running = 0f;

        foreach (int idx in candidates)
        {
            running += Mathf.Max(0.001f, roamPoints[idx].weight);
            if (r <= running) return idx;
        }

        return candidates.Count > 0 ? candidates[candidates.Count - 1] : -1;
    }

    void RememberIndex(int idx)
    {
        recentPointIndices.Enqueue(idx);
        while (recentPointIndices.Count > lastPointsMemory)
            recentPointIndices.Dequeue();
    }

    Vector3 RandomNavmeshPoint(Vector3 center, float radius)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 random = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(random, out var hit, 1.5f, NavMesh.AllAreas))
                return hit.position;
        }
        return center;
    }


    void CheckStuck()
    {
        if (Time.time - lastStuckCheckTime < stuckCheckInterval) return;
        lastStuckCheckTime = Time.time;

        float moved = Vector3.Distance(transform.position, lastPos);
        lastPos = transform.position;

        if (agent.pathPending) return;

        // stuck if agent wants to move but isn't making progress
        bool wantsToMove = agent.hasPath && agent.remainingDistance > arrivalThreshold + 0.2f;
        if (!wantsToMove) { stuckTimer = 0f; return; }

        if (moved < stuckMinProgress)
            stuckTimer += stuckCheckInterval;
        else
            stuckTimer = 0f;

        if (stuckTimer >= maxTimeWithoutProgress)
        {
            EnterRecover();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        foreach (var rp in roamPoints)
        {
            if (rp != null && rp.transform != null)
                Gizmos.DrawWireSphere(rp.transform.position, 0.15f);
        }

        if (agent)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(agent.destination, 0.12f);
        }
    }

}
