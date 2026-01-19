using UnityEngine;

public class BedroomDucks : DuckPuzzleBase
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override System.Collections.IEnumerator OnPuzzleCompleteSequence()
    {
        // Bedroom Ducks puzzle completion logic can be implemented here.
        yield return null;
    }
}
