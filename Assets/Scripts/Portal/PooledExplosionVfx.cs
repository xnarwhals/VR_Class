using System.Collections;
using UnityEngine;

public class PooledExplosionVfx : MonoBehaviour
{
    private ExplosionVfxPool _owner;
    private ParticleSystem _particleSystem;
    private Coroutine _releaseRoutine;

    public void Initialize(ExplosionVfxPool owner, ParticleSystem particleSystem)
    {
        _owner = owner;
        _particleSystem = particleSystem;
    }

    public void Play(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);

        if (_particleSystem == null)
        {
            _owner?.Release(this);
            return;
        }

        if (_releaseRoutine != null)
        {
            StopCoroutine(_releaseRoutine);
            _releaseRoutine = null;
        }

        _particleSystem.Clear(true);
        _particleSystem.Play(true);
        _releaseRoutine = StartCoroutine(ReleaseWhenFinished());
    }

    private IEnumerator ReleaseWhenFinished()
    {
        while (_particleSystem != null && _particleSystem.IsAlive(true))
        {
            yield return null;
        }

        _owner?.Release(this);
        _releaseRoutine = null;
    }
}
