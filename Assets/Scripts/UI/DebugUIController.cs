using System.Data;
using FishyUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class DebugUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private TextMeshProUGUI deathTimerText;
    [SerializeField] private TextMeshProUGUI checkpointText;

    void OnEnable()
    {
        FishyEvents.OnFishyMoveStateChanged += UpdateState;
        FishyEvents.OnDeathTimerChanged += UpdateDeathTimer;
        FishyEvents.OnCheckpointChanged += UpdateCheckpoint;
    }

    void OnDisable()
    {
        FishyEvents.OnFishyMoveStateChanged -= UpdateState;
        FishyEvents.OnDeathTimerChanged -= UpdateDeathTimer;
        FishyEvents.OnCheckpointChanged -= UpdateCheckpoint;
    }
    
    private void UpdateState(FishyStates state)
    {
        stateText.text = state.ToString();
    }
    
    private void UpdateDeathTimer(int time)
    {
        deathTimerText.text = time.ToString();
    }
    
    private void UpdateCheckpoint(int index)
    {
        checkpointText.text = index.ToString();
    }
}
