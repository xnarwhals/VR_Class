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

    [Header("Bullseye Emission")]
    [SerializeField] private Renderer bullseyeRenderer;
    [SerializeField] private string emissionColorProperty = "_EmissionColor";

    // cache audio manager
    private MetarrowAudioManager _audioManager;
    private Material _bullseyeMaterial;
    private Color _initialEmissionColor = Color.black;
    private bool _hasEmissionMaterial = false;

    private void Awake()
    {
        _audioManager = MetarrowAudioManager.Instance;
        CacheBullseyeEmission();
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

        SetBullseyeEmissionOff();
        OnArrowHit(arrow, hit);
        onHit?.Invoke(arrow);      
        PlayHitSound(arrow);
          
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
        RestoreBullseyeEmission();
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

    private void PlayHitSound(Arrow arrow)
    {
        if (arrow == null || arrow.HitSound == null)
        {
            return;
        }

        // Manager can be initialized after this target's Awake, so resolve lazily.
        if (_audioManager == null)
        {
            _audioManager = MetarrowAudioManager.Instance;
        }

        if (_audioManager != null)
        {
            _audioManager.PlaySound(arrow.HitSound);
        }
        else
        {
            Debug.LogWarning("Hit sound skipped: MetarrowAudioManager.Instance is null.");
        }
    }

    private void CacheBullseyeEmission()
    {
        if (bullseyeRenderer == null)
        {
            return;
        }

        _bullseyeMaterial = bullseyeRenderer.material;
        if (_bullseyeMaterial == null || !_bullseyeMaterial.HasProperty(emissionColorProperty))
        {
            return;
        }

        _initialEmissionColor = _bullseyeMaterial.GetColor(emissionColorProperty);
        _hasEmissionMaterial = true;
    }

    private void SetBullseyeEmissionOff()
    {
        if (!_hasEmissionMaterial)
        {
            return;
        }

        _bullseyeMaterial.SetColor(emissionColorProperty, Color.black);
        _bullseyeMaterial.DisableKeyword("_EMISSION");
    }

    private void RestoreBullseyeEmission()
    {
        if (!_hasEmissionMaterial)
        {
            return;
        }

        _bullseyeMaterial.SetColor(emissionColorProperty, _initialEmissionColor);
        if (_initialEmissionColor.maxColorComponent > 0f)
        {
            _bullseyeMaterial.EnableKeyword("_EMISSION");
        }
    }
}
