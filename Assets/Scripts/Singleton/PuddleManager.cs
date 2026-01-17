using UnityEngine;
using UnityEngine.SceneManagement;

public class PuddleManager : MonoBehaviour
{
    [SerializeField] private GameObject puddleWater;
    [SerializeField] private GameObject puddleCoffee;
    
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
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }
    
    
}
