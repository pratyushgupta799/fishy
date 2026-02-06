using System;
using UnityEngine;

public class HintTrigger : MonoBehaviour
{
    [SerializeField] private int hintPhase = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Level1Hint.Instance.SetHintPhase(hintPhase);
            Debug.Log("Hint trigger " + hintPhase + " entered");
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            Level1Hint.Instance.SetHintPhase(0);
        }
        
    }
}
