using UnityEngine;

public class DuckSlot : MonoBehaviour
{
    [SerializeField] private DuckColor expected;
    public DuckColor Expected => expected;
    public DuckID CurrentDuck { get; private set; }
    public bool IsCorrect => CurrentDuck != null && CurrentDuck.color == expected;

    public void SetDuck(DuckID duck)
    {
        CurrentDuck = duck;
    }
    public void Clear()
    {
        CurrentDuck = null;
    }
}
