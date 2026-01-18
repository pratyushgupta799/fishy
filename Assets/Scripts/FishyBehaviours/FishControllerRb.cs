using System;
using System.Collections;
using FishyUtilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Cursor = UnityEngine.Cursor;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class FishControllerRB : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject camera;
    [SerializeField] private Animator animator;
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private GameObject colliderCenter;

    [Header("Projectiles")] 
    [SerializeField] private GameObject waterSpill;
     
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float moveForce = 2f;
    [SerializeField] private float groundSpeedScale = 0.4f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float underWaterMass = 0.02f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashSpeed = 0.5f;
    
    [Header("Roll")]
    [SerializeField] private float torqueForce = 5f;
    [SerializeField] private float maxRollVelocity = 2f;
    
    [Header("Flop")]
    [SerializeField] private float flopTimer = 0.5f;
    [SerializeField] private float flopForce = 2f;
    [SerializeField] private float flopCoyote = 0.5f;
    [SerializeField] private Vector3 flopDirectionNoise;
    [SerializeField] private Vector3 flopRotationNoise;

    [Header("Twirl")] 
    [SerializeField] private float twirlTorqueForce = 0.3f;
    [SerializeField] private float twirlJumpForce = 2f;

    [Header("Spill")] 
    [SerializeField] private float spillCooldown = 1f;
    [SerializeField] private float spillDirectionalForce = 5f;
    [SerializeField] private float spillUpForce = 2f;
    [SerializeField] private float spillPuddleLifetime = 3f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpMoveFactorFromWater = 1.5f;
    [SerializeField] private float jumpMoveFactorFromGround = 0.5f;
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
    private Vector3 forwardDirectional;
    private Vector3 rightDirectional;
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
    
    // jump
    private float jumpHoldTimer = 0f;
    private bool canCharge = false;
    private bool jumpHeld = false;
    
    // movement
    private bool wasMoving;
    private bool forwardLocked;
    private bool rightLocked;
    private bool upLocked;
    
    // flop
    private bool canFlopJump;
    private bool canFlop = true;
    
    // surface
    private Vector3 surfaceNormal;
    private Vector3 curSurfacePos;
    
    // ground check
    private float groundCheckCollDist;
    
    // shake
    private bool canTwirl;
    private bool canSplash = true;
    private SpillBlobBehaviour _frontSpillBlob;
    private SpillBlobBehaviour _backSpillBlob;
    private SpillBlobBehaviour _leftSpillBlob;
    private SpillBlobBehaviour _rightSpillBlob;
    
    // dash
    private bool isDashing;
    
    // reset
    private bool resetHeld;
    
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

    private void OnEnable()
    {
        FishyEvents.OnFishyMoveStateChanged += StateManagement;
    }
    
    private void OnDisable()
    {
        FishyEvents.OnFishyMoveStateChanged -= StateManagement;
    }

    private void Awake()
    {
        _frontSpillBlob = Instantiate(waterSpill).GetComponent<SpillBlobBehaviour>();
        _backSpillBlob = Instantiate(waterSpill).GetComponent<SpillBlobBehaviour>();
        _leftSpillBlob = Instantiate(waterSpill).GetComponent<SpillBlobBehaviour>();
        _rightSpillBlob = Instantiate(waterSpill).GetComponent<SpillBlobBehaviour>();
        
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        groundCheckCollDist = Vector3.Distance(groundCheck.position, sphereCollider.transform.position);
        colliderCenter.transform.position = sphereCollider.transform.position;
        Cursor.lockState = CursorLockMode.Locked;
        animator.enabled = true;
    }

    private void Update()
    {
        PositionGroundChecker();
        ReloadInput();
        MoveInputProcess();
        CheckGrounded();
        JumpCharge();
        MoveCharacter();
    }

    private void PositionGroundChecker()
    {
        groundCheck.position = sphereCollider.transform.position - Vector3.up * groundCheckCollDist;
    }
    
    public void OnReload(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            resetHeld = true;
        }

        if (ctx.canceled)
        {
            resetHeld = false;
        }
    } 
    
    private void ReloadInput()
    {
        if (resetHeld)
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
    
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (isDashing)
        {
            vertical = 0f;
            horizontal = 0f;
        }
        else
        {
            vertical = forwardLocked? 0f : ctx.ReadValue<Vector2>().y;
            horizontal = rightLocked ? 0f : ctx.ReadValue<Vector2>().x;
        }
    }

    public void OnMoveVertical(InputAction.CallbackContext ctx)
    {
        if (isDashing)
        {
            up = 0f;
        }
        else
        {
            up = upLocked? 0f : ctx.ReadValue<float>();
        }
    }

    private void MoveInputProcess()
    {
        forward = CamForwardFlat() * vertical;
        right = CamRightFlat() * horizontal;
        forward.y = 0;
        right.y = 0;
        forwardDirectional = Vector3.ProjectOnPlane(forward, transform.up);
        rightDirectional = Vector3.ProjectOnPlane(right, transform.up);
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
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            Debug.Log("jump pressed");
            jumpHeld = true;

            if ((!IsJumping && isGrounded) || (canFlopJump && (!inWater && !isGrounded)))
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

            if (!IsJumping && isAtSurface)
            {
                rb.useGravity = true;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, minJumpForceWater, rb.linearVelocity.z);

                jumpMoveFactor = jumpMoveFactorFromWater;
                IsJumpingFromSurface = true;

                FishyEvents.OnJumpFromWater?.Invoke(groundCheck.position);

                isGrounded = false;
                isAtSurface = false;

                jumpHoldTimer = 0f;
                canCharge = true;
            }
        }

        if (ctx.canceled)
        {
            jumpHeld = false;
            canCharge = false;
            jumpHoldTimer = 0f;
        }
    }

    private void JumpCharge()
    {
        if (IsJumpingFromSurface && jumpHeld && canCharge)
        {
            if (jumpHoldTimer < maxAirCharge)
            {
                jumpHoldTimer += Time.deltaTime;

                float t = jumpHoldTimer / maxAirCharge;
                float falloff = 1f - t;
                float addedForce = airChargeForce * falloff * Time.deltaTime;

                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y + addedForce,
                    rb.linearVelocity.z
                );
            }
        }
    }

    public void OnShake(InputAction.CallbackContext ctx)
    {
        // dash
        if (inWater && !isAtSurface && !isDashing)
        {
            rb.AddForce(transform.forward * dashSpeed, ForceMode.VelocityChange);
            isDashing = true;
            StartCoroutine(Delay(dashDuration, () => isDashing = false));
        }

        // twirl
        if (!inWater && !isGrounded && canTwirl)
        {
            rb.AddTorque(Vector3.up * twirlTorqueForce, ForceMode.Impulse);
            float yVel = rb.linearVelocity.y;
            if (yVel >= 0)
            {
                float factor = Mathf.Clamp01(1f - (yVel / minJumpForceWater));
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, yVel + twirlJumpForce * factor,
                    rb.linearVelocity.z);
            }
            else
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, twirlJumpForce, rb.linearVelocity.z);
            }

            canTwirl = false;
            if (IsJumpingFromSurface)
            {
                IsJumping = false;
                StartCoroutine(Delay(0.5f, () => { IsJumpingFromSurface = true; }));
            }
        }

        // spill
        if (isAtSurface && canSplash)
        {
            StartCoroutine(TransformSpinOnce());

            _frontSpillBlob.Init(groundCheck.position,
                (transform.forward * spillDirectionalForce + Vector3.up * spillUpForce).normalized,
                spillPuddleLifetime);
            _backSpillBlob.Init(groundCheck.position,
                (-transform.forward * spillDirectionalForce + Vector3.up * spillUpForce).normalized,
                spillPuddleLifetime);
            _leftSpillBlob.Init(groundCheck.position,
                (-transform.right * spillDirectionalForce + Vector3.up * spillUpForce).normalized,
                spillPuddleLifetime);
            _rightSpillBlob.Init(groundCheck.position,
                (transform.right * spillDirectionalForce + Vector3.up * spillUpForce).normalized,
                spillPuddleLifetime);

            canSplash = false;
            StartCoroutine(Delay(spillCooldown, () => { canSplash = true; }));
        }
    }

    private void MoveCharacter()
    {
        if (inWater)
        {
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
            }
            else if (!IsJumping)
            {
                WaterMovement();
            }
            rb.freezeRotation = true;
        }
        else
        {
            rb.freezeRotation = false;
            if (isGrounded)
            {
                GroundMovement();
            }
            else
            {
                AirMovement();
            }
        }
    }

    private void SurfaceMovement()
    {
        FishyEvents.SetState(FishyStates.OnSurface);
        rb.useGravity = false;
        rb.mass = underWaterMass;
            
        forward.y = 0f;
        right.y = 0f;

        Vector3 swimMovement = forwardDirectional + rightDirectional;
        swimDirection = (forwardDirectional + rightDirectional).normalized;
            
        if(upward.y > 0)
        {
            upward.y = 0;
        }

        swimDirection += upward;
        swimMovement += upward;

        rb.linearVelocity = swimMovement * maxSpeed;
        
        if (Mathf.Abs(Vector3.Dot(swimDirection.normalized, Vector3.up)) > 0.99f)
        {
            // keep vertical direction, but set XZ from camera
            Vector3 horizontalDir = new Vector3(camera.transform.forward.x, 0f,camera.transform.forward.z).normalized;
            swimDirection = new Vector3(horizontalDir.x * 0.0001f, swimDirection.y, horizontalDir.z * 0.0001f)
                .normalized;
        }
            
        if (swimDirection.magnitude > 0.1f)
        {
            // RotateTo(swimDirection.normalized);
            
            RotateTo(swimDirection.normalized);
            animator.SetBool("isSwiming", true);
        }
        else
        {
            animator.SetBool("isSwiming", false);
        }

        // Keep near surface
        if (up >= 0 && !IsJumping)
        {
            rb.position = Vector3.Lerp(rb.position, new Vector3(rb.position.x, curSurfacePos.y, rb.position.z),
                10f * Time.deltaTime);
            Vector3 direction = Vector3.ProjectOnPlane(transform.forward, surfaceNormal);
            RotateTo(direction);
        }
    }

    private void WaterMovement()
    {
        FishyEvents.SetState(FishyStates.InWater);
        rb.useGravity = false;
        rb.mass = underWaterMass;
        
        if (isDashing)
        {
            return;
        }

        Vector3 swimVel = (forward + right + upward) * maxSpeed;

        if (!isDashing)
        {
            rb.linearVelocity = new Vector3(swimVel.x, swimVel.y, swimVel.z);
        }
        
        if (Mathf.Abs(Vector3.Dot(swimVel.normalized, Vector3.up)) > 0.99f)
        {
            // keep vertical direction, but set XZ from camera
            Vector3 horizontalDir = new Vector3(camera.transform.forward.x, 0f, camera.transform.forward.z).normalized;
            swimVel = new Vector3(horizontalDir.x * 0.0001f, swimVel.y, horizontalDir.z * 0.0001f).normalized;
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
        
        if (canFlop)
        {
            Flop();
            canFlop = false;
            StartCoroutine(Delay(flopTimer, () => { canFlop = true; }));
            canFlopJump = true;
            StartCoroutine(Delay(flopCoyote, () => { canFlopJump = false; }));
        }

        IsJumping = false;
        rb.useGravity = true;
        rb.linearVelocity = new Vector3(swimDirection.x * groundSpeedScale, rb.linearVelocity.y,
                swimDirection.z * groundSpeedScale);

        if (swimDirection.magnitude > 0.1f)
        {
            var rotAxis = Vector3.Cross(Vector3.down, swimDirection.normalized);
            Quaternion baseYaw = Quaternion.LookRotation(swimDirection.normalized);

            Quaternion plus90  = baseYaw * Quaternion.Euler(0f,  90f, transform.eulerAngles.z);
            Quaternion minus90 = baseYaw * Quaternion.Euler(0f, -90f, transform.eulerAngles.z);

            Quaternion targetYaw =
                Quaternion.Angle(transform.rotation, plus90) <
                Quaternion.Angle(transform.rotation, minus90)
                    ? plus90
                    : minus90;
            
            RotateToSlow(targetYaw);
            
            rb.AddTorque(-rotAxis * torqueForce, ForceMode.Force);
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxRollVelocity);
        }
        else
        {
            Quaternion targetYaw =
                Quaternion.Euler(0f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            
            RotateTo(targetYaw);
            
            Quaternion targetYaw2_1 =
                Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 90f);
            
            Quaternion targetYaw2_2 =
                Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, -90f);
            
            Quaternion targetYaw2 =
                Quaternion.Angle(transform.rotation, targetYaw2_1) <
                Quaternion.Angle(transform.rotation, targetYaw2_2)
                    ? targetYaw2_1
                    : targetYaw2_2;
            
            RotateToFast(targetYaw2);
        }
    }

    private void AirMovement()
    {
        rb.mass = 1f;
        FishyEvents.SetState(FishyStates.InAir);

        swimDirection = (forward + right).normalized;
        rb.useGravity = true;

        rb.linearVelocity = new Vector3(swimDirection.x * jumpMoveFactor, rb.linearVelocity.y,
            swimDirection.z * jumpMoveFactor);
        // Debug.Log(swimDirection);
        
        if (Math.Abs(rb.linearVelocity.x) + Math.Abs(rb.linearVelocity.z) > 0.1f && IsJumping)
        {
            if (IsJumpingFromSurface)
            {
                // Debug.Log("Jump from surface");
                RotateTo(rb.linearVelocity);
            }
        }
        else
        {
            if (IsJumpingFromSurface)
            {
                Vector3 vel = rb.linearVelocity.normalized;

                float pitch = Mathf.Atan2(vel.y, new Vector2(vel.x, vel.z).magnitude) * Mathf.Rad2Deg;

                Quaternion target = Quaternion.Euler(
                    -pitch,
                    transform.eulerAngles.y,
                    transform.eulerAngles.z
                );
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
    
    private void RotateToFast(Quaternion target)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation,
            target,
            turnSmoothTime * 2f * Time.deltaTime);
    }
    
    private void RotateToSlow(Quaternion target)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation,
            target,
            turnSmoothTime * 0.1f * Time.deltaTime);
    }

    private void Flop()
    {
        Vector3 flopDirectionBase = new Vector3(rb.linearVelocity.x, flopForce, rb.linearVelocity.z);
        Vector3 flopDirection = GetFlopDirectionNoise(flopDirectionBase);
        rb.linearVelocity = 1.5f * flopForce * flopDirection.normalized;
        rb.AddTorque(GetFlopRotationNoise(), ForceMode.Impulse);
        jumpMoveFactor = jumpMoveFactorFromGround;
    }

    private Vector3 GetFlopDirectionNoise(Vector3 direction)
    {
        direction.x += Random.Range(-flopDirectionNoise.x, flopDirectionNoise.x);
        direction.y += Random.Range(-flopDirectionNoise.y, flopDirectionNoise.y);
        direction.z += Random.Range(-flopDirectionNoise.z, flopDirectionNoise.z);
        return direction;
    }

    private Vector3 GetFlopRotationNoise()
    {
        Vector3 direction = Vector3.zero;
        direction.x += Random.Range(-flopRotationNoise.x, flopRotationNoise.x);
        direction.y += Random.Range(-flopRotationNoise.y, flopRotationNoise.y);
        direction.z += Random.Range(-flopRotationNoise.z, flopRotationNoise.z);
        return direction;
    }

    private void StateManagement(FishyStates state)
    {
        if (state == FishyStates.InAir)
        {
            canTwirl = true;
        }

        if (state == FishyStates.InWater || state == FishyStates.OnSurface)
        {
            animator.SetBool("inWater", true);
        }
        else
        {
            animator.SetBool("inWater", false);
            animator.SetBool("isSwiming", false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water") || other.CompareTag("WaterSurface"))
        {
            if (!inWater && !isAtSurface)
            {
                FishyEvents.OnWaterEntered?.Invoke(other.ClosestPoint(transform.position));
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
                surfaceNormal = other.transform.up;
                curSurfacePos = other.ClosestPoint(transform.position);
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
                surfaceNormal = other.transform.up;
                curSurfacePos = other.ClosestPoint(transform.position);
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
    
    IEnumerator Delay(float t, Action a)
    {
        yield return new WaitForSeconds(t);
        a();
    }
    
    IEnumerator TransformSpinOnce()
    {
        LockMovement(true, true, true);
        float rotated = 0f;

        while (rotated < 360f)
        {
            float step = 360f * Time.deltaTime * 2f;
            transform.Rotate(0f, step, 0f);
            rotated += step;
            yield return null;
        }
        
        UnlockMovement();
    }

    public bool IsDashing()
    {
        return isDashing;
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