using System;
using FishyUtilities;
using UnityEngine;


public static class FishyEvents
{
    public static event Action<FishyStates> OnFishyMoveStateChanged;
  
    public static Action<int> OnDeathTimerChanged;
  
    public static Action<int> OnCheckpointChanged;
    public static Action LastCheckpointLoaded;
    
    public static Action<Vector3> OnCamSnapZoneEntered;
    public static Action OnCamSnapZoneExit;

    public static Action<Vector3> OnWaterEntered;
    
    public static Action OnMovingWaterStart;
    public static Action OnMovingWaterEnd;
    
    public static Action OnSurfaceReachedFromUnderWater;
    public static Action OnSurfaceReachedFromAir;

    public static Action OnUnderwaterEnter;
    public static Action OnUnderwaterExit;

    public static Action<Vector3> OnJumpFromWater;

    public static Action OnWateringPlantStart;
    public static Action OnWateringPlantEnd;

    private static FishyStates lastState;
    
    public static void SetState(FishyStates newState)
    {
        if (newState == lastState) return;
        
        OnFishyMoveStateChanged?.Invoke(newState);

        if (newState == FishyStates.InWater)
        {
            OnUnderwaterEnter?.Invoke();
        }
        else
        {
            if (lastState == FishyStates.InWater)
            {
                OnUnderwaterExit?.Invoke();
            }
        }

        if (newState == FishyStates.OnSurface && lastState == FishyStates.InWater)
        {
            OnSurfaceReachedFromUnderWater?.Invoke();
        }

        if (newState == FishyStates.OnSurface && lastState == FishyStates.InAir)
        {
            OnSurfaceReachedFromAir?.Invoke();
        }
        
        lastState = newState;
    }
}