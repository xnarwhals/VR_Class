using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class Sprint : MonoBehaviour
{
    public float sprintMultiplier = 2f;
    [SerializeField] private InputActionProperty thumbstickClickAction;

    private ContinuousMoveProvider moveProvider;
    private float baseMoveSpeed;
    private bool sprintEnabled;
    private bool enabledActionLocally;

    private void Awake()
    {
        moveProvider = GetComponent<ContinuousMoveProvider>();
        if (moveProvider == null)
        {
            Debug.LogError($"Sprint component on {gameObject.name} requires a ContinuousMoveProvider.", this);
            enabled = false;
            return;
        }

        baseMoveSpeed = moveProvider.moveSpeed;

        if (thumbstickClickAction.action != null &&
            thumbstickClickAction.action.type != InputActionType.Button)
        {
            Debug.LogWarning(
                "Sprint expects a Button action (bind to <XRController>{LeftHand}/primary2DAxisClick).",
                this);
        }
    }

    private void OnEnable()
    {
        if (thumbstickClickAction.action == null)
        {
            return;
        }

        thumbstickClickAction.action.performed += OnThumbstickClicked;

        if (!thumbstickClickAction.action.enabled)
        {
            thumbstickClickAction.action.Enable();
            enabledActionLocally = true;
        }
    }

    private void OnDisable()
    {
        if (thumbstickClickAction.action != null)
        {
            thumbstickClickAction.action.performed -= OnThumbstickClicked;

            if (enabledActionLocally)
            {
                thumbstickClickAction.action.Disable();
                enabledActionLocally = false;
            }
        }

        sprintEnabled = false;
        if (moveProvider != null)
        {
            moveProvider.moveSpeed = sprintEnabled ? baseMoveSpeed * sprintMultiplier : baseMoveSpeed;
        }
    }

    private void OnThumbstickClicked(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        sprintEnabled = !sprintEnabled;
        moveProvider.moveSpeed = sprintEnabled ? baseMoveSpeed * sprintMultiplier : baseMoveSpeed;
    }
}
