using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class FishControllerRB : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject camera;
    [SerializeField] private Animator animator;
    
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float moveForce = 2f;
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
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private float wallDistance = 0.2f;
    [SerializeField] private LayerMask fishyLayer;
    [SerializeField] private LayerMask interactibleLayer;

    [Header("DebugUI")] 
    [SerializeField] private TextMeshProUGUI debugText;

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
        
    }

    private void Update()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        up = Input.GetAxis("Up");
        
        Debug.DrawRay(wallCheck.position, transform.forward * wallDistance, Color.red);
        // Gizmos.color = Color.green;
        // Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        
        if (!inWater && !(isJumping && rb.linearVelocity.y > 0))
        {
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundDistance,
                ~fishyLayer,
                QueryTriggerInteraction.Ignore
            );
        }
        else
        {
            isGrounded = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isJumping && (isGrounded || isAtSurface))
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
            // inWater = false;
            isAtSurface = false;
            isGrounded = false;
        }

        MoveCharacter();
    }

    private void MoveCharacter()
    {
        Vector3 forward = vertical * camera.transform.forward;
        Vector3 right = horizontal * camera.transform.right;
        forward.y = 0;
        right.y = 0;
        Vector3 upVec = up * Vector3.up;
        
        if (isAtSurface)
        {
            debugText.SetText("IsOnSurface");
            // Debug.Log("IsOnSurface");
            // isJumping = false;
            rb.useGravity = false;
            
            forward.y = 0f;
            right.y = 0f;
            
            swimDirection = (forward + right).normalized;
            
            if(upVec.y > 0)
            {
                upVec.y = 0;
            }
            
            rb.linearVelocity = swimDirection * maxSpeed;
            
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
                Vector3 currentEuler = transform.rotation.eulerAngles;
                currentEuler.x = Mathf.LerpAngle(currentEuler.x, 0f, 10f * Time.deltaTime);
                transform.rotation = Quaternion.Euler(currentEuler);
            }
        }
        else if (inWater)
        {
            if (!isJumping)
            {
                debugText.SetText("IsInWater");
                // Debug.Log("IsInWater");
                // isJumping = false;
                rb.useGravity = false;

                Vector3 swimVel = (forward + right + upVec).normalized * maxSpeed;
                rb.linearVelocity = new Vector3(swimVel.x, swimVel.y, swimVel.z);

                if (swimVel.magnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(rb.linearVelocity),
                        turnSmoothTime * Time.deltaTime);
                    animator.SetBool("isSwiming", true);
                }
                else animator.SetBool("isSwiming", false);
            }
        }
        else if (isGrounded)
        {
            debugText.SetText("IsOnGround");
            // Debug.Log("IsOnGround");
            forward.y = 0f;
            right.y = 0f;
            swimDirection = (forward + right).normalized;
            
            isJumping = false;
            rb.useGravity = true;
            bool canGo = !Physics.Raycast(
                wallCheck.position,
                transform.forward,
                wallDistance,
                ~interactibleLayer,
                QueryTriggerInteraction.Ignore
            );
            if (canGo)
            {
                rb.linearVelocity = new Vector3(swimDirection.x * groundSpeedScale, rb.linearVelocity.y,
                    swimDirection.z * groundSpeedScale);
            }
            else
            {
                Debug.Log("Something in the way");
                rb.linearVelocity = Vector3.zero;
            }

            if (swimDirection.magnitude > 0.1f)
            {
                Quaternion lookRot = transform.rotation;
                lookRot = Quaternion.LookRotation(swimDirection.normalized);

                Quaternion slopeRot = lookRot;
                if (Physics.Raycast(groundCheck.transform.position, Vector3.down, out RaycastHit hit,
                        groundDistance + 0.5f, ~0,
                        QueryTriggerInteraction.Ignore)) 
                {
                    slopeRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * lookRot;
                }
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    slopeRot,
                    turnSmoothTime * Time.deltaTime);
                animator.SetBool("isSwiming", true);
            }
            else
            {
                Quaternion lookRot = transform.rotation;
                Vector3 camForward = camera.transform.forward;
                camForward.y = 0;
                lookRot = Quaternion.LookRotation(camForward.normalized);

                Quaternion slopeRot = lookRot;
                if (Physics.Raycast(groundCheck.transform.position, Vector3.down, out RaycastHit hit,
                        groundDistance + 0.5f, ~0,
                        QueryTriggerInteraction.Ignore)) 
                {
                    slopeRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * lookRot;
                }
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    slopeRot,
                    turnSmoothTime * Time.deltaTime);
            }
        }
        else
        {
            debugText.SetText("IsInAir");
            // Debug.Log("IsInAir");
            forward.y = 0f;
            forward.Normalize();

            right.y = 0f;
            right.Normalize();
            
            swimDirection = (forward + right).normalized;
            rb.useGravity = true;
            // Air movement
            bool canGo = !Physics.Raycast(
                wallCheck.position,
                transform.forward,
                wallDistance,
                ~interactibleLayer,
                QueryTriggerInteraction.Ignore
            );
            if (canGo)
            {
                rb.linearVelocity = new Vector3(swimDirection.x * jumpMoveFactor, rb.linearVelocity.y,
                    swimDirection.z * jumpMoveFactor);
                if (rb.linearVelocity.magnitude > 0.1f)
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
            else
            {
                Debug.Log("Something in the way");
                // rb.linearVelocity = Vector3.zero;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(swimDirection),
                    turnSmoothTime * Time.deltaTime);
            }
            
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            inWater = true;
            Debug.Log("Water triggered");
            isJumping = false;
        }
        if (other.CompareTag("WaterSurface"))
        {
            Debug.Log("Surface trigger");
            if (inWater)
            {
                if (isJumping) isJumping = false;
                isAtSurface = true;
                rb.linearVelocity = Vector3.zero;
                surfaceHeight = other.transform.position.y;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WaterSurface")) isAtSurface = false;
        if (other.CompareTag("Water")) inWater = false;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}

// What do I want:
// 1. While on ground, the fish rotation should snap to ground's normal
// 2. The fish should stop receiving move input if the input is in the direction where there is wall or something
// 3. After jump is clicked, the fish should go to air mode irrespective of 