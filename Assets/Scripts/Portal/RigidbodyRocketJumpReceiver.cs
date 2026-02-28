using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RigidbodyRocketJumpReceiver : MonoBehaviour, IExplosionJumpReceiver
{
    [SerializeField] private CharacterController targetController;
    [SerializeField] private bool clearDownwardVelocity = true;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedStickVelocity = -2f;

    private float _verticalVelocity;

    private void Awake()
    {
        if (targetController == null)
        {
            targetController = GetComponent<CharacterController>();
        }
    }

    private void Update()
    {
        if (targetController == null)
        {
            return;
        }

        if (targetController.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = groundedStickVelocity;
        }

        _verticalVelocity += gravity * Time.deltaTime;
        targetController.Move(Vector3.up * (_verticalVelocity * Time.deltaTime));
    }

    public void ApplyRocketJump(float impulse, Vector3 explosionPoint)
    {
        if (targetController == null || impulse <= 0f)
        {
            return;
        }

        if (clearDownwardVelocity)
        {
            if (_verticalVelocity < 0f)
            {
                _verticalVelocity = 0f;
            }
        }

        _verticalVelocity += impulse;
    }
}
