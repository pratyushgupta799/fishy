using System;
using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    [SerializeField] private GameObject cineMachine;
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
    [SerializeField] private Animator animator;

    private bool inWater;

    private bool surfaceMode;
    private float surfaceHeight;

    private void Awake()
    {
        gravity = Physics.gravity * 0.1f;
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        MoveCharacter();
        // Debug.Log(inWater);
    }

    private void MoveCharacter()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        up = Input.GetAxis("Up");
        if (surfaceMode)
        {
            Vector3 forward = vertical * camera.transform.forward;
            Vector3 right = horizontal * camera.transform.right;
            
            if (up > 0)
            {
                up = 0;
            }
            Vector3 Up = up * Vector3.up;
            
            Vector3 swimDirection = (forward + right).normalized;
            swimDirection.y = 0f;
            swimDirection = (swimDirection + Up).normalized;
            // Debug.Log(swimDirection);
            
            if (swimDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(swimDirection,
                        transform.up),
                    turnSmoothTime * Time.deltaTime);
                animator.SetBool("isSwiming", true);
            }
            else
            {
                animator.SetBool("isSwiming", false);
            }

            characterController.Move((swimDirection * speed) * Time.deltaTime);
            if (up == 0)
            {
                transform.position = Vector3.Lerp(transform.position,
                    new Vector3(transform.position.x, surfaceHeight, transform.position.z), 5f * Time.deltaTime);
            }
        }
        else if (inWater)
        {
            Vector3 forward = vertical * camera.transform.forward;
            Vector3 right = horizontal * camera.transform.right;
            Vector3 Up = up * Vector3.up;
            
            right.y = 0f;
            right.Normalize();
            
            Vector3 swimDirection = (forward + right + Up).normalized;
            
            if (swimDirection.magnitude < 0.01f)
            {
                swimDirection = Vector3.zero;
                animator.SetBool("isSwiming", false);
            }
            else
            {
                swimDirection.Normalize();
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(swimDirection, Vector3.up),
                    turnSmoothTime * Time.deltaTime
                );
                animator.SetBool("isSwiming", true);
            }

            characterController.Move((swimDirection * speed) * Time.deltaTime);
        }
        else
        {
            characterController.Move(gravity * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WaterSurface"))
        {
            surfaceMode = true;
            Debug.Log("Surface Mode");
            surfaceHeight = other.transform.position.y;
            
        }
        else if (other.CompareTag("Water"))
        {
            inWater = true;
            Debug.Log("In water");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WaterSurface"))
        {
            surfaceMode = false;
        }
        else if (other.CompareTag("Water"))
        {
            inWater = false;
            Debug.Log("Outta water");
        }
    }
}
