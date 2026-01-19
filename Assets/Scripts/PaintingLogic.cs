using UnityEngine;


public class PaintingLogic : MonoBehaviour
{
    [Header("Key Setup")]
    [SerializeField] private Transform keyTransform;
    [SerializeField] private Rigidbody keyRigidbody;
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable keyGrabInteractable;
    [SerializeField] private Collider[] keyColliders;

    [Header("Flip Detection")]
    [SerializeField] private float flipDotThreshold = 0f;

    private bool revealed;

    private void Awake()
    {
        if (keyTransform == null)
            keyTransform = transform.Find("Key");

        if (keyTransform != null)
        {
            if (keyRigidbody == null)
                keyRigidbody = keyTransform.GetComponent<Rigidbody>();
            if (keyGrabInteractable == null)
                keyGrabInteractable = keyTransform.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (keyColliders == null || keyColliders.Length == 0)
                keyColliders = keyTransform.GetComponentsInChildren<Collider>(true);
        }
    }

    private void Start()
    {
        SetKeyActive(false);
    }

    private void Update()
    {
        if (revealed || keyTransform == null)
            return;

        if (IsFlipped())
            RevealKey();
    }

    private bool IsFlipped()
    {
        return Vector3.Dot(transform.up, Vector3.up) < flipDotThreshold;
    }

    private void RevealKey()
    {
        revealed = true;
        SetKeyActive(true);
        keyTransform.SetParent(null, true);
    }

    private void SetKeyActive(bool active)
    {
        if (keyGrabInteractable != null)
            keyGrabInteractable.enabled = active;

        if (keyRigidbody != null)
        {
            keyRigidbody.isKinematic = !active;
            keyRigidbody.useGravity = active;
        }

        if (keyColliders != null)
        {
            foreach (var col in keyColliders)
            {
                if (col != null)
                    col.enabled = active;
            }
        }
    }
}
