using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[Serializable]
public class LevelDefinition
{
    [Tooltip("Unique ID used by teleporters/events to start this level.")]
    [Min(1)] public int levelIndex = 1;

    [Tooltip("Editor-only label for readability.")]
    public string levelName = "New Level";

    [Tooltip("How many unique targets are required for level completion.")]
    [Min(0)] public int targetCount = 0;

    [Tooltip("If enabled, arrows/hits are reset when this level starts.")]
    public bool resetPerformanceStatsOnStart = true;

    [Tooltip("Invoked only when this specific level is completed.")]
    public UnityEvent onLevelCompleted;
}

public class MetarrowGameManager : MonoBehaviour
{
    // ----- Singleton + completion events -----
    public static MetarrowGameManager Instance { get; private set; }
    public event Action levelCompleted;
    public event Action<int> targetsLeftChanged;
    public UnityEvent onLevelCompleted;

    // ----- Inspector setup -----
    [Header("Level Configuration")]
    [SerializeField] private int startingLevelIndex = 1;
    [SerializeField] private List<LevelDefinition> levels = new List<LevelDefinition>();

    [Header("Runtime Debug")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int activeLevelTargetCount = 0;
    [SerializeField] private bool levelGatchaPuzzleCompleted = false;

    // ----- Runtime tracking -----
    private int arrowsFired = 0;
    private int successfulHits = 0;
    private readonly HashSet<BaseTarget> countedTargets = new HashSet<BaseTarget>();
    private readonly Dictionary<int, LevelDefinition> levelLookup = new Dictionary<int, LevelDefinition>();
    private LevelDefinition activeLevelDefinition;
    private float levelStartTime;
    private float levelCompletedTime = -1f;
    private bool hasLevelCompleted;
    private static bool isRestartingScene;

    // ----- Public read-only stats -----
    public int ArrowsFired => arrowsFired;
    public string ActiveLevelName => activeLevelDefinition != null ? activeLevelDefinition.levelName : $"Level {currentLevel}";
    public int SuccessfulHits => successfulHits;
    public int UniqueTargetsHit => countedTargets.Count;
    public int CurrentLevel => currentLevel;
    public int ActiveLevelTargetCount => activeLevelTargetCount;
    public int TargetsLeft => Mathf.Max(0, activeLevelTargetCount - countedTargets.Count);
    public bool HasLevelCompleted => hasLevelCompleted;
    public float ElapsedLevelTime => (hasLevelCompleted ? levelCompletedTime : Time.time) - levelStartTime;
    public IReadOnlyList<LevelDefinition> Levels => levels;

    // ----- Lifecycle -----
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        RebuildLevelLookup();
        InitializeStartingLevel();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnValidate()
    {
        RebuildLevelLookup();
    }

    // ----- Gameplay event entry points -----
    public void RegisterTargetHit(BaseTarget target, bool countTowardsAccuracy = true)
    {
        int targetsLeftBefore = TargetsLeft;

        if (countTowardsAccuracy)
        {
            successfulHits++;
        }

        if (target != null && target.ContributesToUniqueTargetWinCondition)
        {
            countedTargets.Add(target);
        }

        if (TargetsLeft != targetsLeftBefore)
        {
            NotifyTargetsLeftChanged();
        }

        EvaluateLevelCompletion();
    }

    public void RegisterArrowFired()
    {
        arrowsFired++;
    }

    public void SetLevelGatchaPuzzleCompleted(bool completed)
    {
        levelGatchaPuzzleCompleted = completed;
        EvaluateLevelCompletion();
    }

    // ----- Level loading -----
    // Primary API for teleporters/UnityEvents.
    public void BeginConfiguredLevel(int levelIndex)
    {
        BeginConfiguredLevel(levelIndex, resetPerformanceStatsOverride: null);
    }

    public void BeginConfiguredLevelPreserveStats(int levelIndex)
    {
        BeginConfiguredLevel(levelIndex, resetPerformanceStatsOverride: false);
    }

    public void BeginConfiguredLevelResetStats(int levelIndex)
    {
        BeginConfiguredLevel(levelIndex, resetPerformanceStatsOverride: true);
    }

    public void BeginConfiguredLevel(int levelIndex, bool? resetPerformanceStatsOverride)
    {
        if (!TryGetLevelDefinition(levelIndex, out LevelDefinition level))
        {
            Debug.LogWarning($"MetarrowGameManager: Level {levelIndex} not found in level definitions.");
            return;
        }

        bool shouldResetStats = resetPerformanceStatsOverride ?? level.resetPerformanceStatsOnStart;
        currentLevel = level.levelIndex;
        activeLevelDefinition = level;
        activeLevelTargetCount = Mathf.Max(0, level.targetCount);
        ResetCurrentLevelStats(shouldResetStats);
        NotifyTargetsLeftChanged();
    }

    public void ResetStats()
    {
        ResetCurrentLevelStats(resetPerformanceStats: true);
        NotifyTargetsLeftChanged();
    }

    // Hard reset entry point for UI/buttons/UnityEvents.
    public void RestartGame()
    {
        if (isRestartingScene)
        {
            return;
        }

        isRestartingScene = true;
        SceneManager.sceneLoaded += HandleRestartSceneLoaded;

        // Destroy persistent managers so the reloaded scene starts clean.
        MetarrowAudioManager audioManager = MetarrowAudioManager.Instance;
        if (audioManager != null)
        {
            Destroy(audioManager.gameObject);
        }

        MetarrowGameManager[] managers = FindObjectsOfType<MetarrowGameManager>();
        for (int i = 0; i < managers.Length; i++)
        {
            MetarrowGameManager manager = managers[i];
            if (manager != null)
            {
                Destroy(manager.gameObject);
            }
        }

        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex, LoadSceneMode.Single);
    }

    // ----- Scoring / completion -----
    private void EvaluateLevelCompletion()
    {
        if (hasLevelCompleted)
        {
            return;
        }

        bool allTargetsHit = countedTargets.Count >= activeLevelTargetCount;
        bool gatchaRequirementMet = levelGatchaPuzzleCompleted;

        if (allTargetsHit && gatchaRequirementMet)
        {
            hasLevelCompleted = true;
            levelCompletedTime = Time.time;
            activeLevelDefinition?.onLevelCompleted?.Invoke();
            levelCompleted?.Invoke();
            onLevelCompleted?.Invoke();
        }
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

    // ----- Internal helpers -----
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

    private void InitializeStartingLevel()
    {
        if (levels.Count == 0)
        {
            Debug.LogWarning("MetarrowGameManager: No levels configured.");
            activeLevelTargetCount = 0;
            levelStartTime = Time.time;
            NotifyTargetsLeftChanged();
            return;
        }

        if (!TryGetLevelDefinition(startingLevelIndex, out LevelDefinition startingLevel))
        {
            Debug.LogWarning($"MetarrowGameManager: Starting level {startingLevelIndex} not found. Falling back to first configured level.");
            startingLevel = levels[0];
        }

        currentLevel = Mathf.Max(1, startingLevel.levelIndex);
        activeLevelDefinition = startingLevel;
        activeLevelTargetCount = Mathf.Max(0, startingLevel.targetCount);
        ResetCurrentLevelStats(resetPerformanceStats: true);
        NotifyTargetsLeftChanged();
    }

    private void NotifyTargetsLeftChanged()
    {
        targetsLeftChanged?.Invoke(TargetsLeft);
    }

    private static void HandleRestartSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isRestartingScene = false;
        SceneManager.sceneLoaded -= HandleRestartSceneLoaded;
    }

    private bool TryGetLevelDefinition(int levelIndex, out LevelDefinition level)
    {
        if (levelLookup.Count == 0)
        {
            RebuildLevelLookup();
        }

        return levelLookup.TryGetValue(levelIndex, out level);
    }

    private void RebuildLevelLookup()
    {
        levelLookup.Clear();

        for (int i = 0; i < levels.Count; i++)
        {
            LevelDefinition level = levels[i];
            if (level == null)
            {
                continue;
            }

            int key = Mathf.Max(1, level.levelIndex);
            if (levelLookup.ContainsKey(key))
            {
                Debug.LogWarning($"MetarrowGameManager: Duplicate level index {key} detected. Keeping the first definition only.");
                continue;
            }

            levelLookup.Add(key, level);
        }
    }
}
