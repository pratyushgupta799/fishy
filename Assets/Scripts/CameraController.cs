using UnityEngine;

public class CameraController : MonoBehaviour
{
    // [SerializeField] private Transform target;
    // [SerializeField] private float distanceFromTarget = 5f;
    [SerializeField] private float sensitivity = 5f;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minY = -30f;
    [SerializeField] private float maxY = 60f;
    
    private float rotationX = 0f;
    private float rotationY = 0f;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void LateUpdate()
    {
        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * sensitivity;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);
        
        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0);
        
        // transform.LookAt(target);
    }
}
