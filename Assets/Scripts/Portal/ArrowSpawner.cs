using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ArrowType
{
    Basic,
    Explode,
    Bounce,
    Grapple
}

[System.Serializable]
public struct ArrowTypePrefabEntry
{
    public ArrowType arrowType;
    public GameObject prefab;
    public bool unlockedAtStart;
}



public class ArrowSpawner : MonoBehaviour
{
    [Header("Arrow Prefabs")]
    public GameObject arrow;
    [SerializeField] private ArrowTypePrefabEntry[] arrowPrefabs;
    [SerializeField] private ArrowType defaultArrowType = ArrowType.Basic;

    public GameObject notch;
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _bow;
    private bool _arrowNotched = false;
    private GameObject _currentArrow = null;
    private Coroutine _spawnRoutine;

    private readonly Dictionary<ArrowType, GameObject> _prefabByType = new Dictionary<ArrowType, GameObject>();
    private readonly HashSet<ArrowType> _unlockedTypes = new HashSet<ArrowType>();
    private ArrowType _selectedArrowType;


    private void Awake()
    {
        BuildArrowTypeLookup();
        _selectedArrowType = defaultArrowType;
        EnsureSelectionIsUnlocked();
    }

    private void Start()
    {
        _bow = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (_bow == null)
        {
            Debug.LogError($"ArrowSpawner on {gameObject.name} requires an XRGrabInteractable.", this);
            enabled = false;
            return;
        }

        PullInteraction.pullActionReleased += NotchEmpty;
    }

    private void OnDestroy()
    {
        PullInteraction.pullActionReleased -= NotchEmpty;
    }

    void Update() {
        if (_bow.isSelected && _arrowNotched == false) {
            _arrowNotched = true;
            _spawnRoutine = StartCoroutine(DelayedSpawn());
        }

        if (!_bow.isSelected && _currentArrow != null) {
            Destroy(_currentArrow);
            NotchEmpty(1f);
        }
    }

    private void NotchEmpty(float value) {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        _arrowNotched = false;
        _currentArrow = null;
    }

    IEnumerator DelayedSpawn() {
        yield return new WaitForSeconds(1f);
        _spawnRoutine = null;

        GameObject prefab = GetSelectedArrowPrefab();
        if (prefab == null || notch == null)
        {
            yield break;
        }

        _currentArrow = Instantiate(prefab, notch.transform);
    }

    public bool SelectArrowType(ArrowType type)
    {
        if (!_unlockedTypes.Contains(type))
        {
            return false;
        }

        _selectedArrowType = type;
        RefreshNotchedArrowVisual();
        return true;
    }

    public void UnlockArrowType(ArrowType type)
    {
        _unlockedTypes.Add(type);
    }

    // UI helper methods (easier to wire in inspector Button onClick).
    public void SelectBasicArrow() => SelectArrowType(ArrowType.Basic);
    public void SelectExplodeArrow() => SelectArrowType(ArrowType.Explode);
    public void SelectBounceArrow() => SelectArrowType(ArrowType.Bounce);
    public void SelectGrappleArrow() => SelectArrowType(ArrowType.Grapple);

    private void RefreshNotchedArrowVisual()
    {
        if (_currentArrow == null || !_arrowNotched)
        {
            return;
        }

        Destroy(_currentArrow);
        _currentArrow = null;

        GameObject prefab = GetSelectedArrowPrefab();
        if (prefab != null && notch != null)
        {
            _currentArrow = Instantiate(prefab, notch.transform);
        }
    }

    private GameObject GetSelectedArrowPrefab()
    {
        if (_prefabByType.TryGetValue(_selectedArrowType, out GameObject prefab) && prefab != null)
        {
            return prefab;
        }

        return arrow;
    }

    private void BuildArrowTypeLookup()
    {
        _prefabByType.Clear();
        _unlockedTypes.Clear();

        if (arrow != null)
        {
            _prefabByType[ArrowType.Basic] = arrow;
            _unlockedTypes.Add(ArrowType.Basic);
        }

        if (arrowPrefabs == null)
        {
            return;
        }

        foreach (ArrowTypePrefabEntry entry in arrowPrefabs)
        {
            if (entry.prefab != null)
            {
                _prefabByType[entry.arrowType] = entry.prefab;
            }

            if (entry.unlockedAtStart)
            {
                _unlockedTypes.Add(entry.arrowType);
            }
        }
    }

    private void EnsureSelectionIsUnlocked()
    {
        if (_unlockedTypes.Contains(_selectedArrowType))
        {
            return;
        }

        foreach (ArrowType type in _unlockedTypes)
        {
            _selectedArrowType = type;
            return;
        }

        _selectedArrowType = ArrowType.Basic;
        _unlockedTypes.Add(_selectedArrowType);
    }

}
