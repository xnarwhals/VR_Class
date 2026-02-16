using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// after each level the player gets this power up ball
// it unlocks a new arrow type [explode, bounce, etc]
public class GatchaBall : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider[] collidersToDisable;
    [SerializeField] private string arrowType;
    [SerializeField] private Vector3 storedLocalScale = Vector3.one;

    private bool isStored;

    public string ArrowTypeName => arrowType;

    private void Awake()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = GetComponentsInChildren<Collider>(true);
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.AddListener(OnSelected);
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.RemoveListener(OnSelected);
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (isStored)
            return;

        XRSocketInteractor socket = args.interactorObject as XRSocketInteractor;
        if (socket == null)
            return;

        StoreInSocket(socket.transform);
    }

    private void StoreInSocket(Transform socketTransform)
    {
        isStored = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (grabInteractable != null)
            grabInteractable.enabled = false;

        // Do this after disabling the interactable to prevent XR from restoring
        // the pre-socket parent.
        transform.SetParent(socketTransform, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = storedLocalScale;

        if (collidersToDisable != null)
        {
            foreach (Collider col in collidersToDisable)
            {
                if (col != null)
                    col.enabled = false;
            }
        }
    }

    public bool TryGetArrowType(out ArrowType parsedArrowType)
    {
        return System.Enum.TryParse(arrowType, true, out parsedArrowType);
    }
}
