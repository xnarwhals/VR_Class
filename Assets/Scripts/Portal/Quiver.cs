using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
public class Quiver : MonoBehaviour
{
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] private float cooldownBeforeNextArrow = 10f;

    float lastSpawnTime = -999f;
    private XRDirectInteractor cachedLeftInteractor;
    private XRDirectInteractor cachedRightInteractor;

    private void Awake()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("Arrow Prefab is not assigned in the Quiver script.");
        }


        CacheDirectInteractors();
    }

    private void OnTriggerStay(Collider other)
    {
        
        XRDirectInteractor interactor = GetCachedInteractor(other);
        if (interactor == null)
        {
            Debug.Log($"Quiver: GetCachedInteractor returned null for {other.name}");
            return;
        }

        TrySpawnAndGrab(interactor);
    }

    private void TrySpawnAndGrab(XRDirectInteractor interactor)
    {
        // dont spawn if already holding an arrow
        if (interactor.hasSelection)
        {
            return;
        }


        if (!IsSelectPressed(interactor))
        {
            Debug.Log($"Quiver: Select not pressed - aborting");
            return;
        }

        if (Time.time - lastSpawnTime < cooldownBeforeNextArrow)
        {
            Debug.Log($"Quiver: cooldown active ({Time.time - lastSpawnTime:0.00}s/{cooldownBeforeNextArrow:0.00}s).", this);
            return;
        }

        Transform attach = interactor.attachTransform != null ? interactor.attachTransform : interactor.transform;

        GameObject spawnedArrow = Instantiate(arrowPrefab, attach.position, attach.rotation, null);
        lastSpawnTime = Time.time;

        XRGrabInteractable grab = spawnedArrow.GetComponent<XRGrabInteractable>();
        if (grab != null && interactor.interactionManager != null)
        {
            interactor.interactionManager.SelectEnter((IXRSelectInteractor)interactor, (IXRSelectInteractable)grab);
        }
    }

    private static bool IsSelectPressed(XRDirectInteractor interactor)
    {
        if (interactor.isSelectActive)
        {
            return true;
        }

        var selectInput = interactor.selectInput;
        bool wasPerformed = selectInput.ReadWasPerformedThisFrame();
        bool isPerformed = selectInput.ReadIsPerformed();
        return wasPerformed || isPerformed;
    }

    private void CacheDirectInteractors()
    {
        XRDirectInteractor[] interactors = FindObjectsOfType<XRDirectInteractor>();
        foreach (XRDirectInteractor interactor in interactors)
        {
            if (interactor.CompareTag("LeftHand") || interactor.name.Contains("Left"))
            {
                cachedLeftInteractor = interactor;
            }
            else if (interactor.CompareTag("RightHand") || interactor.name.Contains("Right"))
            {
                cachedRightInteractor = interactor;
            }
        }
    }
    private XRDirectInteractor GetCachedInteractor(Collider other)
    {
        // Check if the collider belongs to a cached interactor
        if (cachedLeftInteractor != null && IsColliderPartOfInteractor(other, cachedLeftInteractor))
        {
            return cachedLeftInteractor;
        }

        if (cachedRightInteractor != null && IsColliderPartOfInteractor(other, cachedRightInteractor))
        {
            return cachedRightInteractor;
        }

        return null;
    }

    private bool IsColliderPartOfInteractor(Collider collider, XRDirectInteractor interactor)
    {
        // Check if collider is on the interactor or its children
        return collider.GetComponentInParent<XRDirectInteractor>() == interactor ||
               interactor.GetComponentInChildren<Collider>() == collider;
    }

}
