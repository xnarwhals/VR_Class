using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour
{
    public float speed = 6f;
    [SerializeField] private float lifeTime = 10f;
    public Transform tip;
    [SerializeField] private AudioClip hitSound;
    public AudioClip HitSound => hitSound;
    public Vector3 LaunchPosition { get; private set; }
    public bool HasLaunchPosition { get; private set; }

    private Rigidbody _rigidbody;
    private bool _inAir = false;
    private Vector3 _lastPosition = Vector3.zero;

    private ParticleSystem _particleSystem;
    private TrailRenderer _trailRenderer;

    

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
        _inAir = true;
        SetPhysics(true);
        MetarrowGameManager.Instance?.RegisterArrowFired();

        LaunchPosition = tip != null ? tip.position : transform.position;
        HasLaunchPosition = true;

        Vector3 force = transform.forward * value * speed;
        _rigidbody.AddForce(force, ForceMode.Impulse);
        OnArrowReleased(value);

        StartCoroutine(RotateWithVelocity());
        _lastPosition = tip.position;

        _particleSystem.Play();
        _trailRenderer.emitting = true;
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
            CheckCollision();
            _lastPosition = tip.position;
        }
    }

    private void CheckCollision()
    {
        if (Physics.Linecast(_lastPosition, tip.position, out RaycastHit hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == null || hit.transform.IsChildOf(transform))
            {
                return;
            }

            if (hit.transform.gameObject.layer != 8) // player
            {
                BaseTarget target = hit.transform.GetComponent<BaseTarget>();

                if (target == null)
                {
                    target = hit.transform.GetComponentInParent<BaseTarget>();
                }

                if (target != null)
                {
                    target.HandleArrowHit(this, hit);
                }
                
                OnArrowHit(hit, target);

                if (hit.transform.TryGetComponent(out Rigidbody rb))
                {
                    _rigidbody.interpolation = RigidbodyInterpolation.None;
                    transform.parent = hit.transform;
                    rb.AddForce(_rigidbody.linearVelocity, ForceMode.Impulse);
                }
                Stop();
            }
        }
    }

    private void Stop()
    {
        _inAir = false;
        SetPhysics(false);

        _particleSystem.Stop();
        _trailRenderer.emitting = false;
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
}
