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
    }

    private void OnEnable()
    {
        InputManager.Instance.GetComponent<PlayerInput>().onControlsChanged += OnControlsChanged;
    }

    private void OnDisable()
    {
        InputManager.Instance.GetComponent<PlayerInput>().onControlsChanged -= OnControlsChanged;
    }

    private void OnControlsChanged(PlayerInput playerInput)
    {
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
