using UnityEngine;

public class CameraController : MonoBehaviour
{
    // [SerializeField] private Transform target;
    // [SerializeField] private float distanceFromTarget = 5f;
    [SerializeField] private float sensitivity = 5f;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minY = -30f;
    [SerializeField] private float maxY = 60f;
    
    private float rotationX;
    private float rotationY;

    private void Awake()
    {
        rotationX = transform.localEulerAngles.y;
        rotationY = transform.localEulerAngles.x;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void LateUpdate()
    {
        Debug.Log("Rotation X: "+ rotationX + ", rotationY: " + rotationY);
        
        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * sensitivity;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);
        
        transform.localRotation = Quaternion.Euler();
        // transform.LookAt(target);
    }
}
