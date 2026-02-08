using UnityEngine;
using System.Collections;



public class ArrowSpawner : MonoBehaviour
{
    public GameObject arrow;
    public GameObject notch;
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _bow;
    private bool _arrowNotched = false;
    private GameObject _currentArrow = null;


    private void Start()
    {
        _bow = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        PullInteraction.pullActionReleased += NotchEmpty;
    }

    private void OnDestroy()
    {
        PullInteraction.pullActionReleased -= NotchEmpty;
    }

    void Update() {
        if (_bow.isSelected && _arrowNotched == false) {
            _arrowNotched = true;
            StartCoroutine("DelayedSpawn");
        }

        if (!_bow.isSelected && _currentArrow != null) {
            Destroy(_currentArrow);
            NotchEmpty(1f);
        }
    }

    private void NotchEmpty(float value) {
        _arrowNotched = false;
        _currentArrow = null;
    }

    IEnumerator DelayedSpawn() {
        yield return new WaitForSeconds(1f);
        _currentArrow = Instantiate(arrow, notch.transform);
    }


}
