using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour
{
    public float speed = 6f;
    [SerializeField] private float lifeTime = 10f;
    public Transform tip;

    private Rigidbody _rigidbody;
    private bool _inAir = false;
    private Vector3 _lastPosition = Vector3.zero;

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
        PullInteraction.pullActionReleased += Release;
        Stop();
    }   

    private void OnDestroy() {
        PullInteraction.pullActionReleased -= Release;
    }

    private void Release(float value) {
        PullInteraction.pullActionReleased -= Release;
        gameObject.transform.parent = null;
        _inAir = true;
        SetPhysics(true);

        Vector3 force = transform.forward * value * speed;
        _rigidbody.AddForce(force, ForceMode.Impulse);

        StartCoroutine(RotateWithVelocity());
        _lastPosition = tip.position;
    }

    IEnumerator RotateWithVelocity() {
        yield return new WaitForFixedUpdate();
        while (_inAir) {
            Quaternion newRotation = Quaternion.LookRotation(_rigidbody.linearVelocity, transform.up);
            transform.rotation = newRotation;
            yield return null;
        }
    }

    void FixedUpdate() {
        if (_inAir) {
            CheckCollision();
            _lastPosition = tip.position;
        }
    }

    private void CheckCollision() {
        if (Physics.Linecast(_lastPosition, tip.position, out RaycastHit hit)) {
            if (hit.transform.gameObject.layer != 8) {
                if (hit.transform.TryGetComponent(out Rigidbody rb)) {
                    _rigidbody.interpolation = RigidbodyInterpolation.None;
                    transform.parent = hit.transform;
                    rb.AddForce(_rigidbody.linearVelocity, ForceMode.Impulse);
                }
                Stop();
            }
        }
    }

    private void Stop() {
        _inAir = false;
        SetPhysics(false);
    }

    private void SetPhysics(bool enabled) {
        _rigidbody.isKinematic = !enabled;
        _rigidbody.useGravity = enabled;
    }
}
