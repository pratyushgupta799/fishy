using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PauseUIController : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button lastCpButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private GameObject pauseMenuPanel;
    private bool paused;
    
    private void OnEnable()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        lastCpButton.onClick.AddListener(OnLastCpClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }
    
    private void OnDisable()
    {
        continueButton.onClick.RemoveListener(OnContinueClicked);
        lastCpButton.onClick.RemoveListener(OnLastCpClicked);
        exitButton.onClick.RemoveListener(OnExitClicked);
    }

    public void OnPauseToggle(InputAction.CallbackContext context)
    {
        PauseToggle();
    }
    
    private void OnContinueClicked()
    {
        PauseToggle();
    }
    
    private void OnLastCpClicked()
    {
        CheckPointManager.Instance.LoadLastCheckpoint();
        PauseToggle();
    }

    private void OnExitClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void PauseToggle()
    {
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
        if (paused)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        pauseMenuPanel.SetActive(paused);
    }
}
