using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class Sprint : MonoBehaviour
{
    public float sprintMultiplier = 2f;
    [SerializeField] private InputActionProperty leftStickClickAction;

    private ContinuousMoveProvider moveProvider;
    private float baseMoveSpeed;
    private bool sprintEnabled;

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

        if (leftStickClickAction.action != null &&
            leftStickClickAction.action.type != InputActionType.Button)
        {
            Debug.LogWarning(
                "Sprint expects a Button action (bind to <XRController>{LeftHand}/primary2DAxisClick).",
                this);
        }
    }

    private void Update()
    {
        if (leftStickClickAction.action != null && leftStickClickAction.action.WasPressedThisFrame())
        {
            sprintEnabled = !sprintEnabled;
        }

        moveProvider.moveSpeed = sprintEnabled ? baseMoveSpeed * sprintMultiplier : baseMoveSpeed;
    }
}
