using System;
using System.Data;
using FishyUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public class DebugUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private TextMeshProUGUI deathTimerText;
    [SerializeField] private TextMeshProUGUI checkpointText;
    [SerializeField] private TextMeshProUGUI playerSpeedVector;
    [SerializeField] private TextMeshProUGUI playerSpeed;
    
    private Rigidbody fish;

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

    private void Update()
    {
        if (fish == null)
        {
            fish = GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<Rigidbody>();
        }
        else
        {
            playerSpeedVector.text = fish.linearVelocity.ToString();
            playerSpeed.text = fish.linearVelocity.magnitude.ToString();
        }
    }
    
    private void UpdateState(FishyStates state)
    {
        stateText.text = state.ToString();
    }
    
    private void UpdateDeathTimer(int time)
    {
        deathTimerText.SetText("{0}s", time);
    }
    
    private void UpdateCheckpoint(int index)
    {
        checkpointText.text = index.ToString();
    }
}
