using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Level1Manager : MonoBehaviour
{
    [SerializeField] private Transform fishy;
    
    [SerializeField] private PlayerInput playerInput;
    
    [SerializeField] private List<TextMeshPro> keyboardTexts;
    [SerializeField] private List<TextMeshPro> gamepadTexts;

    void OnEnable()
    {
        playerInput.onControlsChanged += OnControlsChanged;
    }

    void OnDisable()
    {
        playerInput.onControlsChanged -= OnControlsChanged;
    }
    
    void Start()
    {
        StartTransition();
    }

    async void StartTransition()
    {
        await Task.Yield();
        
        Vector2 fishyPos = Camera.main.WorldToScreenPoint(fishy.position);
        await CircleTransition.Instance.CircleOut(fishyPos);
    }
    
    void OnControlsChanged(PlayerInput pi)
    {
        if (pi.currentControlScheme == "KeyboardMouse")
        {
            SetKeyboardTextsActive(true);
            SetControllerTextsActive(false);
        }
        else
        {
            SetKeyboardTextsActive(false);
            SetControllerTextsActive(true);
        }
    }

    void SetKeyboardTextsActive(bool active)
    {
        for (int i = 0; i < keyboardTexts.Count; i++)
        {
            keyboardTexts[i].gameObject.SetActive(active);
        }
    }
    
    void SetControllerTextsActive(bool active)
    {
        for (int i = 0; i < gamepadTexts.Count; i++)
        {
            gamepadTexts[i].gameObject.SetActive(active);
        }
    }
}
