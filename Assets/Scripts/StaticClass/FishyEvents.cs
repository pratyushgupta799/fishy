using System;
using FishyUtilities;
using UnityEngine;


public static class FishyEvents
{
    public static Action<FishyStates> OnFishyMoveStateChanged;
  
    public static Action<int> OnDeathTimerChanged;
  
    public static Action<int> OnCheckpointChanged;
    public static Action LastCheckpointLoaded;
    
    public static Action<Vector3> OnCamSnapZoneEntered;
    public static Action OnCamSnapZoneExit;

    public static Action OnWaterEntered;
    
    public static Action OnMovingWaterStart;
    public static Action OnMovingWaterEnd;

    public static Action<int> OnGlassFell;
    
    public static Action OnSurfaceReachedFromUnderWater;

    public static Action OnUnderwaterEnter;
    public static Action OnUnderwaterExit;
    
    public static Action OnLandedOnGround;

    public static Action OnWaterSpilled;

    public static Action OnWaterPlanStart;
    public static Action OnWaterPlaneEnd;
}