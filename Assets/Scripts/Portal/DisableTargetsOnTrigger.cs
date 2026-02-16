using UnityEngine;

public class DisableTargetsOnTrigger : MonoBehaviour
{
    [SerializeField] private Collider bullseyeCollider;
    [SerializeField] private FlipTarget flipTarget;
    [SerializeField] private HingeJoint hingeJointToDisable;
    [SerializeField] private string playerTag = "Player";

    private Rigidbody hingeBody;
    private RigidbodyConstraints originalConstraints;
    private bool originalIsKinematic;
    private bool cachedHingeBodyState;
    
    private void Awake()
    {
        if (flipTarget == null) {
            Debug.LogError("FlipTarget is not assigned in DisableTargetsOnTrigger component.");
        }

        if (hingeJointToDisable == null && flipTarget != null)
        {
            hingeJointToDisable = flipTarget.GetComponent<HingeJoint>();
        }

        if (hingeJointToDisable != null)
        {
            hingeBody = hingeJointToDisable.GetComponent<Rigidbody>();
            if (hingeBody != null)
            {
                originalConstraints = hingeBody.constraints;
                originalIsKinematic = hingeBody.isKinematic;
                cachedHingeBodyState = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerCollider(other))
        {
            return;
        }

        if (bullseyeCollider != null)
        {
            bullseyeCollider.enabled = false;
        }

        flipTarget.SetBullseyeEmissionOff();

        FreezeHingeBody();

    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerCollider(other))
        {
            return;
        }

        if (bullseyeCollider != null)
        {
            bullseyeCollider.enabled = true;
        }
        flipTarget.RestoreBullseyeEmission();
        UnfreezeHingeBody();
    }

    private bool IsPlayerCollider(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.CompareTag(playerTag))
        {
            return true;
        }

        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(playerTag))
        {
            return true;
        }

        return other.transform.root.CompareTag(playerTag);
    }

    private void FreezeHingeBody()
    {
        if (hingeBody == null)
        {
            return;
        }

        hingeBody.isKinematic = true;
        hingeBody.constraints = RigidbodyConstraints.FreezeAll;
        hingeBody.linearVelocity = Vector3.zero;
        hingeBody.angularVelocity = Vector3.zero;
    }

    private void UnfreezeHingeBody()
    {
        if (hingeBody == null || !cachedHingeBodyState)
        {
            return;
        }

        hingeBody.isKinematic = originalIsKinematic;
        hingeBody.constraints = originalConstraints;
    }
}
