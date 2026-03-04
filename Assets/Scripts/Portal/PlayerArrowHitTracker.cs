using UnityEngine;
using UnityEngine.Events;
using System;

public class PlayerArrowHitTracker : MonoBehaviour
{
    [SerializeField] private int hitsToDie = 3;
    [SerializeField] private bool ignoreHitsAfterDeath = true;
    [SerializeField] private bool debugLogs = false;
    [SerializeField] private UnityEvent onPlayerHit;
    [SerializeField] private UnityEvent onPlayerDied;

    private int _currentHits;
    private bool _isDead;

    public int CurrentHits => _currentHits;
    public bool IsDead => _isDead;
    public event Action PlayerDied;

    public void RegisterArrowHit(Arrow arrow, RaycastHit hit)
    {
        if (ignoreHitsAfterDeath && _isDead)
        {
            return;
        }

        _currentHits++;
        onPlayerHit?.Invoke();

        if (debugLogs)
        {
            Debug.Log($"Player hit by arrow ({_currentHits}/{hitsToDie}).", this);
        }

        if (_isDead || _currentHits < hitsToDie)
        {
            return;
        }

        _isDead = true;
        onPlayerDied?.Invoke();
        PlayerDied?.Invoke();

        if (debugLogs)
        {
            Debug.Log("Player death event invoked from arrow hits.", this);
        }
    }

    public void ResetHits()
    {
        _currentHits = 0;
        _isDead = false;
    }
}
