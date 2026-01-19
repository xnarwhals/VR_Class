using UnityEngine;

public class DinnerDucksPuzzle : DuckPuzzleBase
{
    [Header("Fruit Spawn")]
    [SerializeField] private GameObject[] fruitPrefabs;
    [SerializeField] private Transform[] plateLocations;


    protected override void Awake()
    {
        base.Awake();
        if (fruitPrefabs == null || fruitPrefabs.Length == 0)
            Debug.LogWarning("Fruit prefabs not assigned in DinnerDucksPuzzle.");

        if (plateLocations == null || plateLocations.Length == 0)
            Debug.LogWarning("Plate locations not assigned in DinnerDucksPuzzle.");
    }

    protected override System.Collections.IEnumerator OnPuzzleCompleteSequence()
    {
        // sanity check
        if (fruitPrefabs == null || fruitPrefabs.Length < 4 || plateLocations == null || plateLocations.Length == 0)
            yield break;

        var slots = GetComponentsInChildren<DuckSlot>(true);
        var count = Mathf.Min(slots.Length, plateLocations.Length);

        for (int i = 0; i < count; i++)
        {
            var slot = slots[i];
            var duck = slot != null ? slot.CurrentDuck : null;
            var plate = plateLocations[i];
            if (duck == null || plate == null)
                continue;

            var fruitPrefab = GetFruitPrefabForColor(duck.color);
            if (fruitPrefab == null)
                continue;

            Instantiate(fruitPrefab, plate.position, plate.rotation);
            yield return new WaitForSeconds(0.5f);
        }

        // trigger duck dance in animators
        
    }

    private GameObject GetFruitPrefabForColor(DuckColor color)
    {
        switch (color)
        {
            case DuckColor.Red:
                return fruitPrefabs[0]; // Red apple
            case DuckColor.Blue:
                return fruitPrefabs[1]; // Blue apple
            case DuckColor.Green:
                return fruitPrefabs[2]; // Green apple
            case DuckColor.Yellow:
                return fruitPrefabs[3]; // Yellow apple
            default:
                return null;
        }
    }
}
