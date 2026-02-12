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

    // cache audio manager
    private MetarrowAudioManager _audioManager;

    private void Awake()
    {
        _audioManager = MetarrowAudioManager.Instance;
    }


    private bool _hasBeenHit = false;

    public void HandleArrowHit(Arrow arrow, RaycastHit hit)
    {
        if (!CanBeHit(arrow, hit)) {
            return;
        }

        if (!allowMultipleHits && _hasBeenHit) {
            return;
        }

        bool firstHit = !_hasBeenHit;
        _hasBeenHit = true;

        OnArrowHit(arrow, hit);
        onHit?.Invoke(arrow);      
        _audioManager?.PlaySound(arrow.HitSound);
          
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

    // Override for custom hit gating rules (distance checks, puzzle state, etc.)
    protected virtual bool CanBeHit(Arrow arrow, RaycastHit hit)
    {
        return enabled;
    }

    // Override this in derived targets for custom logic.
    protected virtual void OnArrowHit(Arrow arrow, RaycastHit hit) {
        Debug.Log($"{gameObject.name} was hit by an arrow at {hit.point}");
    }
}
