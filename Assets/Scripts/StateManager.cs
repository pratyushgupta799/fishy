using UnityEngine;

public class StateManager : MonoBehaviour
{
    private IInteractible interactible;

    void Awake()
    {
        interactible = gameObject.GetComponent<IInteractible>();
    }
    
    public void RestoreDefault()
    {
        if (interactible != null)
        {
            interactible.RestoreState();
        }
    }
}
