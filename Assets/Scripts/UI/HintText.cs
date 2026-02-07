using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class HintText : MonoBehaviour
{
    [SerializeField] private string hintTextKeyboard;
    [SerializeField] private string hintTextGamepad;
    
    private TextMeshProUGUI textMesh;

    private void Awake()
    {
        gameObject.SetActive(false);
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        InputManager.Instance.GetComponent<PlayerInput>().onControlsChanged += OnControlsChanged;
        OnControlsChanged(InputManager.Instance.GetComponent<PlayerInput>());
    }

    private void OnDisable()
    {
        InputManager.Instance.GetComponent<PlayerInput>().onControlsChanged -= OnControlsChanged;
    }

    private void OnControlsChanged(PlayerInput playerInput)
    {
        // Debug.Log("Control scheme changed to: " + playerInput.currentControlScheme);
        if(playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            textMesh.text = hintTextKeyboard;
        }
        else if(playerInput.currentControlScheme == "Gamepad")
        {
            textMesh.text = hintTextGamepad;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
