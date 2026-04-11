using System.Collections.Generic;
using UnityEngine;

public class ExplosionVfxPool : MonoBehaviour
{
    public static ExplosionVfxPool Instance { get; private set; }

    [SerializeField] private ParticleSystem explosionPrefab;
    [SerializeField] private int initialSize = 12;
    [SerializeField] private bool expandIfExhausted = true;

    private readonly Queue<PooledExplosionVfx> _available = new Queue<PooledExplosionVfx>();
    private readonly HashSet<PooledExplosionVfx> _inUse = new HashSet<PooledExplosionVfx>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple ExplosionVfxPool instances found. Using the first one.", this);
            return;
        }

        Instance = this;
        Prewarm();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    internal void Release(PooledExplosionVfx pooled)
    {
        if (pooled == null || !_inUse.Remove(pooled))
        {
            return;
        }

        pooled.gameObject.SetActive(false);
        _available.Enqueue(pooled);
    }

    private void Prewarm()
    {
        if (explosionPrefab == null)
        {
            Debug.LogWarning("ExplosionVfxPool is missing explosionPrefab.", this);
            return;
        }

        for (int i = 0; i < initialSize; i++)
        {
            PooledExplosionVfx pooled = CreateNewInstance();
            if (pooled != null)
            {
                _available.Enqueue(pooled);
            }
        }
    }

    private PooledExplosionVfx GetNext()
    {
        if (_available.Count > 0)
        {
            return _available.Dequeue();
        }

        if (!expandIfExhausted)
        {
            return null;
        }

        return CreateNewInstance();
    }

    private PooledExplosionVfx CreateNewInstance()
    {
        if (explosionPrefab == null)
        {
            return null;
        }

        ParticleSystem instance = Instantiate(explosionPrefab, transform);
        GameObject instanceObject = instance.gameObject;
        PooledExplosionVfx pooled = instanceObject.GetComponent<PooledExplosionVfx>();
        if (pooled == null)
        {
            pooled = instanceObject.AddComponent<PooledExplosionVfx>();
        }

        pooled.Initialize(this, instance);
        instanceObject.SetActive(false);
        return pooled;
    }

    public void Play(Vector3 position, Quaternion rotation)
    {
        PooledExplosionVfx pooled = GetNext();
        if (pooled == null)
        {
            return;
        }

        _inUse.Add(pooled);
        pooled.gameObject.SetActive(true);
        pooled.Play(position, rotation);
    }
}
