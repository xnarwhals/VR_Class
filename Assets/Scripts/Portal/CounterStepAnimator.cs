using UnityEngine;

public class CounterStepAnimator : MonoBehaviour
{
    [SerializeField] private Animator counterAnimator;
    [SerializeField] private string counterStateName = "CounterState";
    [SerializeField] private int totalSteps = 5;

    private int _stateHash;
    private int _currentStep;

    private void Awake()
    {
        if (counterAnimator == null)
        {
            counterAnimator = GetComponent<Animator>();
        }

        _stateHash = Animator.StringToHash(counterStateName);
        ResetCounter();
    }

    public void ConfigureTotalSteps(int steps)
    {
        totalSteps = Mathf.Max(1, steps);
        _currentStep = Mathf.Min(_currentStep, totalSteps);
        ApplyStep();
    }

    public void OnSuccessfulHit()
    {
        if (counterAnimator == null || _currentStep >= totalSteps)
        {
            return;
        }

        _currentStep++;
        ApplyStep();
    }

    public void ResetCounter()
    {
        if (counterAnimator == null)
        {
            return;
        }

        _currentStep = 0;
        ApplyStep();
    }

    private void ApplyStep()
    {
        if (counterAnimator == null)
        {
            return;
        }

        float normalizedTime = totalSteps <= 0 ? 0f : (float)_currentStep / totalSteps;
        counterAnimator.Play(_stateHash, 0, normalizedTime);
        counterAnimator.Update(0f);
        counterAnimator.speed = 0f;
    }
}
