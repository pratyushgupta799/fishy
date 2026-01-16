using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private AudioSource MusicIntroSource;
    [SerializeField] private AudioSource MusicLoopSource;

    [Header("Variations")] 
    [SerializeField] private float maxVolume;
    [SerializeField] private float minVolume;
    [SerializeField] private float maxPitch;
    [SerializeField] private float minPitch;

    [Header("SFX")]
    [SerializeField] private AudioClip fishSwimLoop;
    [SerializeField] private AudioClip glassFall;
    [SerializeField] private AudioClip glassFall2;
    [SerializeField] private AudioClip surfaceNew;
    [SerializeField] private AudioClip underwater;
    [SerializeField] private AudioClip waterJump;
    [SerializeField] private AudioClip waterLand;
    [SerializeField] private AudioClip waterSpill;
    [SerializeField] private AudioClip wateringPlant;
    
    [Header("Music")]
    [SerializeField] private AudioClip musicIntro;
    [SerializeField] private AudioClip musicLoop;
    
    [Header("Pool")]
    [SerializeField] private AudioSource audioSourcePrefab;
    [SerializeField] private int initialPoolSize = 10;

    private readonly List<AudioSource> pool = new();
    
    [Header("Attributes")]
    [SerializeField] private float stopWaterSwimDelay = 0.5f;
    [SerializeField] private float waterFadeOutTime = 0.5f;
    
    private Coroutine fadeRoutine;
    private Coroutine swimStopRoutine;
    
    private AudioSource underwaterAudioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateSource();
        }
    }

    private void OnEnable()
    {
        FishyEvents.OnWaterEntered += PlayWaterSplashSound;
        FishyEvents.OnMovingWaterStart += StartWaterSwimLoop;
        FishyEvents.OnMovingWaterEnd += StopWaterSwimLoop;
        FishyEvents.OnUnderwaterEnter += PlayUnderWaterLoop;
        FishyEvents.OnUnderwaterExit += StopUnderWaterLoop;
        FishyEvents.OnJumpFromWater += PlayJumpFromWater;
        FishyEvents.OnSurfaceReachedFromUnderWater += PlaySurfaceNew;
    }

    private void OnDisable()
    {
        FishyEvents.OnWaterEntered -= PlayWaterSplashSound;
        FishyEvents.OnMovingWaterStart -= StartWaterSwimLoop;
        FishyEvents.OnMovingWaterEnd -= StopWaterSwimLoop;
        FishyEvents.OnUnderwaterEnter -= PlayUnderWaterLoop;
        FishyEvents.OnUnderwaterExit -= StopUnderWaterLoop;
        FishyEvents.OnJumpFromWater -= PlayJumpFromWater;
        FishyEvents.OnSurfaceReachedFromUnderWater -= PlaySurfaceNew;
    }

    private void Start()
    {
        StartMusic();
    }

    private void StartMusic()
    {
        PlayIntroThenLoop();
    }
    
    private void PlayIntroThenLoop()
    {
        double dspStart = AudioSettings.dspTime + 0.1f;
        
        MusicIntroSource.clip = musicIntro;
        MusicIntroSource.loop = false;
        MusicIntroSource.PlayScheduled(dspStart);
        
        MusicLoopSource.clip = musicLoop;
        MusicLoopSource.loop = true;
        MusicLoopSource.PlayScheduled(dspStart + musicIntro.length);
    }

    private void PlayWaterSplashSound(Vector3 position)
    {
        // Debug.Log("Water splash sound played");
        var audioSource = AudioSourceVariate();
        audioSource.spatialBlend = 0f;
        audioSource.PlayOneShot(waterLand);
    }

    private void PlayUnderWaterLoop()
    {
        if (underwaterAudioSource == null)
        {
            // Debug.Log("Underwater sound played");
            underwaterAudioSource = AudioSourceVariate();
            underwaterAudioSource.loop = true;
            underwaterAudioSource.spatialBlend = 0f;
            underwaterAudioSource.clip = underwater;
            underwaterAudioSource.Play();
        }
    }

    private void PlayJumpFromWater()
    {
        var audioSource = AudioSourceVariate();
        audioSource.spatialBlend = 0f;
        audioSource.PlayOneShot(waterJump);
    }

    private void StopUnderWaterLoop()
    {
        if (underwaterAudioSource != null)
        {
            Debug.Log("Underwater sound stopped");
            underwaterAudioSource.Stop();
            underwaterAudioSource = null;
        }
    }

    private void StartWaterSwimLoop()
    {
        if (!SFXSource.isPlaying)
        {
            SFXSource.loop = true;
            SFXSource.clip = fishSwimLoop;
            SFXSource.Play();
        }
        if (swimStopRoutine != null)
        {
            StopCoroutine(swimStopRoutine);
            swimStopRoutine = null;
        }
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
    }
    
    private void StopWaterSwimLoop()
    {
        if (swimStopRoutine == null)
        {
            swimStopRoutine = StartCoroutine(StopWaterSwimLoopGrace());
        }
    }

    private void PlaySurfaceNew()
    {
        var audioSource = AudioSourceVariate();
        audioSource.spatialBlend = 0f;
        audioSource.PlayOneShot(surfaceNew);
    }
    
    public void AudioPlayOneShotAt(AudioClip clip, Vector3 position)
    {
        AudioSource src = GetSource();
        src.transform.position = position;
        src.spatialBlend = 1f;
        src.volume = GetVolume();
        src.pitch = GetPitch();
        src.loop = false;
        src.PlayOneShot(clip);
    }

    
    // todo: make stop and fade decoupled
    private IEnumerator StopWaterSwimLoopGrace()
    {
        float t = 0f;
        
        while (t < stopWaterSwimDelay)
        {
            t += Time.deltaTime;
            yield return null;
        }
        
        if (t >= stopWaterSwimDelay)
        {
            fadeRoutine = StartCoroutine(FadeOutWaterLoop());
        }
    }
    
    private IEnumerator FadeOutWaterLoop()
    {
        float startVol = SFXSource.volume;
        float t = 0f;

        while (t < waterFadeOutTime)
        {
            t += Time.deltaTime;
            SFXSource.volume = Mathf.Lerp(startVol, 0f, t / waterFadeOutTime);
            yield return null;
        }

        SFXSource.Stop();
        SFXSource.volume = startVol;
        SFXSource.loop = false;
    }

    #region Helpers

    private AudioSource AudioSourceVariate()
    {
        AudioSource source = GetSource();
        source.pitch = GetPitch();
        source.volume = GetVolume();
        return source;
    }

    private float GetPitch()
    {
        return Random.Range(minPitch, maxPitch);
    }

    private float GetVolume()
    {
        return Random.Range(minVolume, maxVolume);
    }

    private AudioSource CreateSource()
    {
        AudioSource source = Instantiate(audioSourcePrefab);
        source.gameObject.SetActive(false);
        pool.Add(source);
        return source;
    }

    private AudioSource GetSource()
    {
        foreach (AudioSource source in pool)
        {
            if (!source.isPlaying)
            {
                source.gameObject.SetActive(true);
                return source;
            }
        }

        return CreateSource();
    }

    #endregion
}
