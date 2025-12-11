using FishyUtilities;
using UnityEngine;


public static class FishyEvents
{
    public static System.Action<FishyStates> OnFishyMoveStateChanged;
  
    public static System.Action<int> OnDeathTimerChanged;
  
    public static System.Action<int> OnCheckpointChanged;
    public static System.Action LastCheckpointLoaded;


    public static System.Action<Vector3> OnCamSnapZoneEntered;
    public static System.Action OnCamSnapZoneExit;
}