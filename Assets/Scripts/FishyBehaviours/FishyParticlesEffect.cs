using FishyUtilities;
using UnityEngine;

public class FishyParticlesEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem bubblesUp;
    
    private void OnEnable()
    {
        FishyEvents.OnFishyMoveStateChanged += HandleStateChanged;
    }
    
    private void OnDisable()
    {
        FishyEvents.OnFishyMoveStateChanged -= HandleStateChanged;
    }
    
    private void HandleStateChanged(FishyStates newState)
    {
        if (newState == FishyStates.InWater)
        {
            bubblesUp.Play();
        }
        else
        {
            bubblesUp.Stop();
        }
    }
}
