using UnityEngine;

namespace FishyUtilities
{
    public enum FishyStates
    {
        OnGround,
        InWater,
        InAir,
        OnSurface
    }

    public enum FishyJumpState
    {
        NotJumping,
        JumpingFromGround,
        JumpingFromWater
    }
}
