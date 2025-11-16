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
    private bool isGrounded;
    private bool surfaceMode;
    private float surfaceHeight;

    [SerializeField] private float jumpForceWater = 5f;
    [SerializeField] private float jumpForceGround = 2f;
    [SerializeField] private float waterGravityScale = 0.1f;
    [SerializeField] private float airGravityScale = 1f;
    private float jumpMoveFactor;
    [SerializeField] private float jumpMoveFactorFromWater = 1.5f;
    [SerializeField] private float jumpMoveFactorFromGround = 0.5f;

    [SerializeField] private float groundSpeedScale = 0.4f;
    private Vector3 swimDirection;
    
    private float verticalVelocity;
    private bool isJumping;

    private void Awake()
    {
        gravity = Physics.gravity * 0.1f;
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        MoveCharacter();
        Vector3 currentEuler = transform.eulerAngles;
        currentEuler.z = 0f;
        transform.eulerAngles = currentEuler;
        // Debug.Log(inWater);
    }

    private void MoveCharacter()
    {
        if (characterController.isGrounded && !inWater)
        {
            isGrounded = true;
        }
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        up = Input.GetAxis("Up");
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping && !(inWater && !surfaceMode))
        {
            if (isGrounded)
            {
                verticalVelocity = jumpForceGround;
                jumpMoveFactor = jumpMoveFactorFromGround;
            }
            else
            {
                verticalVelocity = jumpForceWater;
                jumpMoveFactor = jumpMoveFactorFromWater;
            }
            isJumping = true;
            inWater = false;
            surfaceMode = false;
            isGrounded = false;
        }
        if (surfaceMode)
        {
            isGrounded = false;
            Vector3 forward = vertical * camera.transform.forward;
            Vector3 right = horizontal * camera.transform.right;
            
            if (up > 0)
            {
                up = 0;
            }
            Vector3 Up = up * Vector3.up;
            
            swimDirection = (forward + right).normalized;
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
            
            if (!isJumping)
            {
                characterController.Move((swimDirection * speed) * Time.deltaTime);
                if (up == 0)
                {
                    transform.position = Vector3.Lerp(transform.position,
                        new Vector3(transform.position.x, surfaceHeight, transform.position.z), 5f * Time.deltaTime);
                    
                    Vector3 currentEuler = transform.rotation.eulerAngles;
                    currentEuler.x = Mathf.LerpAngle(currentEuler.x, 0f, 10f * Time.deltaTime);
                    transform.rotation = Quaternion.Euler(currentEuler);
                }
            }

            
        }
        else if (inWater)
        {
            isGrounded = false;
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
        else if (isGrounded)
        {
            // Debug.Log("is grounded");
            isJumping = false;
            inWater = false;
            surfaceMode = false;
            
            verticalVelocity = Physics.gravity.y * airGravityScale;
            Vector3 forward = vertical * camera.transform.forward;
            Vector3 right = horizontal * camera.transform.right;
            Vector3 Up = verticalVelocity * Vector3.up;
            
            forward.y = 0f;
            forward.Normalize();
            
            right.y = 0f;
            right.Normalize();
            
            Vector3 currentEuler = transform.rotation.eulerAngles;
            currentEuler.x = Mathf.LerpAngle(currentEuler.x, 0f, 10f * Time.deltaTime);
            // currentEuler.x = 0f;
            transform.rotation = Quaternion.Euler(currentEuler);
            
            swimDirection = (forward + right).normalized;
            swimDirection += Up;
            
            if (Math.Abs(swimDirection.x) > 0.1f || Math.Abs(swimDirection.z) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(new Vector3(swimDirection.x, 0f, swimDirection.z),
                        Vector3.up),
                    turnSmoothTime * Time.deltaTime);
                animator.SetBool("isSwiming", true);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(new Vector3(camera.transform.forward.x, 0f, camera.transform.forward.z),
                        Vector3.up),
                    turnSmoothTime * Time.deltaTime);
                animator.SetBool("isSwiming", false);
            }
            
            if (!isJumping)
            {
                characterController.Move((swimDirection * groundSpeedScale) * Time.deltaTime);
                Debug.Log(swimDirection);
                // transform.position = Vector3.Lerp(transform.position,
                //     new Vector3(transform.position.x, surfaceHeight, transform.position.z), 5f * Time.deltaTime);
            }
        }
        else
        {
            // jump logic
            verticalVelocity += Physics.gravity.y * airGravityScale * Time.deltaTime;
            Vector3 forward = vertical * camera.transform.forward;
            Vector3 right = horizontal * camera.transform.right;
            Vector3 Up = verticalVelocity * Vector3.up;
            
            forward.y = 0f;
            forward.Normalize();
            
            right.y = 0f;
            right.Normalize();
            
            swimDirection = (forward + right).normalized;
            swimDirection *= jumpMoveFactor;
            swimDirection = (swimDirection + Up);

            if (Math.Abs(swimDirection.x) > 0.1f || Math.Abs(swimDirection.z) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(swimDirection,
                        Vector3.up),
                    turnSmoothTime * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(swimDirection + camera.transform.forward,
                        Vector3.up),
                    turnSmoothTime * Time.deltaTime);
            }
            
            characterController.Move(swimDirection * Time.deltaTime);
        }
        Debug.Log(swimDirection);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WaterSurface"))
        {
            if (isJumping)
            {
                isJumping = false;
                inWater = true;
                verticalVelocity = 0f;
            }
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
