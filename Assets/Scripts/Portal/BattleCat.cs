using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public class BattleCat : MonoBehaviour
{
    // Refs
    private Transform _player;
    [SerializeField] private Transform modelRoot; // visual model to rotate; keep root physics independent
    private NavMeshAgent _navMeshAgent; // for movement
    private BattleCatMovement _movementController;
    private Rigidbody _rigidbody;
    private bool _warnedModelRootIsPhysicsRoot;
    private AudioSource _audioSource;

    // General Settings
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float normalArrowDamage = 20f;
    [SerializeField] private float explodeArrowDamage = 25f;
    [SerializeField] private UnityEvent onCatDied;
    private float _currentHealth;
    private bool _isDead;
    private BattleCatState _currentState;

    // Movement Settings NavMesh
    public float moveSpeed = 5f;

    // Attack Settings
    [SerializeField] private Transform bowMuzzle;
    [SerializeField] private bool autoStartAttacking = true;
    [SerializeField] private bool facePlayerWhenAggro = true;
    [SerializeField] private bool moveBeforeEachShot = true;
    [SerializeField] private float modelYawOffset = 0f; // use if model's forward axis is not +Z
    [SerializeField] private float playerAimYOffset = 1.2f;
    public float chargeUpTime = 2f;
    public float shotInterval = 4f;
    public float arrowSpeed = 6f;
    public float arrowLifeTime = 10f;
    public GameObject projectilePrefab; // arrow shot from bow of cat
    [Header("Arrow Telegraph")]
    [SerializeField] private bool enhanceArrowTrail = true;
    [SerializeField] private float enemyArrowTrailTime = 0.9f;
    [SerializeField] private float enemyArrowTrailWidthMultiplier = 1.35f;
    [SerializeField] private Gradient enemyArrowTrailColor;

    private Coroutine _attackLoopRoutine;
    private bool _killedByPlayerArrow;

    public event Action<BattleCat, bool> CatDied;

    private void Start()
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _movementController = GetComponent<BattleCatMovement>();
        _rigidbody = GetComponent<Rigidbody>();
        _currentHealth = maxHealth;
        _currentState = BattleCatState.Idle;

        if (modelRoot == null)
        {
            modelRoot = transform;
        }

        if (_navMeshAgent != null)
        {
            _navMeshAgent.updateRotation = false;
        }

        if (autoStartAttacking)
        {
            TryStartAttackLoop();
        }
    }

    private void Update()
    {
        UpdateFacing();
    }

    public void Initialize(Transform playerTransform)
    {
        _player = playerTransform;
        TryStartAttackLoop();
    }

    public void ApplyStats(BattleCatStats stats)
    {
        if (stats == null)
        {
            return;
        }

        maxHealth = stats.maxHealth;
        moveSpeed = stats.moveSpeed;
        chargeUpTime = stats.chargeUpTime;
        shotInterval = stats.shotInterval;
        arrowSpeed = stats.arrowSpeed;
        arrowLifeTime = stats.arrowLifeTime;
        normalArrowDamage = stats.normalArrowDamage;
        explodeArrowDamage = stats.explodeArrowDamage;

        if (!_isDead)
        {
            _currentHealth = maxHealth;
        }
    }

    public void HandleArrowHit(Arrow arrow, RaycastHit hit)
    {
        if (_isDead || arrow == null || arrow.IsLaunchedByAI)
        {
            return;
        }

        float damage = arrow is ExplodeArrow ? explodeArrowDamage : normalArrowDamage;
        _audioSource?.Play();
        ApplyDamage(damage, true);
    }

    private void TryStartAttackLoop()
    {
        if (_attackLoopRoutine != null)
        {
            return;
        }

        _attackLoopRoutine = StartCoroutine(AttackLoop());
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            if (_player == null || projectilePrefab == null || bowMuzzle == null)
            {
                yield return null;
                continue;
            }

            if (moveBeforeEachShot && _movementController != null)
            {
                _currentState = BattleCatState.Moving;
                yield return _movementController.MoveToRandomPoint(moveSpeed);
            }

            _currentState = BattleCatState.Charging;
            yield return new WaitForSeconds(chargeUpTime);

            FireArrowAtPlayer();

            _currentState = BattleCatState.Attacking;
            yield return new WaitForSeconds(shotInterval);

            _currentState = BattleCatState.Idle;
        }
    }

    private void FireArrowAtPlayer()
    {
        if (_player == null || projectilePrefab == null || bowMuzzle == null)
        {
            return;
        }

        Vector3 playerAimPosition = _player.position + Vector3.up * playerAimYOffset;
        Vector3 launchDirection = (playerAimPosition - bowMuzzle.position).normalized;
        Quaternion launchRotation = Quaternion.LookRotation(launchDirection, Vector3.up);

        GameObject arrowObject = Instantiate(projectilePrefab, bowMuzzle.position, launchRotation);
        ConfigureEnemyArrowTrail(arrowObject);

        if (arrowObject.TryGetComponent(out Arrow arrow))
        {
            arrow.LaunchFromAI(launchDirection, arrowSpeed, false);
        }
        else if (arrowObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.linearVelocity = launchDirection * arrowSpeed;
            Destroy(arrowObject, arrowLifeTime);
        }
        else
        {
            Destroy(arrowObject, arrowLifeTime);
        }
    }

    private void ConfigureEnemyArrowTrail(GameObject arrowObject)
    {
        if (!enhanceArrowTrail || arrowObject == null)
        {
            return;
        }

        TrailRenderer trail = arrowObject.GetComponentInChildren<TrailRenderer>(true);
        if (trail == null)
        {
            return;
        }

        trail.Clear();
        trail.time = Mathf.Max(0.05f, enemyArrowTrailTime);
        trail.widthMultiplier = Mathf.Max(0.01f, trail.widthMultiplier * enemyArrowTrailWidthMultiplier);

        if (enemyArrowTrailColor != null && enemyArrowTrailColor.colorKeys != null && enemyArrowTrailColor.colorKeys.Length > 0)
        {
            trail.colorGradient = enemyArrowTrailColor;
        }

        trail.emitting = true;
    }

    private void UpdateFacing()
    {
        if (!facePlayerWhenAggro)
        {
            return;
        }

        if ((_currentState != BattleCatState.Charging && _currentState != BattleCatState.Attacking) || _player == null)
        {
            return;
        }

        Vector3 lookDirection = _player.position - modelRoot.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up) * Quaternion.Euler(0f, modelYawOffset, 0f);

        if (_rigidbody != null && !_rigidbody.isKinematic && modelRoot == transform)
        {
            if (!_warnedModelRootIsPhysicsRoot)
            {
                Debug.LogWarning("BattleCat modelRoot is set to root while Rigidbody is non-kinematic. Assign a child modelRoot to decouple visual facing from root physics.", this);
                _warnedModelRootIsPhysicsRoot = true;
            }
        }

        modelRoot.rotation = targetRotation;
    }

    private void ApplyDamage(float damage, bool fromPlayerArrow = false)
    {
        if (_isDead)
        {
            return;
        }

        if (fromPlayerArrow)
        {
            _killedByPlayerArrow = true;
        }

        _currentHealth -= damage;
        if (_currentHealth > 0f)
        {
            return;
        }

        Die();
    }

    private void Die()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;
        _currentHealth = 0f;
        _currentState = BattleCatState.Idle;

        if (_attackLoopRoutine != null)
        {
            StopCoroutine(_attackLoopRoutine);
            _attackLoopRoutine = null;
        }

        if (_movementController != null)
        {
            _movementController.StopMovement();
        }

        CatDied?.Invoke(this, _killedByPlayerArrow);
        onCatDied?.Invoke();
        Destroy(gameObject);
    }
}

public enum BattleCatState
{
    Idle,
    Moving,
    Charging,
    Attacking
}

[System.Serializable]
public class BattleCatStats
{
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float chargeUpTime = 2f;
    public float shotInterval = 4f;
    public float arrowSpeed = 6f;
    public float arrowLifeTime = 10f;
    public float normalArrowDamage = 20f;
    public float explodeArrowDamage = 25f;
}
