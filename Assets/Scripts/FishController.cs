using System;
using UnityEngine;

public class FishController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private GameObject camera;
    private Transform camForward;
    
    [SerializeField] private float speed = 5f;
    [SerializeField] private float maxSpeed = 10f;
    
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // float vertical = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(KeyCode.W))
        {
            MoveCharacter();
        }
        else
        {
            DisableMovement();
            Debug.Log("yo");
        }
    }

    private void MoveCharacter()
    {
        Vector3 cameraForward = new Vector3(camera.transform.forward.x, 0, camera.transform.forward.z).normalized;
        transform.rotation = Quaternion.LookRotation(cameraForward);
        // transform.Rotate(new Vector3(0, 0, 0), Space.Self);
        
        Vector3 forward = camera.transform.forward;
        Vector3 swimDirection = forward.normalized;
        
        rb.AddForce(swimDirection * speed, ForceMode.VelocityChange);
        
    }

    private void DisableMovement()
    {
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

    // private void FixedUpdate()
    // {
    //     float horizontal = Input.GetAxisRaw("Horizontal");
    //     float vertical = Input.GetAxisRaw("Vertical");
    //     Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
    //     
    //     if (direction.magnitude >= 0.1f)
    //     {
    //         float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camera.transform.eulerAngles.y;
    //         
    //         transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(camera.transform.eulerAngles.x, targetAngle, 0), turnSmoothTime);
    //
    //         Vector3 moveDirection = Quaternion.Euler(camera.transform.eulerAngles.x, targetAngle, 0f) * Vector3.forward;
    //         rb.AddForce(moveDirection.normalized * speed * Time.deltaTime, ForceMode.VelocityChange);
    //     }
    // }
}
