using UnityEngine;
using UnityEngine.Events;

public class TimedPuzzle : GatchaPuzzle
{
    [Header("Feedback")]
    [SerializeField] private CounterStepAnimator counterStepAnimator;
    private FeedbackArrow _feedbackArrowA;
    private FeedbackArrow _feedbackArrowB;
    public GameObject LightUpArrowA;
    public GameObject LightUpArrowB;

    [Header("Targets")]
    [SerializeField] private BaseTarget[] swapTargets;

    [Header("Timing")]
    [SerializeField] private float swapIntervalSeconds = 2f;
    [SerializeField] private int maxSwaps = 5; // 0 = infinite
    [SerializeField] private int requiredSuccessfulHits = 5; // <= 0 uses maxSwaps

    private bool _swapHitListenersRegistered;
    private float _swapTimer;
    private int _activeSwapTargetIndex = -1;
    private int _swapCount;
    private int _successfulHitCount;
    private UnityAction[] _swapTargetHitActions;

    private void Update()
    {
        if (!IsPuzzleRunning || swapTargets == null || swapTargets.Length == 0)
        {
            return;
        }

        _swapTimer += Time.deltaTime;
        if (_swapTimer >= swapIntervalSeconds)
        {
            _swapTimer = 0f;
            ActivateNextSwapTarget(true);
        }
    }

    protected override void InitializePuzzle()
    {
        if (swapIntervalSeconds <= 0f)
        {
            swapIntervalSeconds = 0.1f;
        }

        CacheFeedbackArrows();
        RegisterSwapTargetHitListeners();

        if (counterStepAnimator != null)
        {
            counterStepAnimator.ConfigureTotalSteps(GetRequiredSuccessfulHits());
        }

        base.InitializePuzzle();
        DeactivateAllSwapTargets();
        SetFeedbackArrowsForActiveTarget(-1);
        counterStepAnimator?.ResetCounter();
    }

    protected override void CleanupPuzzle()
    {
        UnregisterSwapTargetHitListeners();
    }

    protected override void OnPuzzleStarted()
    {
        _swapTimer = 0f;
        _swapCount = 0;
        _successfulHitCount = 0;
        _activeSwapTargetIndex = -1;

        counterStepAnimator?.ResetCounter();
        ActivateNextSwapTarget(false);
    }

    protected override void OnPuzzleStopped()
    {
        _activeSwapTargetIndex = -1;
        _swapCount = 0;
        _successfulHitCount = 0;

        DeactivateAllSwapTargets();
        SetFeedbackArrowsForActiveTarget(-1);
        counterStepAnimator?.ResetCounter();
    }

    protected override void OnPuzzleSucceeded()
    {
        base.OnPuzzleSucceeded();
        _activeSwapTargetIndex = -1;
        DeactivateAllSwapTargets();
        SetFeedbackArrowsForActiveTarget(-1);
    }

    private void ActivateNextSwapTarget(bool countAsSwap)
    {
        if (swapTargets == null || swapTargets.Length == 0)
        {
            return;
        }

        _activeSwapTargetIndex = (_activeSwapTargetIndex + 1) % swapTargets.Length;

        for (int i = 0; i < swapTargets.Length; i++)
        {
            BaseTarget target = swapTargets[i];
            if (target == null)
            {
                continue;
            }

            target.SetTargetAvailability(i == _activeSwapTargetIndex);
        }

        SetFeedbackArrowsForActiveTarget(_activeSwapTargetIndex);

        if (countAsSwap)
        {
            _swapCount++;
            if (maxSwaps > 0 && _swapCount >= maxSwaps)
            {
                FailPuzzle();
            }
        }
    }

    private void DeactivateAllSwapTargets()
    {
        if (swapTargets == null)
        {
            return;
        }

        for (int i = 0; i < swapTargets.Length; i++)
        {
            if (swapTargets[i] != null)
            {
                swapTargets[i].SetTargetAvailability(false);
            }
        }
    }

    private void RegisterSwapTargetHitListeners()
    {
        if (_swapHitListenersRegistered || swapTargets == null)
        {
            return;
        }

        _swapTargetHitActions = new UnityAction[swapTargets.Length];

        for (int i = 0; i < swapTargets.Length; i++)
        {
            if (swapTargets[i] != null)
            {
                int targetIndex = i;
                UnityAction hitAction = () => OnSwapTargetHit(targetIndex);
                _swapTargetHitActions[i] = hitAction;
                swapTargets[i].onFirstHit.AddListener(hitAction);
            }
        }

        _swapHitListenersRegistered = true;
    }

    private void UnregisterSwapTargetHitListeners()
    {
        if (!_swapHitListenersRegistered || swapTargets == null)
        {
            return;
        }

        for (int i = 0; i < swapTargets.Length; i++)
        {
            if (swapTargets[i] != null && _swapTargetHitActions != null && i < _swapTargetHitActions.Length && _swapTargetHitActions[i] != null)
            {
                swapTargets[i].onFirstHit.RemoveListener(_swapTargetHitActions[i]);
            }
        }

        _swapTargetHitActions = null;
        _swapHitListenersRegistered = false;
    }

    private void OnSwapTargetHit(int targetIndex)
    {
        if (!IsPuzzleRunning)
        {
            return;
        }

        if (targetIndex != _activeSwapTargetIndex)
        {
            return;
        }

        counterStepAnimator?.OnSuccessfulHit();
        _successfulHitCount++;

        int hitsNeeded = GetRequiredSuccessfulHits();
        if (hitsNeeded > 0 && _successfulHitCount >= hitsNeeded)
        {
            SucceedPuzzle();
            return;
        }

        _swapTimer = 0f;
        ActivateNextSwapTarget(false);
    }

    private void CacheFeedbackArrows()
    {
        _feedbackArrowA = LightUpArrowA != null ? LightUpArrowA.GetComponent<FeedbackArrow>() : null;
        _feedbackArrowB = LightUpArrowB != null ? LightUpArrowB.GetComponent<FeedbackArrow>() : null;
    }

    private void SetFeedbackArrowsForActiveTarget(int targetIndex)
    {
        if (_feedbackArrowA != null)
        {
            _feedbackArrowA.SetArrowActive(targetIndex == 0);
        }

        if (_feedbackArrowB != null)
        {
            _feedbackArrowB.SetArrowActive(targetIndex == 1);
        }
    }

    private int GetRequiredSuccessfulHits()
    {
        if (requiredSuccessfulHits > 0)
        {
            return requiredSuccessfulHits;
        }

        return maxSwaps > 0 ? maxSwaps : 1;
    }

    private void FailPuzzle()
    {
        CompletePuzzleFailure("TimedPuzzle failed.");
    }

    private void SucceedPuzzle()
    {
        CompletePuzzleSuccess("TimedPuzzle success.");
    }
}
