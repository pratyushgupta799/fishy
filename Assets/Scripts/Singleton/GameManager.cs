using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    void Awake()
    {
        //#if UNITY_EDITOR
        //    Debug.unityLogger.logEnabled = true;
        //#else
        //    Debug.unityLogger.logEnabled = false;
        //#endif
        
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


}
