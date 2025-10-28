using System;
using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    private Transform camForward;
    
    [SerializeField] private float speed = 5f;
    [SerializeField] private float maxSpeed = 10f;
    
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    
    [SerializeField] private CharacterController characterController;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // float vertical = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(KeyCode.W))
        {
            MoveCharacter();
        }
    }

    private void MoveCharacter()
    {
        Vector3 cameraForward = camera.transform.forward;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(cameraForward),
            turnSmoothTime * Time.deltaTime);
        
        Vector3 forward = camera.transform.forward;
        Vector3 swimDirection = forward.normalized;
        
        characterController.Move(swimDirection * speed * Time.deltaTime);
        
    }
}
