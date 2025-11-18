using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FishControllerRB : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject camera;
    [SerializeField] private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float groundSpeedScale = 0.4f;
    [SerializeField] private float turnSmoothTime = 0.1f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpMoveFactorFromWater = 1.5f;
    [SerializeField] private float jumpMoveFactorFromGround = 0.5f;
    [SerializeField] private float jumpForceWater = 5f;
    [SerializeField] private float jumpForceGround = 2f;

    [Header("Gravity")]
    [SerializeField] private float airGravityScale = 1f;
    
    [Header("Ground check parameters")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody rb;
    private float vertical;
    private float horizontal;
    private float up;
    private bool inWater;
    private bool isAtSurface;
    private bool isGrounded;
    private bool isJumping;
    private float jumpMoveFactor;
    private Vector3 swimDirection;
    private float surfaceHeight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // rb.angularDamping = 10f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask);
    }

    private void Update()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        up = Input.GetAxis("Up");

        if (Input.GetKeyDown(KeyCode.Space) && !isJumping && !(inWater && !isAtSurface))
        {
            rb.useGravity = true;
            if (isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForceGround, rb.linearVelocity.z);
                jumpMoveFactor = jumpMoveFactorFromGround;
            }
            else
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForceWater, rb.linearVelocity.z);
                jumpMoveFactor = jumpMoveFactorFromWater;
            }

            isJumping = true;
            inWater = false;
            isAtSurface = false;
            isGrounded = false;
        }

        MoveCharacter();
    }

    private void MoveCharacter()
    {
        Vector3 forward = vertical * camera.transform.forward;
        Vector3 right = horizontal * camera.transform.right;
        Vector3 upVec = up * Vector3.up;
        
        

        if (isAtSurface)
        {
            isJumping = false;
            rb.useGravity = false;
            
            forward.y = 0f;
            right.y = 0f;
            
            swimDirection = (forward + right).normalized;
            
            if(upVec.y > 0)
            {
                upVec.y = 0;
            }
            
            swimDirection = (swimDirection + upVec).normalized;
            rb.linearVelocity = swimDirection * speed;
            
            if (swimDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(swimDirection.normalized),
                    turnSmoothTime * Time.deltaTime);
                animator.SetBool("isSwiming", true);
            }
            else animator.SetBool("isSwiming", false);

            // Keep near surface
            if (up == 0 && !isJumping)
            {
                rb.position = Vector3.Lerp(rb.position, new Vector3(rb.position.x, surfaceHeight, rb.position.z),
                    5f * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.Euler(0f, transform.rotation.y, transform.rotation.z),
                    turnSmoothTime * Time.deltaTime);
            }
        }
        else if (inWater)
        {
            isJumping = false;
            rb.useGravity = false;
            
            Vector3 swimVel = (forward + right + upVec).normalized * speed;
            rb.linearVelocity = new Vector3(swimVel.x, swimVel.y, swimVel.z);

            if (swimVel.magnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rb.linearVelocity),
                    turnSmoothTime * Time.deltaTime);
                animator.SetBool("isSwiming", true);
            }
            else animator.SetBool("isSwiming", false);
        }
        else if (isGrounded)
        {
            isJumping = false;
            rb.useGravity = true;
            rb.linearVelocity = new Vector3(swimDirection.x * groundSpeedScale, rb.linearVelocity.y,
                swimDirection.z * groundSpeedScale);
        }
        else
        {
            rb.useGravity = true;
            // Air movement
            rb.linearVelocity = new Vector3(swimDirection.x * jumpMoveFactor, rb.linearVelocity.y,
                swimDirection.z * jumpMoveFactor);

            if (swimDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rb.linearVelocity),
                    turnSmoothTime * Time.deltaTime);
            }
            else
            {
                Vector3 camLookFlat = camera.transform.forward;
                camLookFlat.y = 0f;
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(camLookFlat + rb.linearVelocity),
                    turnSmoothTime * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WaterSurface"))
        {
            if (isJumping) isJumping = false;
            isAtSurface = true;
            inWater = true;
            rb.linearVelocity = Vector3.zero;
            surfaceHeight = other.transform.position.y;
        }
        else if (other.CompareTag("Water"))
        {
            inWater = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WaterSurface")) isAtSurface = false;
        if (other.CompareTag("Water")) inWater = false;
    }
}
