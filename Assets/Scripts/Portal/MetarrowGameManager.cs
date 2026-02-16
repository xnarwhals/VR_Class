using UnityEngine;
using System;
using System.Collections.Generic;

public class MetarrowGameManager : MonoBehaviour
{
    public static MetarrowGameManager Instance { get; private set; }
    public event Action levelCompleted;
    [SerializeField] private int totalTargets = 11;
    [SerializeField] private int currentLevel = 1;

    private int arrowsFired = 0;
    private int successfulHits = 0;
    private readonly HashSet<BaseTarget> countedTargets = new HashSet<BaseTarget>();

    public int ArrowsFired => arrowsFired;
    public int SuccessfulHits => successfulHits;
    public int UniqueTargetsHit => countedTargets.Count;

    public void TargetHit()
    {
        // Legacy support for old UnityEvent hookups.
        successfulHits++;
        EvaluateLevelCompletion();
    }

    public void RegisterTargetHit(BaseTarget target)
    {
        // Every successful target hit affects accuracy.
        successfulHits++;

        // Unique target progress only increments once per target instance.
        if (target != null)
        {
            countedTargets.Add(target);
        }

        EvaluateLevelCompletion();
    }

    public void RegisterArrowFired()
    {
        arrowsFired++;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void EvaluateLevelCompletion()
    {

        bool allTargetsHit = CheckAllTargetsHit();
        if (allTargetsHit)
        {
            levelCompleted?.Invoke();
        }
    }

    private bool CheckAllTargetsHit()
    {
        return countedTargets.Count >= totalTargets;
    }

    public float GetAccuracy()
    {
        if (arrowsFired <= 0)
        {
            return 0f;
        }

        return (float)successfulHits / arrowsFired;
    }

    public float GetAccuracyPercent()
    {
        return GetAccuracy() * 100f;
    }

    public void ResetStats()
    {
        arrowsFired = 0;
        successfulHits = 0;
        countedTargets.Clear();
    }
}
