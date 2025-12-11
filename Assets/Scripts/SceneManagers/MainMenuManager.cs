using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button start;
    [SerializeField] private Button exit;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        start.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
            Debug.Log("Starting level 1");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
