using FishyUtilities;
using UnityEngine;
using UnityEngine.Serialization;

public class FishyParticlesEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem bubblesUp;
    [SerializeField] private ParticleSystem waterSplashLand;
    [SerializeField] private ParticleSystem waterSplashJump;

    private ParticleSystem sceneWaterSplashLand;
    private ParticleSystem sceneWaterSplashJump;
    
    private void OnEnable()
    {
        sceneWaterSplashJump = Instantiate(waterSplashJump, transform.position, Quaternion.LookRotation(Vector3.up));
        sceneWaterSplashLand = Instantiate(waterSplashLand, transform.position, Quaternion.LookRotation(Vector3.up));
        FishyEvents.OnFishyMoveStateChanged += HandleStateChanged;
        FishyEvents.OnSurfaceReachedFromAir += PlayWaterSplashLand;
        FishyEvents.OnJumpFromWater += PlayWaterSplashJump;
    }
    
    private void OnDisable()
    {
        FishyEvents.OnFishyMoveStateChanged -= HandleStateChanged;
        FishyEvents.OnSurfaceReachedFromAir -= PlayWaterSplashLand;
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

    private void PlayWaterSplashLand(Vector3 position)
    {
        sceneWaterSplashLand.transform.position = position;
        sceneWaterSplashLand.Play();
    }

    private void PlayWaterSplashJump(Vector3 position)
    {
        sceneWaterSplashJump.transform.position = position;
        sceneWaterSplashJump.Play();
    }
}
