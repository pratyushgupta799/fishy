using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource SFXSource;

    [SerializeField] private AudioClip waterSplashSound;

    private void OnEnable()
    {
        FishyEvents.OnWaterEntered += PlayWaterSplashSound;
    }

    private void OnDisable()
    {
        FishyEvents.OnWaterEntered -= PlayWaterSplashSound;
    }

    private void PlayWaterSplashSound()
    {
        SFXSource.PlayOneShot(waterSplashSound);
    }
}
