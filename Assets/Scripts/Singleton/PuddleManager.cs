using UnityEngine;
using UnityEngine.SceneManagement;

public class PuddleManager : MonoBehaviour
{
    [SerializeField] private PuddleBehaviour puddleWater;
    private PuddleBehaviour[] puddleWaters;
    
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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        for (int i = 0; i < 12; i++)
        {
            puddleWaters[i] = Instantiate(puddleWater);
            puddleWaters[i].gameObject.SetActive(false);
        }
    }

    public void RaiseEvapouratablePuddle(Vector3 pos, float raiseTime, float raiseHeight, float evaporateTime)
    {
        for (int i = 0; i < puddleWaters.Length; i++)
        {
            if(puddleWaters[i].gameObject.activeSelf == false)
            {
                puddleWaters[i].transform.position = pos;
                puddleWaters[i].Raise(raiseTime, raiseHeight);
                puddleWaters[i].SetEvaporate(evaporateTime);
                break;
            }
        }
    }
}
