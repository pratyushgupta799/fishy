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
    [SerializeField] private float underWaterMass = 0.02f;

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
    [SerializeField] private TextMeshProUGUI stateDebugText;
    [SerializeField] private TextMeshProUGUI deathTimerText;

    [Header("Death Settings")] 
    [SerializeField] private int deathTime = 5;
    
    // components reference
    private Rigidbody rb;
    
    // input axes
    private float vertical;
    private float horizontal;
    private float up;
    
    // input vectors
    private Vector3 forward;
    private Vector3 right;
    private Vector3 Up;
    
    // states
    private bool inWater;
    private bool isAtSurface;
    private bool isGrounded;
    private bool isJumping;
    
    // others
    private float jumpMoveFactor;
    private Vector3 swimDirection;
    private float surfaceHeight;
    private bool onSurfaceThisFrame;
    private bool inWaterThisFrame;
    private float surfaceExitTime;
    private float surfaceExitGrace = 0.05f;
    private float waterExitTime;
    private float waterExitGrace = 0.05f;
    
    // death
    private float currentDeathTimer = 0.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Debug.DrawRay(wallCheck.position, transform.forward * wallDistance, Color.red);
        MoveInput();
        CheckGrounded();
        JumpInput();
        MoveCharacter();
    }

    private void MoveCharacter()
    {
        if (isAtSurface)
        {
            SurfaceMovement();
            currentDeathTimer = 0;
        }
        else if (inWater)
        {
            if (!isJumping)
            {
                WaterMovement();
                currentDeathTimer = 0;
            }
        }
        else if (isGrounded)
        {
            GroundMovement();
        }
        else
        {
            AirMovement();
        }
        
        currentDeathTimer += Time.deltaTime;
        deathTimerText.text = ((int)currentDeathTimer).ToString();
        if (currentDeathTimer >= deathTime)
        {
            CheckPointManager.Instance.LoadLastCheckpoint();
        }
    }

    private void MoveInput()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        up = Input.GetAxis("Up");
        
        forward = vertical * camera.transform.forward;
        right = horizontal * camera.transform.right;
        forward.y = 0;
        right.y = 0;
        Up = up * Vector3.up;
    }

    private void CheckGrounded()
    {
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
    }

    private void JumpInput()
    {
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
            isAtSurface = false;
            isGrounded = false;
        }
    }

    private void SurfaceMovement()
    {
        stateDebugText.SetText("IsOnSurface");
        rb.useGravity = false;
        rb.mass = underWaterMass;
            
        forward.y = 0f;
        right.y = 0f;
            
        swimDirection = (forward + right).normalized;
            
        if(Up.y > 0)
        {
            Up.y = 0;
        }

        swimDirection += Up;
        
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
            rb.position = Vector3.Lerp(rb.position, new Vector3(rb.position.x, surfaceHeight + 0.07f, rb.position.z),
                5f * Time.deltaTime);
            Vector3 currentEuler = transform.rotation.eulerAngles;
            currentEuler.x = Mathf.LerpAngle(currentEuler.x, 0f, 10f * Time.deltaTime);
            transform.rotation = Quaternion.Euler(currentEuler);
        }
    }

    private void WaterMovement()
    {
        stateDebugText.SetText("IsInWater");
        rb.useGravity = false;
        rb.mass = underWaterMass;

        Vector3 swimVel = (forward + right + Up).normalized * maxSpeed;
        
        rb.linearVelocity = new Vector3(swimVel.x, swimVel.y, swimVel.z);

        if (swimVel.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(swimVel.normalized),
                turnSmoothTime * Time.deltaTime);
            animator.SetBool("isSwiming", true);
        }
        else animator.SetBool("isSwiming", false);
    }

    private void GroundMovement()
    {
        rb.mass = 1f;
        stateDebugText.SetText("IsOnGround");
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

    private void AirMovement()
    {
        rb.mass = 1f;
        stateDebugText.SetText("IsInAir");
        forward.y = 0f;
        forward.Normalize();

        right.y = 0f;
        right.Normalize();
            
        swimDirection = (forward + right).normalized;
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
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(swimDirection),
                turnSmoothTime * Time.deltaTime);
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
        if (other.CompareTag("Death"))
        {
            CheckPointManager.Instance.LoadLastCheckpoint();
            Debug.Log("Death trigger");
        }

        if (other.CompareTag("CheckpointTrigger"))
        {
            CheckPointManager.Instance.SetCheckPoint(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("WaterSurface"))
        {
            Debug.Log("Water surface trigger stay");
            if (inWater && !isJumping)
            {
                isAtSurface = true;
                onSurfaceThisFrame = true;
                rb.linearVelocity = Vector3.zero;
                surfaceHeight = other.transform.position.y;
                
                surfaceExitTime = Time.time + surfaceExitGrace;
            }
        }
        if (other.CompareTag("Water"))
        {
            Debug.Log("Water trigger stay");
            inWater = true;
            inWaterThisFrame = true;
            
            waterExitTime = Time.time + waterExitGrace;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WaterSurface"))
        {
            Debug.Log("Surface trigger exit");
            isAtSurface = false;
        }

        if (other.CompareTag("Water"))
        {
            Debug.Log("Water trigger exit");
            inWater = false;
        }
    }

    private void LateUpdate()
    {
        if (!onSurfaceThisFrame && isAtSurface)
        {
            isAtSurface = false;
        }
        else
        {
            // Debug.Log("is on surface this frame");
        }

        if (!inWaterThisFrame && inWater)
        {
            inWater = false;
        }
        else
        {
            // Debug.Log("is in water this frame");
        }

        if (Time.time > waterExitTime)
        {
            inWaterThisFrame = false;
        }
        else
        {
            // Debug.Log("water grace " + waterExitGrace + " current time " + Time.time);
        }
        
        if (Time.time > surfaceExitTime)
        {
            onSurfaceThisFrame = false;
        }
        else
        {
            // Debug.Log("surface grace " + surfaceExitGrace + " current time " + Time.time);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
#endif
}