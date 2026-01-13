using UnityEngine;
using UnityEngine.Events;


public class DuckPuzzle : MonoBehaviour
{
    [Header("Sockets (4)")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor[] sockets;

    [SerializeField] private string[] requiredColorIds; // Yellow, Red, Blue, Green
    [SerializeField] private GameObject[] foodObjects; // 

    [Header("Events")]
    public UnityEvent OnSolved; // lock ducks to prevent unsolving 
    public bool solved = false;

    void OnEnable()
    {
        foreach (var socket in sockets)
        {
            if (socket == null) continue;
            socket.selectEntered.AddListener(_ => Evaluate());
            socket.selectExited.AddListener(_ => Evaluate());
        }
    }


    void OnDisable()
    {
        foreach (var socket in sockets)
        {
            if (socket == null) continue;
            socket.selectEntered.RemoveAllListeners();
            socket.selectExited.RemoveAllListeners();
        }
    }

    private void Evaluate()
    {
        bool allCorrect = true;
        for (int i = 0; i < sockets.Length; i++)
        {
            var socket = sockets[i];
            if (socket == null || !socket.hasSelection)
            {
                allCorrect = false;
                break;            
            }
    

            var selected = socket.firstInteractableSelected;
            if (selected == null)
            {
                allCorrect = false;
                break;
            }

            var duck = selected.transform.GetComponent<DuckID>();

            if (duck.colorId != requiredColorIds[i])
            {
                allCorrect = false;
                break;
            }


            if (allCorrect && !solved)
            {
                solved = true;
                OnSolved?.Invoke();
                SpawnFood();

                // prevent removal of ducks
                foreach (var socketToLock in sockets)
                {
                    if (socketToLock == null) continue;
                    socketToLock.interactionLayers = 0; // disable interaction
                }
            }

            
        }
    }

    private void SpawnFood()
    {
        // sound, effects, etc
        Debug.Log("Spawning food items for ducks.");
    }



}
