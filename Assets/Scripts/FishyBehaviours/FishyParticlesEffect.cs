using FishyUtilities;
using UnityEngine;

public class FishyParticlesEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem bubblesUp;
    [SerializeField] private ParticleSystem waterSplash;

    private ParticleSystem sceneWaterSplash;
    
    private void OnEnable()
    {
        sceneWaterSplash = Instantiate(waterSplash, transform.position, Quaternion.LookRotation(Vector3.up));
        FishyEvents.OnFishyMoveStateChanged += HandleStateChanged;
        FishyEvents.OnWaterEntered += PlayWaterSplash;
    }
    
    private void OnDisable()
    {
        FishyEvents.OnFishyMoveStateChanged -= HandleStateChanged;
        FishyEvents.OnWaterEntered -= PlayWaterSplash;
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

    private void PlayWaterSplash(Vector3 position)
    {
        sceneWaterSplash.transform.position = position;
        sceneWaterSplash.Play();
    }
}
