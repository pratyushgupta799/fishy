using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenManager : MonoBehaviour
{
    public static EndScreenManager Instance;

    public void Replay()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
