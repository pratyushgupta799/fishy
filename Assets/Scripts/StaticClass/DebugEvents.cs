using UnityEngine;

public static class DebugEvents
{
    public static System.Action<string> OnFishyMoveStateChanged;
    public static System.Action<int> OnDeathTimerChanged;
    public static System.Action<int> OnCheckpointChanged;
}
