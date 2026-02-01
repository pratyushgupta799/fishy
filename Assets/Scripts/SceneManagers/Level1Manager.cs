using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Level1Manager : MonoBehaviour
{
    [SerializeField] private Transform fishy;
    
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
}
