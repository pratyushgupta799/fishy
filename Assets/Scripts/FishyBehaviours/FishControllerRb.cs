using System;
using FishyUtilities;
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
    [SerializeField] private ParticleSystem bubblesps;
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private GameObject colliderCenter;
    
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float moveForce = 2f;
    [SerializeField] private float groundSpeedScale = 0.4f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float underWaterMass = 0.02f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashSpeed = 0.5f;
    
    [Header("Roll and Flop")]
    [SerializeField] private float torqueForce = 5f;
    [SerializeField] private float maxRollVelocity = 2f;
    [SerializeField] private float flopTimer = 0.5f;
    [SerializeField] private float flopForce = 2f;
    [SerializeField] private float flopCoyote = 0.5f;

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
    
    // jump
    private float jumpHoldTimer = 0f;
    private bool canCharge = false;
    
    // movement
    private bool wasMoving;
    private bool forwardLocked;
    private bool rightLocked;
    private bool upLocked;
    private float flopTimerCurrent = 0f;
    private float flopCoyoteTimer = 0f;
    
    // ground check
    private float groundCheckCollDist;
    
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
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        groundCheckCollDist = Vector3.Distance(groundCheck.position, sphereCollider.transform.position);
        colliderCenter.transform.position = sphereCollider.transform.position;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Debug.DrawRay(wallCheck.position, transform.forward * wallDistance, Color.red);
        // Debug.Log(colliderCenter.transform.position.y);
        PositionGroundChecker();
        ReloadInput();
        MoveInput();
        CheckGrounded();
        JumpInput();
        DashInput();
        MoveCharacter();
    }

    private void MoveCharacter()
    {
        if (inWater)
        {
            flopCoyoteTimer = 0f;
            bool isMoving = rb.linearVelocity.sqrMagnitude > 0.01f;
            
            if (isMoving && !wasMoving)
            {
                FishyEvents.OnMovingWaterStart?.Invoke();
                // Debug.Log("Movement start fired");
            }
            if (wasMoving && !isMoving)
            {
                FishyEvents.OnMovingWaterEnd?.Invoke();
                // Debug.Log("Movement end fired");
            }
            
            wasMoving = isMoving;
            
            if (isAtSurface)
            {
                SurfaceMovement();
                if (bubblesps.isPlaying)
                {
                    bubblesps.Stop();
                }
            }
            else if (!IsJumping)
            {
                WaterMovement();
                if (!bubblesps.isPlaying)
                {
                    bubblesps.Play();
                }
            }
            rb.freezeRotation = true;
            // animator.enabled = true;
        }
        else
        {
            // animator.enabled = false;
            rb.freezeRotation = false;
            if (bubblesps.isPlaying)
            {
                bubblesps.Stop();
            }
            if (isGrounded)
            {
                flopCoyoteTimer = 0f;
                GroundMovement();
            }
            else
            {
                flopCoyoteTimer += Time.deltaTime;
                AirMovement();
            }
        }
    }

    private void PositionGroundChecker()
    {
        groundCheck.position = sphereCollider.transform.position - Vector3.up * groundCheckCollDist;
    }
    
    private void ReloadInput()
    {
        if (Input.GetKey(KeyCode.R))
        {
            rholdTimer += Time.deltaTime;

            if (rholdTimer >= rHoldTime)
            {
                CheckPointManager.Instance.LoadLastCheckpoint();
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
            vertical = forwardLocked ? 0f : Input.GetAxis("Vertical");
            horizontal = rightLocked ? 0f : Input.GetAxis("Horizontal");
            up = upLocked ? 0f : Input.GetAxis("Up");
        }

        forward = CamForwardFlat() * vertical;
        right = CamRightFlat() * horizontal;
        forward.y = 0;
        right.y = 0;
        upward = up * Vector3.up;
    }

    private void CheckGrounded()
    {
        if (isJumping)
        {
            if (!inWater && !((!IsJumping) && rb.linearVelocity.y > 0.001f))
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
        else
        {
            if (!inWater)
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

        if (isGrounded)
        {
            
        }
        else
        {
            flopTimerCurrent = 0f;
        }
    }

    private void JumpInput()
    {
        if ((!IsJumping && isGrounded) || (flopTimerCurrent < flopCoyote && (!inWater && !isGrounded)))
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Submit") || Input.GetButtonDown("Fire2"))
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
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Submit") || Input.GetButtonDown("Fire2"))
            {
                rb.useGravity = true;
                
                float startForce = minJumpForceWater;

                rb.linearVelocity = new Vector3(rb.linearVelocity.x, startForce, rb.linearVelocity.z);
                
                jumpMoveFactor = jumpMoveFactorFromWater;

                IsJumpingFromSurface = true;
                FishyEvents.OnJumpFromWater?.Invoke();
                isGrounded = false;
                isAtSurface = false;
                
                jumpHoldTimer = 0f;

                canCharge = true;
            }
        }

        if (IsJumpingFromSurface)
        {
            if (Input.GetKey(KeyCode.Space) || Input.GetButton("Submit") || Input.GetButton("Fire2"))
            {
                if ((jumpHoldTimer < maxAirCharge) && canCharge)
                {
                    jumpHoldTimer += Time.deltaTime;
                    float t = jumpHoldTimer / maxAirCharge;
                    float falloff = 1f - t;
                    float addedForce = airChargeForce * falloff * Time.deltaTime;
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y + addedForce,
                        rb.linearVelocity.z);
                }
            }
            
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Submit") || Input.GetButtonUp("Fire2"))
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
        FishyEvents.SetState(FishyStates.OnSurface);
        rb.useGravity = false;
        rb.mass = underWaterMass;
        
        if (dashTime > 0)
        {
            return;
        }
            
        forward.y = 0f;
        right.y = 0f;

        Vector3 swimMovement = forward + right;
        swimDirection = (forward + right).normalized;
            
        if(upward.y > 0)
        {
            upward.y = 0;
        }

        swimDirection += upward;
        swimMovement += upward;

        if (dashTime <= 0f)
        {
            rb.linearVelocity = swimMovement * maxSpeed;
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
        FishyEvents.SetState(FishyStates.InWater);
        rb.useGravity = false;
        rb.mass = underWaterMass;

        
        
        if (dashTime > 0)
        {
            return;
        }

        Vector3 swimVel = (forward + right + upward) * maxSpeed;

        if (dashTime <= 0f)
        {
            rb.linearVelocity = new Vector3(swimVel.x, swimVel.y, swimVel.z);
        }
        
        if (swimVel.magnitude > 0.01f)
        {
            RotateTo(swimVel.normalized);
            animator.SetBool("isSwiming", true);
        }
        else
        {
            animator.SetBool("isSwiming", false);
        }
    }

    private void GroundMovement()
    {
        rb.mass = 1f;
        FishyEvents.SetState(FishyStates.OnGround);
        forward.y = 0f;
        right.y = 0f;
        swimDirection = (forward + right).normalized;
        
        flopTimerCurrent += Time.deltaTime;
        if (flopTimerCurrent > flopTimer)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, flopForce, rb.linearVelocity.z);
            flopTimerCurrent = 0f;
        }

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
            // Debug.Log("Something in the way");
            rb.linearVelocity = Vector3.zero;
        }
        
        Quaternion lookRot = transform.rotation;

        if (swimDirection.magnitude > 0.1f)
        {
            var rotAxis = Vector3.Cross(Vector3.down, swimDirection.normalized);
            // lookRot = Quaternion.LookRotation(swimDirection.normalized);

            // Quaternion slopeRot = lookRot;
            // if (Physics.Raycast(groundCheck.transform.position, Vector3.down, out RaycastHit hit,
            //         groundDistance + 0.5f, ~0,
            //         QueryTriggerInteraction.Ignore))
            // {
            //     Vector3 swimDirectionRotated = Quaternion.AngleAxis(90f, Vector3.up) * swimDirection;
            //     Vector3 slopeDir = Vector3.ProjectOnPlane(swimDirectionRotated, hit.normal).normalized;
            //     slopeRot = Quaternion.LookRotation(slopeDir, Vector3.up);
            // }
            
            // var rotY = transform.rotation.eulerAngles.y;
            // rotY = 
            // Quaternion newRot =
            //     Quaternion.Euler(transform.rotation.eulerAngles.x, rotY, transform.rotation.eulerAngles.z);
            // Quaternion.Slerp(transform.rotation, newRot, 0.1f);
            Quaternion baseYaw = Quaternion.LookRotation(swimDirection.normalized);

            Quaternion plus90  = baseYaw * Quaternion.Euler(0f,  90f, transform.eulerAngles.z);
            Quaternion minus90 = baseYaw * Quaternion.Euler(0f, -90f, transform.eulerAngles.z);

            Quaternion targetYaw =
                Quaternion.Angle(transform.rotation, plus90) <
                Quaternion.Angle(transform.rotation, minus90)
                    ? plus90
                    : minus90;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetYaw,
                0.1f
            );
            
            // Debug.DrawRay(hit.point, hit.normal * 20f, Color.red);
            
            // RotateTo(slopeRot);
            rb.AddTorque(-rotAxis * torqueForce, ForceMode.Force);
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxRollVelocity);
            
            // rb.AddForce(Vector3.up * flopForce, ForceMode.Impulse);
        }
        else
        {
            // lookRot.x = 0f;

            Quaternion slopeRot = lookRot;
            if (Physics.Raycast(groundCheck.transform.position, Vector3.down, out RaycastHit hit,
                    groundDistance + 0.5f, ~0,
                    QueryTriggerInteraction.Ignore))
            {
                Vector3 slopeDir = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
                slopeRot = Quaternion.LookRotation(slopeDir, Vector3.up);
            }
            
            Debug.DrawRay(hit.point, hit.normal * 20f, Color.red);
            
            Quaternion targetYaw =
                Quaternion.Euler(0f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetYaw,
                0.1f
            );
            
            // RotateTo(slopeRot);
        }
    }

    private void AirMovement()
    {
        rb.mass = 1f;
        FishyEvents.SetState(FishyStates.InAir);
            
        swimDirection = (forward + right).normalized;
        rb.useGravity = true;
        
        var canGoDir = transform.forward;
        canGoDir.y = 0f;
        
        bool canGo = !Physics.Raycast(
            wallCheck.position,
            canGoDir,
            out RaycastHit hit,
            wallDistance,
            ~interactibleLayer,
            QueryTriggerInteraction.Ignore
        );
        if (canGo)
        {
            // Debug.Log("Something not in way");
            
            rb.linearVelocity = new Vector3(swimDirection.x * jumpMoveFactor, rb.linearVelocity.y,
                swimDirection.z * jumpMoveFactor);
            // Debug.Log(swimDirection);
            if (Math.Abs(rb.linearVelocity.x) + Math.Abs(rb.linearVelocity.z) > 0.1f && IsJumping)
            {
                if (IsJumpingFromSurface)
                {
                    Debug.Log("Jump from surface");
                    RotateTo(rb.linearVelocity);
                }
            }
            else
            {
                Vector3 vel = rb.linearVelocity.normalized;

                float pitch = Mathf.Atan2(vel.y, new Vector2(vel.x, vel.z).magnitude) * Mathf.Rad2Deg;

                Quaternion target = Quaternion.Euler(
                    -pitch,
                    transform.eulerAngles.y,
                    transform.eulerAngles.z
                );

                if (IsJumpingFromSurface)
                {
                    Debug.Log("Jump from surface");
                    RotateTo(target);
                }
            }
        }
        else
        {
            // Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
            // localVel.z = 0f;
            // rb.linearVelocity = transform.TransformDirection(localVel);
            
            rb.linearVelocity = new Vector3(swimDirection.x * jumpMoveFactor, rb.linearVelocity.y,
                swimDirection.z * jumpMoveFactor);

            Vector3 vel = rb.linearVelocity;
            
            Vector3 intoWall = Vector3.Project(vel, hit.normal);
            rb.linearVelocity = vel - intoWall;
            
            // Debug.Log("Local Velocity: " + localVel);
            
            Debug.Log("Something in the way");

            float pitch = Mathf.Atan2(vel.y, new Vector2(vel.x, vel.z).magnitude) * Mathf.Rad2Deg;

            Quaternion target = Quaternion.Euler(
                -pitch,
                transform.eulerAngles.y,
                transform.eulerAngles.z
            );
                
            if (IsJumpingFromSurface)
            {
                Debug.Log("Jump from surface");
                RotateTo(target);
            }
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
        if (other.CompareTag("Water") || other.CompareTag("WaterSurface"))
        {
            if (!inWater && !isAtSurface)
            {
                FishyEvents.OnWaterEntered?.Invoke();
            }
        }
        if (other.CompareTag("Water"))
        {
            inWater = true;
            // Debug.Log("Water triggered");
            IsJumping = false;
        }
        if (other.CompareTag("WaterSurface"))
        {
            // Debug.Log("Surface trigger");
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
            // Debug.Log("Surface trigger exit");
            isAtSurface = false;
        }

        if (other.CompareTag("Water"))
        {
            // Debug.Log("Water trigger exit");
            FishyEvents.OnMovingWaterEnd?.Invoke();
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
            FishyEvents.OnMovingWaterEnd?.Invoke();
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
        camForward = camForward.normalized;
        // Debug.Log("CamForward: " + camForward);
        return camForward;
    }
    
    Vector3 CamRightFlat()
    {
        Vector3 camRight = camera.transform.right;
        camRight.y = 0f;
        camRight = camRight.normalized;
        // Debug.Log("CamRight: " + camRight);
        return camRight;
    }

    public void SnapFishyTo(Vector3 location, Quaternion rotation)
    {
        rb.linearVelocity = Vector3.zero;
        transform.position = location;
        transform.rotation = rotation;
        
        Debug.Log("Fishy's position snapped to " + location);
    }

    public void LockMovement(bool forward, bool right, bool up)
    {
        forwardLocked = forward;
        rightLocked = right;
        upLocked = up;
    }

    public void UnlockMovement()
    {
        forwardLocked = false;
        rightLocked = false;
        upLocked = false;
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