using UnityEngine;

public class UIFollow : MonoBehaviour
{
    [Tooltip("Lower = smoother/slower. Try 0.01 for very floaty movement.")]
    public float positionSmoothSpeed = 0.03f;
    [Tooltip("Lower = smoother/slower. Try 0.01 for very floaty rotation.")]
    public float rotationSmoothSpeed = 0.03f;
    [Tooltip("Lower = smoother/slower. Try 0.01 for very floaty distance changes.")]
    public float distanceSmoothSpeed = 0.03f;
    public float distance = 2f; // Target distance from camera

    private Transform cameraTransform;
    private float initialY;
    private float currentDistance;
    private bool initialized = false;

    void Start()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            cameraTransform = mainCam.transform;
            initialY = transform.position.y;
            // Start at the initial distance from the camera
            Vector3 camToUI = transform.position - cameraTransform.position;
            camToUI.y = 0;
            currentDistance = camToUI.magnitude;
            initialized = true;
        }
        else
        {
            Debug.LogError("UIFollowCameraSmooth: No MainCamera found in scene!");
        }
    }

    void LateUpdate()
    {
        if (!initialized || cameraTransform == null || !gameObject.activeInHierarchy)
            return;

        // Smoothly update the current distance toward the target distance
        currentDistance = Mathf.Lerp(currentDistance, distance, distanceSmoothSpeed);

        // Get the camera's yaw (rotation around Y axis)
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();

        // Calculate the target position: always at a smoothly-updated distance in the XZ plane, fixed Y
        Vector3 targetPosition = cameraTransform.position + camForward * currentDistance;
        targetPosition.y = initialY; // Keep Y fixed

        // Smoothly move towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, positionSmoothSpeed);

        // Make the UI face the camera horizontally (no tilt)
        Vector3 lookAtPos = cameraTransform.position;
        lookAtPos.y = transform.position.y; // Ignore vertical difference
        Quaternion targetRotation = Quaternion.LookRotation(transform.position - lookAtPos, Vector3.up);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed);
    }
}
