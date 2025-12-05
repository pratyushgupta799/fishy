using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

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
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashSpeed = 0.5f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpMoveFactorFromWater = 1.5f;
    [SerializeField] private float jumpMoveFactorFromGround = 0.5f;
    [SerializeField] private float jumpForceWater = 5f;
    [SerializeField] private float jumpForceGround = 2f;
    [SerializeField] private float minJumpForceWater = 2f;
    [SerializeField] private float maxAirCharge = 1f;
    [SerializeField] private float airChargeForce = 3f;

    [Header("Gravity")]
    [SerializeField] private float airGravityScale = 1f;
    
    [Header("Ground check parameters")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private float wallDistance = 0.2f;
    [SerializeField] private LayerMask fishyLayer;
    [SerializeField] private LayerMask interactibleLayer;

    [Header("Death Settings")] 
    [SerializeField] private int deathTime = 5;
	[SerializeField] private int foodTime = 2;

    [Header("Checkpoint")] 
    [SerializeField] private float rHoldTime = 1f;

	// components reference
	private Rigidbody rb;
    
    // input axes
    private float vertical;
    private float horizontal;
    private float up;
    
    // input vectors
    private Vector3 forward;
    private Vector3 right;
    private Vector3 upward;
    
    // states
    private bool inWater;
    private bool isAtSurface;
    private bool isGrounded;
    private bool isJumpingFromSurface;
    private bool isJumpingFromGround;
    private bool isJumping;
    
    // checkpoint
    private float rholdTimer = 0f;
    
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
    private float dashTime;
    
    // death
    private float currentDeathTimer;
    
    // jump
    private float jumpHoldTimer = 0f;
    private bool canCharge = false;
    
    // properties
    private bool IsJumping
    {
        get
        {
            return isJumping;
        }
        set
        {
            if (value == false)
            {
                canCharge = false;
                isJumping = false;
                isJumpingFromGround = false;
                isJumpingFromSurface = false;
            }
        }
    }
    private bool IsJumpingFromSurface
    {
        get
        {
            return isJumpingFromSurface;
        }
        set
        {
            if (value == true)
            {
                isJumping = true;
                isJumpingFromSurface = true;
            }
        }
    }
    private bool IsJumpingFromGround
    {
        get
        {
            return isJumpingFromGround;
        }
        set
        {
            if (value == true)
            {
                isJumping = true;
                isJumpingFromGround = true;
            }
        }
    }

    private void Awake()
    {
        currentDeathTimer = deathTime;
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Debug.DrawRay(wallCheck.position, transform.forward * wallDistance, Color.red);
        ReloadInput();
        MoveInput();
        CheckGrounded();
        JumpInput();
        DashInput();
        MoveCharacter();
    }

    private void MoveCharacter()
    {
        if (isAtSurface)
        {
            SurfaceMovement();
            if(currentDeathTimer <= deathTime)
            {
                currentDeathTimer = deathTime;
            }
        }
        else if (inWater)
        {
            if (!IsJumping)
            {
                WaterMovement();
                if(currentDeathTimer <= deathTime)
                {
                    currentDeathTimer = deathTime;
                }
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

        if (!inWater)
        {
            currentDeathTimer -= Time.deltaTime;
        }
        
        DebugEvents.OnDeathTimerChanged?.Invoke((int)currentDeathTimer);
        if (currentDeathTimer <= 0)
        {
            CheckPointManager.Instance.LoadLastCheckpoint();
            currentDeathTimer = deathTime;
        }
    }
    
    private void ReloadInput()
    {
        if (Input.GetKey(KeyCode.R))
        {
            rholdTimer += Time.deltaTime;

            if (rholdTimer >= rHoldTime)
            {
                CheckPointManager.Instance.LoadLastCheckpoint();
                currentDeathTimer = deathTime;
                rholdTimer = 0;
            }
        }
        else
        {
            rholdTimer = 0f;
        }
    }

    private void MoveInput()
    {
        if (dashTime > 0)
        {
            dashTime -= Time.deltaTime;
            vertical = 0f;
            horizontal = 0f;
            up = 0f;
        }
        else
        {
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
            up = Input.GetAxis("Up");
        }

        forward = CamForwardFlat() * vertical;
        right = CamRightFlat() * horizontal;
        forward.y = 0;
        right.y = 0;
        upward = up * Vector3.up;
    }

    private void CheckGrounded()
    {
        if (!inWater && !((!IsJumping) && rb.linearVelocity.y > 0))
        {
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundDistance,
                ~fishyLayer,
                QueryTriggerInteraction.Ignore
            );
            
            IsJumping = !isGrounded;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void JumpInput()
    {
        if (!IsJumping && isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.useGravity = true;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForceGround, rb.linearVelocity.z);

                IsJumpingFromGround = true;
                isAtSurface = false;
                isGrounded = false;
                
                jumpMoveFactor = jumpMoveFactorFromGround;

                jumpHoldTimer = 0f;
                return;
            }
        }

        if (!IsJumping && isAtSurface)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.useGravity = true;
                
                float startForce = minJumpForceWater;

                rb.linearVelocity = new Vector3(rb.linearVelocity.x, startForce, rb.linearVelocity.z);
                
                jumpMoveFactor = jumpMoveFactorFromWater;

                IsJumpingFromSurface = true;
                isGrounded = false;
                isAtSurface = false;
                
                jumpHoldTimer = 0f;

                canCharge = true;
            }
        }

        if (IsJumpingFromSurface)
        {
            if (Input.GetKey(KeyCode.Space) && (jumpHoldTimer < maxAirCharge) && canCharge)
            {
                jumpHoldTimer += Time.deltaTime;
                float t = jumpHoldTimer / maxAirCharge;
                float falloff = 1f - t;
                float addedForce = airChargeForce * falloff * Time.deltaTime;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y + addedForce,
                    rb.linearVelocity.z);
            }
            
            if (Input.GetKeyUp(KeyCode.Space))
            {
                jumpHoldTimer = 0f;
                canCharge = false;
            }
        }
    }

    private void DashInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && inWater && dashTime <= 0f)
        {
            rb.AddForce(transform.forward * dashSpeed, ForceMode.VelocityChange);
            dashTime = dashDuration;
        }
    }

    private void SurfaceMovement()
    {
        DebugEvents.OnFishyMoveStateChanged?.Invoke("IsOnSurface");
        rb.useGravity = false;
        rb.mass = underWaterMass;
        
        if (dashTime > 0)
        {
            return;
        }
            
        forward.y = 0f;
        right.y = 0f;
            
        swimDirection = (forward + right).normalized;
            
        if(upward.y > 0)
        {
            upward.y = 0;
        }

        swimDirection += upward;

        if (dashTime <= 0f)
        {
            rb.linearVelocity = swimDirection * maxSpeed;
        }
            
        if (swimDirection.magnitude > 0.1f)
        {
            RotateTo(swimDirection.normalized);
            animator.SetBool("isSwiming", true);
        }
        else animator.SetBool("isSwiming", false);

        // Keep near surface
        if (up >= 0 && !IsJumping && (dashTime <= 0f))
        {
            rb.position = Vector3.Lerp(rb.position, new Vector3(rb.position.x, surfaceHeight + 0.07f, rb.position.z),
                5f * Time.deltaTime);
            Vector3 currentEuler = transform.rotation.eulerAngles;
            currentEuler.x = Mathf.LerpAngle(currentEuler.x, 0f, 10f * Time.deltaTime);
            currentEuler.z = Mathf.LerpAngle(currentEuler.z, 0f, 10f * Time.deltaTime);
            transform.rotation = Quaternion.Euler(currentEuler);
        }
    }

    private void WaterMovement()
    {
        DebugEvents.OnFishyMoveStateChanged?.Invoke("IsInWater");
        rb.useGravity = false;
        rb.mass = underWaterMass;
        
        if (dashTime > 0)
        {
            return;
        }

        Vector3 swimVel = (forward + right + upward).normalized * maxSpeed;

        if (dashTime <= 0f)
        {
            rb.linearVelocity = new Vector3(swimVel.x, swimVel.y, swimVel.z);
        }
        
        if (swimVel.magnitude > 0.01f)
        {
            RotateTo(swimVel.normalized);
            animator.SetBool("isSwiming", true);
        }
        else animator.SetBool("isSwiming", false);
    }

    private void GroundMovement()
    {
        rb.mass = 1f;
        DebugEvents.OnFishyMoveStateChanged?.Invoke("IsOnGround");
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
        
        Quaternion lookRot = transform.rotation;

        if (swimDirection.magnitude > 0.1f)
        {
            lookRot = Quaternion.LookRotation(swimDirection.normalized);

            Quaternion slopeRot = lookRot;
            if (Physics.Raycast(groundCheck.transform.position, Vector3.down, out RaycastHit hit,
                    groundDistance + 0.5f, ~0,
                    QueryTriggerInteraction.Ignore))
            {
                slopeRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * lookRot;
            }
            
            RotateTo(slopeRot);
            animator.SetBool("isSwiming", true);
        }
        else
        {
            lookRot.x = 0f;

            Quaternion slopeRot = lookRot;
            if (Physics.Raycast(groundCheck.transform.position, Vector3.down, out RaycastHit hit,
                    groundDistance + 0.5f, ~0,
                    QueryTriggerInteraction.Ignore))
            {
                slopeRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * lookRot;
            }
            
            RotateTo(slopeRot);
        }
    }

    private void AirMovement()
    {
        rb.mass = 1f;
        DebugEvents.OnFishyMoveStateChanged?.Invoke("IsInAir");
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
            // Debug.Log(swimDirection);
            if (swimDirection.x > 0.1f || swimDirection.z > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rb.linearVelocity),
                    turnSmoothTime * Time.deltaTime);
            }
            else
            {
                Vector3 camLookFlat = camera.transform.forward;
                camLookFlat.y = 0f;
                RotateTo(camLookFlat + rb.linearVelocity);
            }
        }
        else
        {
            Debug.Log("Something in the way");
            RotateTo(swimDirection);
        }
    }

    private void RotateTo(Vector3 target)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, 
            Quaternion.LookRotation(target),
            turnSmoothTime * Time.deltaTime);
    }

    private void RotateTo(Quaternion target)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation,
            target,
            turnSmoothTime * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            inWater = true;
            Debug.Log("Water triggered");
            IsJumping = false;
        }
        if (other.CompareTag("WaterSurface"))
        {
            Debug.Log("Surface trigger");
            if (inWater)
            {
                if (IsJumping) IsJumping = false;
                isAtSurface = true;
                rb.linearVelocity = Vector3.zero;
                surfaceHeight = other.transform.position.y;
            }
        }
        if (other.CompareTag("Death"))
        {
            CheckPointManager.Instance.LoadLastCheckpoint();
            currentDeathTimer = deathTime;
            Debug.Log("Death trigger");
        }

        if (other.CompareTag("CheckpointTrigger"))
        {
            CheckPointManager.Instance.SetCheckPoint(other.gameObject);
        }

        if (other.CompareTag("Health"))
        {
            currentDeathTimer += foodTime;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("WaterSurface"))
        {
            // Debug.Log("Water surface trigger stay");
            if (inWater && !isJumping)
            {
                isAtSurface = true;
                onSurfaceThisFrame = true;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                surfaceHeight = other.transform.position.y;
                
                surfaceExitTime = Time.time + surfaceExitGrace;
            }
        }
        if (other.CompareTag("Water"))
        {
            // Debug.Log("Water trigger stay");
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

    public bool IsDashing()
    {
        return (dashTime > 0 && dashTime <= dashDuration);
    }

    Vector3 CamForwardFlat()
    {
        Vector3 camForward = camera.transform.forward;
        camForward.y = 0f;
        return camForward;
    }
    
    Vector3 CamRightFlat()
    {
        Vector3 camRight = camera.transform.right;
        camRight.y = 0f;
        return camRight;
    }

    public void SnapFishyTo(Vector3 location, Quaternion rotation)
    {
        rb.linearVelocity = Vector3.zero;
        transform.position = location;
        transform.rotation = rotation;
        
        Debug.Log("Fishy's position snapped to " + location);
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