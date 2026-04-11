using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteStatsUI : MonoBehaviour
{
    [Header("Level Binding")]
    [Tooltip("Only show stats when this level index completes.")]
    [Min(1)][SerializeField] private int targetLevelIndex = 1;
    [Tooltip("Hide this panel when another level completes.")]
    [SerializeField] private bool hideWhenNonMatchingLevelCompletes = true;

    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text hitSummaryText;
    [SerializeField] private Image rankImage;
    [Header("Rank Sprites")]
    [SerializeField] private Sprite rankS;
    [SerializeField] private Sprite rankA;
    [SerializeField] private Sprite rankB;
    [SerializeField] private Sprite rankC;
    [SerializeField] private Sprite rankF;

    [Header("Grading Weights")]
    [Range(0f, 1f)][SerializeField] private float accuracyWeight = 0.6f;
    [Range(0f, 1f)][SerializeField] private float timeWeight = 0.4f;

    [Header("Time Grading (seconds)")]
    [Min(0.01f)][SerializeField] private float idealTimeSeconds = 180f;
    [Min(0.02f)][SerializeField] private float failTimeSeconds = 360f;

    [Header("Rank Thresholds (0-100)")]
    [Range(0f, 100f)][SerializeField] private float sThreshold = 90f;
    [Range(0f, 100f)][SerializeField] private float aThreshold = 80f;
    [Range(0f, 100f)][SerializeField] private float bThreshold = 65f;
    [Range(0f, 100f)][SerializeField] private float cThreshold = 50f;
    [SerializeField] private bool showPanelOnComplete = true;

    private MetarrowGameManager _manager;

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }
    }

    private void OnEnable()
    {
        TryBindManager();
    }

    private void Start()
    {
        TryBindManager();
        if (_manager != null && _manager.HasLevelCompleted && IsRelevantLevel())
        {
            ShowCompletedStats();
        }
        else if (hideWhenNonMatchingLevelCompletes && panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void OnDisable()
    {
        UnbindManager();
    }

    private void TryBindManager()
    {
        if (_manager != null)
        {
            return;
        }

        _manager = MetarrowGameManager.Instance;
        if (_manager != null)
        {
            _manager.levelCompleted += HandleLevelCompleted;
        }
    }

    private void UnbindManager()
    {
        if (_manager == null)
        {
            return;
        }

        _manager.levelCompleted -= HandleLevelCompleted;
        _manager = null;
    }

    private void HandleLevelCompleted()
    {
        if (!IsRelevantLevel())
        {
            if (hideWhenNonMatchingLevelCompletes && panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
            return;
        }

        ShowCompletedStats();
    }

    private void ShowCompletedStats()
    {
        if (_manager == null)
        {
            return;
        }

        if (showPanelOnComplete && panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (timeText != null)
        {
            timeText.text = $"Time: {FormatElapsedTime(_manager.ElapsedLevelTime)}";
        }

        if (accuracyText != null)
        {
            accuracyText.text = $"Accuracy: {_manager.GetAccuracyPercent():0.0}%";
        }

        if (hitSummaryText != null)
        {
            hitSummaryText.text = $"Hits: {_manager.SuccessfulHits}/{_manager.ArrowsFired}  Targets: {_manager.UniqueTargetsHit}";
        }

        UpdateRankImage();
    }

    private void UpdateRankImage()
    {
        if (rankImage == null || _manager == null)
        {
            return;
        }

        float gradeScore = CalculateGradeScore(_manager.ElapsedLevelTime, _manager.GetAccuracyPercent());
        rankImage.sprite = GetRankSprite(gradeScore);
    }

    private float CalculateGradeScore(float elapsedSeconds, float accuracyPercent)
    {
        float clampedAccuracy = Mathf.Clamp(accuracyPercent, 0f, 100f);
        float normalizedAccuracyScore = clampedAccuracy;

        float safeIdeal = Mathf.Max(0.01f, idealTimeSeconds);
        float safeFail = Mathf.Max(safeIdeal + 0.01f, failTimeSeconds);
        float clampedTime = Mathf.Max(0f, elapsedSeconds);

        // 100 at/under ideal time, 0 at/over fail time.
        float normalizedTimeScore = 100f;
        if (clampedTime > safeIdeal)
        {
            float t = Mathf.InverseLerp(safeIdeal, safeFail, clampedTime);
            normalizedTimeScore = Mathf.Lerp(100f, 0f, t);
        }

        float totalWeight = Mathf.Max(0.0001f, accuracyWeight + timeWeight);
        float weightedScore =
            (normalizedAccuracyScore * accuracyWeight) +
            (normalizedTimeScore * timeWeight);

        return weightedScore / totalWeight;
    }

    private Sprite GetRankSprite(float score)
    {
        // Enforce descending threshold logic even if values are misconfigured in the inspector.
        float s = sThreshold;
        float a = Mathf.Min(aThreshold, s);
        float b = Mathf.Min(bThreshold, a);
        float c = Mathf.Min(cThreshold, b);

        if (score >= s && rankS != null) return rankS;
        if (score >= a && rankA != null) return rankA;
        if (score >= b && rankB != null) return rankB;
        if (score >= c && rankC != null) return rankC;
        return rankF;
    }

    private static string FormatElapsedTime(float elapsedSeconds)
    {
        if (elapsedSeconds < 0f)
        {
            elapsedSeconds = 0f;
        }

        int minutes = Mathf.FloorToInt(elapsedSeconds / 60f);
        float seconds = elapsedSeconds - (minutes * 60f);
        return $"{minutes:00}:{seconds:00.00}";
    }

    private bool IsRelevantLevel()
    {
        return _manager != null && _manager.CurrentLevel == targetLevelIndex;
    }
}
