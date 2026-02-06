using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    
    [SerializeField] private PlayerInput playerInput;
    
    [SerializeField] private List<TextMeshPro> keyboardTexts;
    [SerializeField] private List<TextMeshPro> gamepadTexts;

    public event System.Action<InputActionAsset> OnActionReady;
    
    public InputAction Look => playerInput.actions["Look"];

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("Duplicate InputManager instance destroyed");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        playerInput = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.onControlsChanged += OnControlsChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
    }

    void OnDisable()
    {
        if (playerInput != null)
            playerInput.onControlsChanged -= OnControlsChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnControlsChanged(PlayerInput pi)
    {
        Debug.Log("Control scheme changed to: " + pi.currentControlScheme);
        if (pi.currentControlScheme == "Keyboard&Mouse")
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

    private void SetKeyboardTextsActive(bool active)
    {
        for (int i = 0; i < keyboardTexts.Count; i++)
        {
            keyboardTexts[i].gameObject.SetActive(active);
        }
    }
    
    private void SetControllerTextsActive(bool active)
    {
        for (int i = 0; i < gamepadTexts.Count; i++)
        {
            gamepadTexts[i].gameObject.SetActive(active);
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded " + scene.name);
        keyboardTexts = new List<TextMeshPro>();
        gamepadTexts = new List<TextMeshPro>();

        var gpTxts = GameObject.FindGameObjectsWithTag("TextGp");
        var kbTxts = GameObject.FindGameObjectsWithTag("TextKb");

        foreach (var gp in gpTxts)
        {
            gamepadTexts.Add(gp.GetComponent<TextMeshPro>());
        }

        foreach (var kb in kbTxts)
        {
            keyboardTexts.Add(kb.GetComponent<TextMeshPro>());
        }
        
        Debug.Log("firing OnActionReady event");
        OnActionReady?.Invoke(playerInput.actions);
        OnControlsChanged(playerInput);
    }
}
