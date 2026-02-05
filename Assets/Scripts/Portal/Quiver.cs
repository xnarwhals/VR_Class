using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
public class Quiver : MonoBehaviour
{
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] private float cooldownBeforeNextArrow = 3f;

    GameObject currentArrow;
    float lastSpawnTime = -999f;
    XRDirectInteractor currentInteractor;

    private void Awake()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("Arrow Prefab is not assigned in the Quiver script.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        XRDirectInteractor interactor = other.GetComponentInParent<XRDirectInteractor>();
        if (interactor == null)
        {
            return;
        }

        currentInteractor = interactor;
        TrySpawnAndGrab(interactor);
    }

    private void OnTriggerExit(Collider other)
    {
        XRDirectInteractor interactor = other.GetComponentInParent<XRDirectInteractor>();
        if (interactor == null)
        {
            return;
        }

        if (currentInteractor == interactor)
        {
            currentInteractor = null;
        }
    }

    private void Update()
    {
        if (currentInteractor == null)
        {
            return;
        }

        TrySpawnAndGrab(currentInteractor);
    }

    private void TrySpawnAndGrab(XRDirectInteractor interactor)
    {
        if (!IsSelectPressed(interactor))
        {
            return;
        }

        if (Time.time - lastSpawnTime < cooldownBeforeNextArrow)
        {
            return;
        }

        Transform attach = interactor.attachTransform != null ? interactor.attachTransform : interactor.transform;

        if (currentArrow == null)
        {
            currentArrow = Instantiate(arrowPrefab, attach.position, attach.rotation, null);
            lastSpawnTime = Time.time;
            XRGrabInteractable grab = currentArrow.GetComponent<XRGrabInteractable>();
            if (grab != null && interactor.interactionManager != null)
            {
                interactor.interactionManager.SelectEnter((IXRSelectInteractor)interactor, (IXRSelectInteractable)grab);
            }
            else if (grab == null)
            {
                Debug.LogWarning("Spawned arrow is missing XRGrabInteractable.", currentArrow);
            }
        }
    }

    private static bool IsSelectPressed(XRDirectInteractor interactor)
    {
        if (interactor.isSelectActive)
        {
            return true;
        }

        var selectInput = interactor.selectInput;
        return selectInput.ReadWasPerformedThisFrame() || selectInput.ReadIsPerformed();
    }
}
