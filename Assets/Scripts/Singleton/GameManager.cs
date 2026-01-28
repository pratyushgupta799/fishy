using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Canvas pauseMenu;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void Awake()
    {
        //#if UNITY_EDITOR
        //    Debug.unityLogger.logEnabled = true;
        //#else
        //    Debug.unityLogger.logEnabled = false;
        //#endif
        
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }
}
