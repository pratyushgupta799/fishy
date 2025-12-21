using System.Threading.Tasks;
using UnityEngine;

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
