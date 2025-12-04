using System.Data;
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
        DebugEvents.OnFishyMoveStateChanged += UpdateState;
        DebugEvents.OnDeathTimerChanged += UpdateDeathTimer;
        DebugEvents.OnCheckpointChanged += UpdateCheckpoint;
    }

    void OnDisable()
    {
        DebugEvents.OnFishyMoveStateChanged -= UpdateState;
        DebugEvents.OnDeathTimerChanged -= UpdateDeathTimer;
        DebugEvents.OnCheckpointChanged -= UpdateCheckpoint;
    }
    
    private void UpdateState(string state)
    {
        stateText.text = state;
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
