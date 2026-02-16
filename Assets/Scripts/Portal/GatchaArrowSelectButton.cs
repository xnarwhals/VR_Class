using UnityEngine;

public class GatchaArrowSelectButton : MonoBehaviour
{
    [SerializeField] private Transform socketRoot;
    [SerializeField] private ArrowSpawner arrowSpawner;

    // Hook this method to a UI Button onClick.
    public void SelectArrowFromSocket()
    {
        if (socketRoot == null || arrowSpawner == null)
        {
            Debug.LogWarning("GatchaArrowSelectButton is missing socketRoot or arrowSpawner reference.", this);
            return;
        }

        GatchaBall socketBall = socketRoot.GetComponentInChildren<GatchaBall>(true);
        if (socketBall == null)
        {
            Debug.Log("No gatcha ball found in this socket.", this);
            return;
        }

        if (!socketBall.TryGetArrowType(out ArrowType arrowType))
        {
            Debug.LogWarning($"Invalid arrowType '{socketBall.ArrowTypeName}' on {socketBall.name}.", socketBall);
            return;
        }

        arrowSpawner.UnlockArrowType(arrowType);
        bool selected = arrowSpawner.SelectArrowType(arrowType);
        if (!selected)
        {
            Debug.LogWarning($"Arrow type '{arrowType}' is not selectable.", this);
        }
    }
}
