using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Arrow : MonoBehaviour
{
    public float speed = 6f;
    [SerializeField] private float lifeTime = 10f;
    public Transform tip;
    [SerializeField] private AudioClip hitSound;
    public AudioClip HitSound => hitSound;
    public Vector3 LaunchPosition { get; private set; }
    public bool HasLaunchPosition { get; private set; }
    public bool IsLaunchedByAI { get; private set; }
    public IReadOnlyList<Vector3> FlightSamples => _flightSamples;
    public float FlightDuration { get; private set; }

    private Rigidbody _rigidbody;
    private bool _inAir = false;
    private Vector3 _lastPosition = Vector3.zero;
    private Coroutine _rotateRoutine;
    private float _flightStartTime;

    private ParticleSystem _particleSystem;
    private TrailRenderer _trailRenderer;
    private readonly List<Vector3> _flightSamples = new List<Vector3>(64);

    private const int MaxFlightSamples = 256;

    

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _trailRenderer = GetComponentInChildren<TrailRenderer>();

        PullInteraction.pullActionReleased += Release;
        Stop();
    }

    private void OnDestroy()
    {
        PullInteraction.pullActionReleased -= Release;
    }

    private void Release(float value)
    {
        PullInteraction.pullActionReleased -= Release;
        gameObject.transform.parent = null;
        MetarrowGameManager.Instance?.RegisterArrowFired();
        IsLaunchedByAI = false;

        LaunchPosition = tip != null ? tip.position : transform.position;
        HasLaunchPosition = true;

        BeginFlight();
        _rigidbody.useGravity = true;
        _rigidbody.AddForce(transform.forward * value * speed, ForceMode.Impulse);
        OnArrowReleased(value);
    }

    public void LaunchFromAI(Vector3 direction, float launchSpeed, bool useGravity = false)
    {
        if (_rigidbody == null)
        {
            return;
        }

        PullInteraction.pullActionReleased -= Release;
        transform.parent = null;

        LaunchPosition = tip != null ? tip.position : transform.position;
        HasLaunchPosition = true;
        IsLaunchedByAI = true;

        BeginFlight();
        _rigidbody.useGravity = useGravity;
        _rigidbody.linearVelocity = direction.normalized * launchSpeed;
    }

    IEnumerator RotateWithVelocity()
    {
        yield return new WaitForFixedUpdate();
        while (_inAir)
        {
            Quaternion newRotation = Quaternion.LookRotation(_rigidbody.linearVelocity, transform.up);
            transform.rotation = newRotation;
            yield return null;
        }
    }

    void FixedUpdate()
    {
        if (_inAir)
        {
            RecordFlightSample(GetTipPosition());
            CheckCollision();
            _lastPosition = GetTipPosition();
        }
    }

    private void CheckCollision()
    {
        Vector3 tipPosition = GetTipPosition();
        QueryTriggerInteraction triggerInteraction = IsLaunchedByAI
            ? QueryTriggerInteraction.Collide
            : QueryTriggerInteraction.Ignore;

        if (Physics.Linecast(_lastPosition, tipPosition, out RaycastHit hit, Physics.DefaultRaycastLayers, triggerInteraction))
        {
            if (hit.transform == null || hit.transform.IsChildOf(transform))
            {
                return;
            }

            RecordFlightSample(hit.point);
            FlightDuration = Mathf.Max(0f, Time.time - _flightStartTime);

            BaseTarget target = null;
            bool hitBattleCat = false;
            bool hitPlayer = false;

            if (IsLaunchedByAI)
            {
                PlayerArrowHitTracker playerHitTracker = hit.transform.GetComponentInParent<PlayerArrowHitTracker>();
                if (playerHitTracker != null)
                {
                    playerHitTracker.RegisterArrowHit(this, hit);
                    hitPlayer = true;
                }
            }
            else
            {
                BattleCat battleCat = hit.transform.GetComponentInParent<BattleCat>();
                if (battleCat != null)
                {
                    battleCat.HandleArrowHit(this, hit);
                    hitBattleCat = true;
                }
            }

            // Cat hits should not contribute to accuracy, so skip BaseTarget handling for cats.
            if (!hitBattleCat)
            {
                target = hit.transform.GetComponent<BaseTarget>();
                if (target == null)
                {
                    target = hit.transform.GetComponentInParent<BaseTarget>();
                }

                if (target != null)
                {
                    target.HandleArrowHit(this, hit);
                }
            }

            OnArrowHit(hit, target);

            // Cat-hit and player-hit arrows should disappear right after applying damage/effects.
            if (hitBattleCat || hitPlayer)
            {
                Destroy(gameObject);
                return;
            }

            if (hit.transform.TryGetComponent(out Rigidbody rb))
            {
                _rigidbody.interpolation = RigidbodyInterpolation.None;
                transform.parent = hit.transform;
                rb.AddForce(_rigidbody.linearVelocity, ForceMode.Impulse);
            }

            Stop();
        }
    }

    private void Stop()
    {
        _inAir = false;
        if (FlightDuration <= 0f && HasLaunchPosition)
        {
            FlightDuration = Mathf.Max(0f, Time.time - _flightStartTime);
        }

        SetPhysics(false);

        if (_rotateRoutine != null)
        {
            StopCoroutine(_rotateRoutine);
            _rotateRoutine = null;
        }

        if (_particleSystem != null)
        {
            _particleSystem.Stop();
        }

        if (_trailRenderer != null)
        {
            _trailRenderer.emitting = false;
        }

        OnArrowStopped();
        Destroy(gameObject, lifeTime); // heap costly maybe idk
    }

    protected virtual void OnArrowReleased(float pullValue) { }

    protected virtual void OnArrowHit(RaycastHit hit, BaseTarget target) { }

    protected virtual void OnArrowStopped() { }

    private void SetPhysics(bool enabled)
    {
        _rigidbody.isKinematic = !enabled;
        _rigidbody.useGravity = enabled;
    }

    private void BeginFlight()
    {
        _inAir = true;
        SetPhysics(true);
        FlightDuration = 0f;
        _flightStartTime = Time.time;
        _flightSamples.Clear();
        RecordFlightSample(GetTipPosition());

        if (_rotateRoutine != null)
        {
            StopCoroutine(_rotateRoutine);
        }

        _rotateRoutine = StartCoroutine(RotateWithVelocity());
        _lastPosition = GetTipPosition();

        if (_particleSystem != null)
        {
            _particleSystem.Play();
        }

        if (_trailRenderer != null)
        {
            _trailRenderer.emitting = true;
        }
    }

    private void RecordFlightSample(Vector3 samplePoint)
    {
        if (_flightSamples.Count > 0)
        {
            Vector3 lastSample = _flightSamples[_flightSamples.Count - 1];
            if ((samplePoint - lastSample).sqrMagnitude <= 0.000001f)
            {
                return;
            }
        }

        if (_flightSamples.Count >= MaxFlightSamples)
        {
            return;
        }

        _flightSamples.Add(samplePoint);
    }

    private Vector3 GetTipPosition()
    {
        return tip != null ? tip.position : transform.position;
    }
}
