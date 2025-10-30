using System;
using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    private Transform camForward;
    
    [SerializeField] private float speed = 5f;
    [SerializeField] private float maxSpeed = 10f;
    private float vertical;
    private float horizontal;
    private float up;
    private Vector3 gravity;
    
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    
    [SerializeField] private CharacterController characterController;

    private bool inWater;

    private void Awake()
    {
        gravity = Physics.gravity * 0.1f;
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        MoveCharacter();
        Debug.Log(inWater);
    }

    private void MoveCharacter()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        up = Input.GetAxis("Up");
        if (inWater)
        {
            Vector3 forward = vertical * camera.transform.forward;
            Vector3 right = horizontal * camera.transform.right;
            Vector3 Up = up * camera.transform.up;
            
            Vector3 swimDirection = (forward + right).normalized;

            if (swimDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(swimDirection, camera.transform.up),
                                turnSmoothTime * Time.deltaTime);
            }
            
            var finalSwimDirection = (swimDirection + Up).normalized;

            characterController.Move((finalSwimDirection * speed) * Time.deltaTime);
        }
        else
        {
            Debug.Log(gravity);
            characterController.Move(gravity * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            inWater = true;
            Debug.Log("In water");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            inWater = false;
            Debug.Log("Outta water");
        }
    }
}
