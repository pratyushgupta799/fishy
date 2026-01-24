using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuddleManager : MonoBehaviour
{
    [SerializeField] private PuddleBehaviour puddleWater;
    [SerializeField] private PuddleBehaviour bigPuddleWater;
    private PuddleBehaviour[] bigPuddles;
    private PuddleBehaviour[] puddleWaters;
    private PuddleBehaviour[] spillPuddles;

    private int activeSpillPuddles = 0;
    
    public static PuddleManager Instance;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        
        puddleWaters = new PuddleBehaviour[8];
        spillPuddles = new PuddleBehaviour[4];
        bigPuddles = new PuddleBehaviour[2];
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        for (int i = 0; i < puddleWaters.Length; i++)
        {
            puddleWaters[i] = Instantiate(puddleWater);
            puddleWaters[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < spillPuddles.Length; i++)
        {
            spillPuddles[i] = Instantiate(puddleWater);
            spillPuddles[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < bigPuddles.Length; i++)
        {
            bigPuddles[i] = Instantiate(bigPuddleWater);
            bigPuddles[i].gameObject.SetActive(false);
        }
    }

    public void RaiseEvapouratablePuddle(Vector3 pos, float raiseTime, float raiseHeight, float evaporateTime, float evaporateScale)
    {
        for (int i = 0; i < puddleWaters.Length; i++)
        {
            if(puddleWaters[i].gameObject.activeSelf == false)
            {
                puddleWaters[i].transform.position = pos;
                puddleWaters[i].Raise(raiseTime, raiseHeight);
                puddleWaters[i].SetEvaporate(evaporateTime, evaporateScale);
                break;
            }
        }
    }

    public void GetSpillPuddle()
    {
        for (int i = 0; i < spillPuddles.Length; i++)
        {
            spillPuddles[i].gameObject.SetActive(false);
            activeSpillPuddles--;
        }
    }

    public void RaiseBigPuddle(Vector3 pos, float raiseTime, float raiseHeight)
    {
        for (int i = 0; i < bigPuddles.Length; i++)
        {
            if (bigPuddles[i].gameObject.activeSelf == false)
            {
                bigPuddles[i].transform.position = pos;
                bigPuddles[i].Raise(raiseTime, raiseHeight);
                break;
            }
        }
    }

    public void RaiseEvapouratableSpillPuddle(Vector3 pos, float raiseTime, float raiseHeight, float evaporateTime,
        float evaporatedScale)
    {
        for (int i = 0; i < spillPuddles.Length; i++)
        {
            if (spillPuddles[i].gameObject.activeSelf == false)
            {
                spillPuddles[i].transform.position = pos;
                spillPuddles[i].Raise(raiseTime, raiseHeight);
                spillPuddles[i].SetEvaporate(evaporateTime, evaporatedScale);
                activeSpillPuddles += 1;
                break;
            }
        }
    }
}
