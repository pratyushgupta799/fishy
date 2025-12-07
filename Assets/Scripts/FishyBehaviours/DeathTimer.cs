using FishyUtilities;
using UnityEngine;

public class DeathTimer : MonoBehaviour
{
    [SerializeField] private int deathTime = 5;
    [SerializeField] private int foodTime = 2;

    private float currentDeathTimer;
    
    private FishyStates curFishyState;
    void Start()
    {
        FishyEvents.OnFishyMoveStateChanged += SetFishyState;
        FishyEvents.LastCheckpointLoaded += ResetDeathTimer;
        currentDeathTimer = deathTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(curFishyState == FishyStates.OnSurface || curFishyState == FishyStates.InWater)
        {
            currentDeathTimer = deathTime;
        }
        else if(curFishyState == FishyStates.OnGround)
        {
            currentDeathTimer -= Time.deltaTime;
        }
        else if(curFishyState == FishyStates.InAir)
        {
            currentDeathTimer -= Time.deltaTime * 2;
        }

        if (currentDeathTimer <= 0)
        {
            CheckPointManager.Instance.LoadLastCheckpoint();
        }
        
        FishyEvents.OnDeathTimerChanged?.Invoke((int)currentDeathTimer);
    }

    private void SetFishyState(FishyStates state)
    {
        curFishyState = state;
    }

    public void ResetDeathTimer()
    {
        currentDeathTimer = deathTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Health"))
        {
            currentDeathTimer += foodTime;
        }
    }
}
