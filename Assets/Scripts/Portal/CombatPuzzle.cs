using UnityEngine;
using UnityEngine.AI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CombatPuzzle : GatchaPuzzle
{
    [SerializeField] private BattleCat battleCatPrefab;
    [SerializeField] private Transform islandOne;
    [SerializeField] private Transform islandTwo;
    [SerializeField] private Transform playerTransform; // assign XR rig root in inspector
    [Header("Cat Stats")]
    [SerializeField] private BattleCatStats islandOneStats = new BattleCatStats();
    [SerializeField] private BattleCatStats islandTwoStats = new BattleCatStats();
    [SerializeField] private PlayerArrowHitTracker playerHitTracker;
    [Header("Spawn Settings")]
    [SerializeField] private bool snapSpawnToNavMesh = true;
    [SerializeField] private float navMeshSnapDistance = 4f;
    [Header("Debug")]
    [SerializeField] private bool allowKeyboardStart = true;
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private Key startPuzzleKey = Key.Space;
#else
    [SerializeField] private KeyCode startPuzzleKey = KeyCode.Space;
#endif
    private BattleCat _spawnedCatOne;
    private BattleCat _spawnedCatTwo;
    private bool _subscribedToPlayerDeath;
    private int _catsKilledByPlayerArrows;

    private void Update()
    {
        if (!allowKeyboardStart)
        {
            return;
        }

        if (IsStartKeyPressed())
        {
            StartPuzzle();
        }
    }

    private bool IsStartKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        return keyboard[startPuzzleKey].wasPressedThisFrame;
#else
        return false;
#endif
    }

    protected override void OnPuzzleStarted()
    {
        if (battleCatPrefab == null || islandOne == null || islandTwo == null)
        {
            Debug.LogError("CombatPuzzle is missing cat prefab or spawn points.", this);
            return;
        }

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("Player transform not assigned and no GameObject with tag 'Player' found.");
                return;
            }
        }

        ResolvePlayerHitTracker();
        SubscribeToPlayerDeath();
        playerHitTracker?.ResetHits();
        _catsKilledByPlayerArrows = 0;

        CleanupSpawnedCats();
        _spawnedCatOne = SpawnCat(islandOne, islandOneStats);
        _spawnedCatTwo = SpawnCat(islandTwo, islandTwoStats);
    }

    protected override void OnPuzzleStopped()
    {
        CleanupSpawnedCats();
    }

    private void OnDisable()
    {
        UnsubscribeFromPlayerDeath();
    }

    private BattleCat SpawnCat(Transform spawnPoint, BattleCatStats stats)
    {
        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        if (snapSpawnToNavMesh && NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit navHit, navMeshSnapDistance, NavMesh.AllAreas))
        {
            spawnPosition = navHit.position;
        }
        else if (snapSpawnToNavMesh)
        {
            Debug.LogWarning(
                $"CombatPuzzle could not find NavMesh near spawn point '{spawnPoint.name}'. Using transform position {spawnPoint.position}.",
                this);
        }

        BattleCat cat = Instantiate(battleCatPrefab, spawnPosition, spawnRotation);

        cat.ApplyStats(stats);
        cat.CatDied += HandleSpawnedCatDied;
        cat.Initialize(playerTransform);
        return cat;
    }

    private void CleanupSpawnedCats()
    {
        if (_spawnedCatOne != null)
        {
            _spawnedCatOne.CatDied -= HandleSpawnedCatDied;
            Destroy(_spawnedCatOne.gameObject);
            _spawnedCatOne = null;
        }

        if (_spawnedCatTwo != null)
        {
            _spawnedCatTwo.CatDied -= HandleSpawnedCatDied;
            Destroy(_spawnedCatTwo.gameObject);
            _spawnedCatTwo = null;
        }
    }

    private void ResolvePlayerHitTracker()
    {
        if (playerHitTracker != null)
        {
            return;
        }

        playerHitTracker = FindFirstObjectByType<PlayerArrowHitTracker>();
        if (playerHitTracker == null)
        {
            Debug.LogWarning("CombatPuzzle could not find PlayerArrowHitTracker. Player death will not reset the puzzle.", this);
        }
    }

    private void SubscribeToPlayerDeath()
    {
        if (_subscribedToPlayerDeath || playerHitTracker == null)
        {
            return;
        }

        playerHitTracker.PlayerDied += HandlePlayerDied;
        _subscribedToPlayerDeath = true;
    }

    private void UnsubscribeFromPlayerDeath()
    {
        if (!_subscribedToPlayerDeath || playerHitTracker == null)
        {
            return;
        }

        playerHitTracker.PlayerDied -= HandlePlayerDied;
        _subscribedToPlayerDeath = false;
    }

    private void HandlePlayerDied()
    {
        if (!IsPuzzleRunning)
        {
            return;
        }

        StopPuzzle();
    }

    private void HandleSpawnedCatDied(BattleCat cat, bool killedByPlayerArrow)
    {
        if (!IsPuzzleRunning || !killedByPlayerArrow)
        {
            return;
        }

        _catsKilledByPlayerArrows++;
        if (_catsKilledByPlayerArrows < 2)
        {
            return;
        }

        CompletePuzzleSuccess("CombatPuzzle success: both cats were defeated by player arrows.");
    }
}
