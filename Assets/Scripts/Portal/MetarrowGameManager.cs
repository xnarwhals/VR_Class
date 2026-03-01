using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class MetarrowGameManager : MonoBehaviour
{
    public static MetarrowGameManager Instance { get; private set; }
    public event Action levelCompleted;
    public UnityEvent onLevelCompleted;
    [SerializeField] private int totalTargets = 11;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private bool levelGatchaPuzzleCompleted = false;

    private int arrowsFired = 0;
    private int successfulHits = 0;
    private readonly HashSet<BaseTarget> countedTargets = new HashSet<BaseTarget>();
    private int activeLevelTargetCount;
    private float levelStartTime;
    private float levelCompletedTime = -1f;
    private bool hasLevelCompleted;

    public int ArrowsFired => arrowsFired;
    public int SuccessfulHits => successfulHits;
    public int UniqueTargetsHit => countedTargets.Count;
    public int CurrentLevel => currentLevel;
    public int ActiveLevelTargetCount => activeLevelTargetCount;
    public bool HasLevelCompleted => hasLevelCompleted;
    public float ElapsedLevelTime => (hasLevelCompleted ? levelCompletedTime : Time.time) - levelStartTime;

    public void TargetHit()
    {
        // Legacy support for old UnityEvent hookups.
        successfulHits++;
        EvaluateLevelCompletion();
    }

    public void RegisterTargetHit(BaseTarget target, bool countTowardsAccuracy = true)
    {
        if (countTowardsAccuracy)
        {
            // Every successful target hit affects accuracy unless explicitly excluded.
            successfulHits++;
        }

        // Unique target progress only increments once per target instance.
        if (target != null && target.ContributesToUniqueTargetWinCondition)
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
        activeLevelTargetCount = Mathf.Max(0, totalTargets);
        levelStartTime = Time.time;
    }

    private void EvaluateLevelCompletion()
    {
        if (hasLevelCompleted)
        {
            return;
        }

        bool allTargetsHit = CheckAllTargetsHit();
        if (allTargetsHit && levelGatchaPuzzleCompleted)
        {
            hasLevelCompleted = true;
            levelCompletedTime = Time.time;
            levelCompleted?.Invoke();
            onLevelCompleted?.Invoke();
        }
    }

    private bool CheckAllTargetsHit()
    {
        return countedTargets.Count >= activeLevelTargetCount;
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
        ResetCurrentLevelStats(resetPerformanceStats: true);
    }

    public void SetLevelGatchaPuzzleCompleted(bool completed)
    {
        levelGatchaPuzzleCompleted = completed;
        EvaluateLevelCompletion();
    }

    public void BeginLevel(int levelIndex, int targetCount, bool resetPerformanceStats = true)
    {
        currentLevel = levelIndex;
        totalTargets = Mathf.Max(0, targetCount); // Kept in sync for inspector/debug visibility.
        activeLevelTargetCount = totalTargets;
        ResetCurrentLevelStats(resetPerformanceStats);
    }

    public void SetActiveLevelTargetCount(int targetCount)
    {
        totalTargets = Mathf.Max(0, targetCount); // Kept in sync for inspector/debug visibility.
        activeLevelTargetCount = totalTargets;
        EvaluateLevelCompletion();
    }

    private void ResetCurrentLevelStats(bool resetPerformanceStats)
    {
        if (resetPerformanceStats)
        {
            arrowsFired = 0;
            successfulHits = 0;
        }

        countedTargets.Clear();
        levelGatchaPuzzleCompleted = false;
        levelStartTime = Time.time;
        levelCompletedTime = -1f;
        hasLevelCompleted = false;
    }
}
