using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ArrowHitEvent : UnityEvent<Arrow> { }

// Base class for anything that reacts to arrows.
public class BaseTarget : MonoBehaviour
{
    [Header("Hit Behavior")]
    [SerializeField] private bool allowMultipleHits = false;
    [SerializeField] private bool disableAfterFirstHit = false;

    [Header("Events")]
    public ArrowHitEvent onHit;
    public UnityEvent onFirstHit;
    public UnityEvent onReset;

    private bool _hasBeenHit = false;

    public void HandleArrowHit(Arrow arrow, RaycastHit hit)
    {
        if (!allowMultipleHits && _hasBeenHit) {
            return;
        }

        bool firstHit = !_hasBeenHit;
        _hasBeenHit = true;

        OnArrowHit(arrow, hit);
        onHit?.Invoke(arrow);
        if (firstHit) {
            onFirstHit?.Invoke();
        }

        if (disableAfterFirstHit && firstHit) {
            enabled = false;
        }
    }

    public void ResetTarget()
    {
        _hasBeenHit = false;
        enabled = true;
        onReset?.Invoke();
    }

    // Override this in derived targets for custom logic.
    protected virtual void OnArrowHit(Arrow arrow, RaycastHit hit) { }
}
