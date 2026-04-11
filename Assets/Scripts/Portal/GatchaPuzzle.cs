using UnityEngine;
using UnityEngine.Events;

public class GatchaPuzzle : MonoBehaviour
{
    // General flow for boss puzzles that are started by a target hit.

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip puzzleCompleteSound;
    [SerializeField] protected AudioClip puzzleFailSound;

    [Header("Targets")]
    [SerializeField] protected BaseTarget startPuzzleTarget;

    [Header("Outcome Events")]
    [SerializeField] private UnityEvent onPuzzleSuccess;
    [SerializeField] private UnityEvent onPuzzleFailure;

    private bool _puzzleRunning;

    protected bool IsPuzzleRunning => _puzzleRunning;

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        InitializePuzzle();
    }

    private void OnDestroy()
    {
        CleanupPuzzle();
    }

    // Hook this method to StartPuzzleTarget.onFirstHit in the inspector.
    public virtual void StartPuzzle()
    {
        if (_puzzleRunning)
        {
            return;
        }

        _puzzleRunning = true;
        SetStartTargetAvailability(false);
        OnPuzzleStarted();
    }

    public virtual void StopPuzzle()
    {
        _puzzleRunning = false;
        SetStartTargetAvailability(true);
        OnPuzzleStopped();
    }

    protected virtual void InitializePuzzle()
    {
        SetStartTargetAvailability(true);
    }

    protected virtual void CleanupPuzzle()
    {
    }

    protected virtual void OnPuzzleStarted()
    {
    }

    protected virtual void OnPuzzleStopped()
    {
    }

    protected virtual void OnPuzzleSucceeded()
    {
        MetarrowGameManager.Instance?.SetLevelGatchaPuzzleCompleted(true);
    }

    protected virtual void OnPuzzleFailed()
    {
    }

    protected void CompletePuzzleSuccess(string logMessage)
    {
        _puzzleRunning = false;
        OnPuzzleSucceeded();
        SetStartTargetAvailability(false);

        onPuzzleSuccess?.Invoke();
        audioSource?.PlayOneShot(puzzleCompleteSound);

        if (!string.IsNullOrEmpty(logMessage))
        {
            Debug.Log(logMessage);
        }
    }

    protected void CompletePuzzleFailure(string logMessage)
    {
        OnPuzzleFailed();
        onPuzzleFailure?.Invoke();
        audioSource?.PlayOneShot(puzzleFailSound);

        if (!string.IsNullOrEmpty(logMessage))
        {
            Debug.Log(logMessage);
        }

        StopPuzzle();
    }

    protected void SetStartTargetAvailability(bool isActive)
    {
        if (startPuzzleTarget != null)
        {
            startPuzzleTarget.SetTargetAvailability(isActive);
        }
    }
}
