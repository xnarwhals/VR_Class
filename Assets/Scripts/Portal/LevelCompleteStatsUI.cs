using TMPro;
using UnityEngine;

public class LevelCompleteStatsUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text hitSummaryText;
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
        if (_manager != null && _manager.HasLevelCompleted)
        {
            ShowCompletedStats();
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
}
