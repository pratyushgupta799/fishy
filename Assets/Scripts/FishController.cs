using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float verticalSpeed = 2f;
    [SerializeField] private float mouseSensivity = 1f;
    
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
}
