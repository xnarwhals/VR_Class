using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Pedastal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] XRGrabInteractable interactable;
    [SerializeField] Transform hoverAnchor;

    [Header("Spawn")]
    [SerializeField] GameObject pedestalPrefab;

    [Header("Rotate")]
    [SerializeField] float rotateSpeed = 30f;
    GameObject spawnedInstance;

    void Awake()
    {
        if (interactable == null)
        {
            interactable = GetComponentInChildren<XRGrabInteractable>();
        }

        if (hoverAnchor == null)
        {
            Transform existingAnchor = transform.Find("Anchor");
            hoverAnchor = existingAnchor != null ? existingAnchor : null;
        }

        if (hoverAnchor == null)
        {
            hoverAnchor = new GameObject("HoverAnchor").transform;
            hoverAnchor.SetParent(transform, false);
            hoverAnchor.localPosition = Vector3.up * 0.15f;
            hoverAnchor.localRotation = Quaternion.identity;
        }
    }

    void Start()
    {
        if (pedestalPrefab != null && spawnedInstance == null)
        {
            spawnedInstance = Instantiate(pedestalPrefab, hoverAnchor.position, hoverAnchor.rotation, null);
            interactable = spawnedInstance.GetComponentInChildren<XRGrabInteractable>();
        }
    }

    void Update()
    {
        if (interactable == null || hoverAnchor == null)
        {
            return;
        }
        interactable.transform.position = hoverAnchor.position;

        if (rotateSpeed > 0f)
        {
            interactable.transform.Rotate(hoverAnchor.up, rotateSpeed * Time.deltaTime, Space.World);
        }
    }
}
