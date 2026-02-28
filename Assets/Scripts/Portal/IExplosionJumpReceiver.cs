using UnityEngine;

public interface IExplosionJumpReceiver
{
    void ApplyRocketJump(float impulse, Vector3 explosionPoint);
}
