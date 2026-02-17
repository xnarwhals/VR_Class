using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Fracture))]
public class FractureExplosionEffect : MonoBehaviour
{
    [SerializeField] private float explosionForce = 700f;
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float upwardsModifier = 0.2f;
    [SerializeField] private float fragmentLifetime = 2f;
    [SerializeField] private bool destroySourceObject = true;

    private Fracture _fracture;
    private Vector3 _explosionPoint;
    private bool _hasExplosionPoint;
    private bool _subscribed;

    private void Awake()
    {
        _fracture = GetComponent<Fracture>();
        EnsureCallbacks();
        SubscribeCallbacks();
    }

    private void OnDestroy()
    {
        UnsubscribeCallbacks();
    }

    public void CauseExplosionFracture(Vector3 explosionPoint)
    {
        if (_fracture == null || !gameObject.activeInHierarchy)
        {
            return;
        }

        _explosionPoint = explosionPoint;
        _hasExplosionPoint = true;
        _fracture.callbackOptions.CallOnFracture(null, gameObject, explosionPoint);
        _fracture.CauseFracture();
    }

    private void OnFractureStarted(Collider instigator, GameObject fracturedObject, Vector3 point)
    {
        if (!_hasExplosionPoint)
        {
            _explosionPoint = point;
        }
    }

    private void OnFractureCompleted()
    {
        ApplyExplosionForce();
    }

    private void ApplyExplosionForce()
    {
        Transform fragmentRoot = null;
        if (transform.parent != null)
        {
            fragmentRoot = transform.parent.Find($"{name}Fragments");
        }

        if (fragmentRoot == null)
        {
            GameObject rootObject = GameObject.Find($"{name}Fragments");
            if (rootObject != null)
            {
                fragmentRoot = rootObject.transform;
            }
        }

        if (fragmentRoot == null)
        {
            return;
        }

        Vector3 point = _hasExplosionPoint ? _explosionPoint : transform.position;
        Rigidbody[] rigidbodies = fragmentRoot.GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].AddExplosionForce(explosionForce, point, explosionRadius, upwardsModifier, ForceMode.Impulse);
        }

        Destroy(fragmentRoot.gameObject, fragmentLifetime);
        if (destroySourceObject)
        {
            Destroy(gameObject, fragmentLifetime);
        }
        _hasExplosionPoint = false;
    }

    private void EnsureCallbacks()
    {
        if (_fracture.callbackOptions == null)
        {
            _fracture.callbackOptions = new CallbackOptions();
        }

        if (_fracture.callbackOptions.onFracture == null)
        {
            _fracture.callbackOptions.onFracture = new UnityEvent<Collider, GameObject, Vector3>();
        }

        if (_fracture.callbackOptions.onCompleted == null)
        {
            _fracture.callbackOptions.onCompleted = new UnityEvent();
        }
    }

    private void SubscribeCallbacks()
    {
        if (_subscribed)
        {
            return;
        }

        _fracture.callbackOptions.onFracture.AddListener(OnFractureStarted);
        _fracture.callbackOptions.onCompleted.AddListener(OnFractureCompleted);
        _subscribed = true;
    }

    private void UnsubscribeCallbacks()
    {
        if (!_subscribed || _fracture == null || _fracture.callbackOptions == null)
        {
            return;
        }

        _fracture.callbackOptions.onFracture?.RemoveListener(OnFractureStarted);
        _fracture.callbackOptions.onCompleted?.RemoveListener(OnFractureCompleted);
        _subscribed = false;
    }
}
