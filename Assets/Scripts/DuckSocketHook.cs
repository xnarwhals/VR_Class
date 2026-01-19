using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor))]
public class DuckSocketHook : MonoBehaviour
{
    [SerializeField] private DuckSlot duckSlot;
    [SerializeField] private DuckPuzzleBase puzzle;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    private void Awake()
    {
        socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
    }

    private void OnEnable()
    {
        socket.selectEntered.AddListener(OnDuckPlaced);
        socket.selectExited.AddListener(OnDuckRemoved);
    }

    private void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnDuckPlaced);
        socket.selectExited.RemoveListener(OnDuckRemoved);
    }

    private void OnDuckPlaced(SelectEnterEventArgs args)
    {
        var duck = args.interactableObject.transform.GetComponent<DuckID>();
        if (duck != null)
        {
            duckSlot.SetDuck(duck);
            puzzle.NotifySlotChanged();
        }
    }

    private void OnDuckRemoved(SelectExitEventArgs args)
    {
        var duck = args.interactableObject.transform.GetComponent<DuckID>();
        if (duck != null)
        {
            duckSlot.Clear();
            puzzle.NotifySlotChanged();
        }
    }
}
