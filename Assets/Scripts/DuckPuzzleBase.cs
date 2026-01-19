using System;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

public abstract class DuckPuzzleBase : MonoBehaviour
{
    [SerializeField] private DuckSlot[] slots;
    public bool IsComplete {get; private set;}
    public event Action<DuckPuzzleBase> Completed;
    protected abstract System.Collections.IEnumerator OnPuzzleCompleteSequence();

    protected virtual void Awake()
    {
        if (slots == null || slots.Length == 0)
            slots = GetComponentsInChildren<DuckSlot>(true);
    }


    public void NotifySlotChanged()
    {
        if (IsComplete) return;
        if (slots.All(s => s.IsCorrect))
        {
            StartCoroutine(CompletePuzzle());
        }
    }

    private System.Collections.IEnumerator CompletePuzzle()
    {
        // lock ducks
        IsComplete = true;
        GameManager.Instance?.OnPuzzleCompleted();
        foreach (var s in slots) s.CurrentDuck?.SetLocked(true);

        // free ducks and play dance sequence
        yield return OnPuzzleCompleteSequence();

        AudioManager.Instance?.PlayVictoryJingle();
        foreach (var s in slots) {
            s.CurrentDuck?.PlayDance();
            s.CurrentDuck?.SetLocked(false);
        }
        Completed?.Invoke(this);
    }
}
