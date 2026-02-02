using UnityEngine;
using UnityEngine.InputSystem;

public class HintText : MonoBehaviour
{
    [SerializeField] private string hintTextKeyboard;
    [SerializeField] private string hintTextGamepad;

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
        
    }
}
