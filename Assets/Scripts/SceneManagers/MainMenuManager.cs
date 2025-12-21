using System.Threading.Tasks;
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
        start.onClick.AddListener(OnStartClicked);
    }

    async void OnStartClicked()
    {
        await ChangeScene();
    }

    public async Task ChangeScene()
    {
        Vector2 buttonPos = RectTransformUtility.WorldToScreenPoint(null, start.transform.position);
        await CircleTransition.Instance.CircleIn(buttonPos);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
        Debug.Log("Starting level 1");
    }
}
