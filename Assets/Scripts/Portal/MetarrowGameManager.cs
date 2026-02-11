using UnityEngine;
using System;

public class MetarrowGameManager : MonoBehaviour
{
    public static MetarrowGameManager Instance { get; private set; }
    public event Action levelCompleted;
    [SerializeField] private int totalTargets = 3;
    private int targetsHit = 0;

    public void TargetHit()
    {
        targetsHit++;
        EvaluateLevelCompletion();
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
        return targetsHit >= totalTargets;
    }
}
