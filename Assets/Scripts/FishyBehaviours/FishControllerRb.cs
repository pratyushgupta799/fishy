using System;
using System.Collections;
using DG.Tweening;
using FishyUtilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    
    [Header("Dash")]
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashSpeed = 0.5f;

    [Header("Surface movement")] 
    [SerializeField] private float surfaceHeightOffset = 0.1f;

    [Header("Surface Dip")] 
    [SerializeField] private float maxSurfaceDip = 0.15f;
    [SerializeField] private float minSurfaceDip = 0.05f;
    
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
    [SerializeField] private Ease spillRotationEase;
    [SerializeField] private float spillRotationDuration = 0.5f;

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
    
    [Header("Water check parameters")]
    [SerializeField] private Transform waterCheck;
    [SerializeField] private float waterDistance = 0.3f;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private float colliderBorderOffset = 0.02f;

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
    private float jumpMoveFactor;
    
    // movement
    private bool wasMoving;
    
    // movement locks
    private bool snapRotationSideways;
    private bool forwardLocked;
    private bool rightLocked;
    private bool upLocked;
    
    // flop
    private bool canFlopJump;
    private bool canFlop = true;
    private bool flopStarted;
    private float flopTimerCurrent = 0;
    
    // surface
    private Vector3 surfaceNormal;
    private Vector3 curSurfacePos;
    
    // ground check
    private float groundCheckCollDist;
    private float waterCheckCollDist;
    
    // shake
    private bool canTwirl;
    private bool canSplash = true;
    private bool splashSpinning = false;
    private bool onSpillSurface = false;
    
    // spill blob refs
    private SpillBlobBehaviour _frontSpillBlob;
    private SpillBlobBehaviour _backSpillBlob;
    private SpillBlobBehaviour _leftSpillBlob;
    private SpillBlobBehaviour _rightSpillBlob;
    
    // dash
    private bool isDashing;
    
    // reset
    private bool resetHeld;
    
    // surface transition
    private bool inSurfaceTransition;
    private Tween surfaceTween;
    
    // cam snapper
    private float xSnap;
    private float zSnap;
    private bool snapToX;
    private bool snapToZ;
    
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
    
    private bool InSurfaceTransition
    {
        get
        {
            return inSurfaceTransition;
        }
        set
        {
            if (value == false)
            {
                // Debug.Log("Surface transition stopped");
                inSurfaceTransition = false;
                surfaceTween?.Kill();
            }
            else
            {
                inSurfaceTransition = true;
                // Debug.Log("Surface transition started");
            }
        }
    }

    private void OnEnable()
    {
        FishyEvents.OnFishyMoveStateChanged += StateManagement;
        FishyEvents.OnSurfaceReachedFromAir += SurfaceDip;
    }
    
    private void OnDisable()
    {
        FishyEvents.OnFishyMoveStateChanged -= StateManagement;
        FishyEvents.OnSurfaceReachedFromAir += SurfaceDip;
    }

    private void Awake()
    {
        // var pi = GetComponent<PlayerInput>();
        // pi.defaultControlScheme = null;
        
        _frontSpillBlob = Instantiate(waterSpill).GetComponent<SpillBlobBehaviour>();
        _backSpillBlob = Instantiate(waterSpill).GetComponent<SpillBlobBehaviour>();
        _leftSpillBlob = Instantiate(waterSpill).GetComponent<SpillBlobBehaviour>();
        _rightSpillBlob = Instantiate(waterSpill).GetComponent<SpillBlobBehaviour>();
        
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        groundCheckCollDist = Vector3.Distance(groundCheck.position, sphereCollider.bounds.center);
        waterCheckCollDist = Vector3.Distance(waterCheck.position, sphereCollider.transform.position);
        colliderCenter.transform.position = sphereCollider.transform.position;
        Cursor.lockState = CursorLockMode.Locked;
        animator.enabled = true;
    }

    private void Update()
    {
        PositionGroundChecker();
        PositionWaterChecker();
        ReloadInput();
        MoveInputProcess();
        CheckGrounded();
        JumpCharge();
        WaterCheck();
        MoveCharacter();
    }

    private void PositionGroundChecker()
    {
        groundCheck.position = sphereCollider.bounds.center - Vector3.up * groundCheckCollDist;
    }
    
    private void PositionWaterChecker()
    {
        waterCheck.position =
            sphereCollider.bounds.center +
            Vector3.down * (sphereCollider.radius - (waterDistance + colliderBorderOffset));
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
                if (CheckPointManager.Instance != null)
                {
                    CheckPointManager.Instance.LoadLastCheckpoint();
                }
                else
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
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

        if (up < 0f)
        {
            if (InSurfaceTransition)
            {
                InSurfaceTransition = false;
            }
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
    
    private void WaterCheck()
    {
        if (Physics.CheckSphere(
                waterCheck.position,
                waterDistance,
                waterLayer))
        {
            if (!inWater)
            {
                FishyEvents.OnWaterEntered?.Invoke();
                IsJumping = false;
            }

            inWater = true;
            inWaterThisFrame = true;
            waterExitTime = Time.time + waterExitGrace;
        }
        else
        {
            inWaterThisFrame = false;
        }
    }

    private void CheckGrounded()
    {
        if (IsJumping)
        {
            if (!inWater && (rb.linearVelocity.y < 0.01f))
            {
                isGrounded = GroundCheckRaycast();

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
                isGrounded = GroundCheckRaycast();
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
            if ((!IsJumping && isGrounded) || (canFlopJump && (!inWater && !isGrounded)))
            {
                // Debug.Log("jump from ground");
                flopStarted = false;
                canFlopJump = false;
                // canFlop = false;
                rb.useGravity = true;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForceGround, rb.linearVelocity.z);

                IsJumpingFromGround = true;
                isAtSurface = false;
                isGrounded = false;

                jumpMoveFactor = jumpMoveFactorFromGround;

                jumpHoldTimer = 0f;
                return;
            }

            if ((!IsJumping && isAtSurface) || InSurfaceTransition)
            {
                // Debug.Log("jump from surface");
                IsJumpingFromSurface = true;
                if (InSurfaceTransition)
                {
                    InSurfaceTransition = false;
                }
                
                jumpHeld = true;
                rb.useGravity = true;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, minJumpForceWater, rb.linearVelocity.z);

                jumpMoveFactor = jumpMoveFactorFromWater;

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
        // Debug.Log("Shake pressed");
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
            // Debug.Log("Twirl");
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
        if (isAtSurface && canSplash && !onSpillSurface)
        {
            splashSpinning = true;
            PuddleManager.Instance.GetSpillPuddle();
            transform.DORotate(new Vector3(0,360,0), spillRotationDuration, RotateMode.LocalAxisAdd)
             .SetEase(spillRotationEase)
             .OnComplete(() => splashSpinning = false);

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
            
            if (isAtSurface || InSurfaceTransition)
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
        if (IsJumpingFromSurface && rb.linearVelocity.y > 0f)
        {
            return;
        }
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
            
        if (swimDirection.magnitude > 0.1f && !splashSpinning)
        {
            RotateTo(swimDirection.normalized);
            animator.SetBool("isSwiming", true);
        }
        else
        {
            if (snapRotationSideways)
            {
                RotateToSideway();
            }
            animator.SetBool("isSwiming", false);
        }

        // snap to surface
        if (!IsJumpingFromSurface)
        {
            if (!InSurfaceTransition)
            {
                rb.position = Vector3.Lerp(rb.position,
                    new Vector3(rb.position.x, curSurfacePos.y + surfaceHeightOffset, rb.position.z),
                    10f * Time.deltaTime);
            }
            Vector3 direction = Vector3.ProjectOnPlane(transform.forward, surfaceNormal);
            RotateTo(direction);
        }
    }

    private void SurfaceDip(Vector3 pos)
    {
        var freeSpaceBelow = GetFreeSpaceBelow(maxSurfaceDip, pos);

        InSurfaceTransition = true;

        surfaceTween = DOTween.Sequence()
            .Append(rb.DOMoveY(pos.y, 0.1f))
            .Append(
                rb.DOMoveY(-freeSpaceBelow, 0.25f)
                    .SetRelative(true)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.OutQuad)
            )
            .OnComplete(() =>
            {
                InSurfaceTransition = false;
            });
    }

    private void WaterMovement()
    {
        if (InSurfaceTransition)
        {
            return;
        }
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
            if (snapRotationSideways)
            {
                RotateToSideway();
            }
            animator.SetBool("isSwiming", false);
        }
    }

    private void GroundMovement()
    {
        if (InSurfaceTransition)
        {
            return;
        }
        rb.mass = 1f;
        FishyEvents.SetState(FishyStates.OnGround);
        forward.y = 0f;
        right.y = 0f;
        swimDirection = (forward + right).normalized;
        
        flopTimerCurrent += Time.deltaTime;
        if (flopTimerCurrent >= flopTimer)
        {
            flopTimerCurrent = 0f;
            flopStarted = false;
            Flop();
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

    private void Flop()
    {
        // Debug.Log("Flop");
        Vector3 flopDirectionBase = new Vector3(rb.linearVelocity.x, flopForce, rb.linearVelocity.z);
        Vector3 flopDirection = GetFlopDirectionNoise(flopDirectionBase);
        rb.linearVelocity = 1.5f * flopForce * flopDirection.normalized;
        rb.AddTorque(GetFlopRotationNoise(), ForceMode.Impulse);
        jumpMoveFactor = jumpMoveFactorFromGround;
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
            flopStarted = false;
        }
        else
        {
            animator.SetBool("inWater", false);
            animator.SetBool("isSwiming", false);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (CollisionUtils.HitByWithVelocity(other, 0.5f))
        {
            if (IsJumpingFromSurface)
            {
                IsJumpingFromSurface = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WaterSurface") || other.CompareTag("PuddleSurface"))
        {
            if(IsJumpingFromSurface && rb.linearVelocity.y > 0f)
            {
                return;
            }
            // Debug.Log("Surface trigger");
            if (inWater)
            {
                if (IsJumping) IsJumping = false;
                if (other.CompareTag("PuddleSurface"))
                {
                    onSpillSurface = true;
                }
                surfaceNormal = other.transform.up;
                curSurfacePos = other.ClosestPoint(transform.position);
                if (!isAtSurface)
                {
                    if (transform.position.y > curSurfacePos.y)
                    {
                        FishyEvents.OnSurfaceReachedFromAir.Invoke(curSurfacePos);
                    }
                    else
                    {
                        Debug.Log("Fishy Height: " + transform.position.y + ", Surface height: " + curSurfacePos.y);
                    }
                }
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
        if (other.CompareTag("WaterSurface") || other.CompareTag("PuddleSurface"))
        {
            if(IsJumpingFromSurface && rb.linearVelocity.y > 0f)
            {
                return;
            }
            // Debug.Log("Water surface trigger stay");
            if (inWater && !isJumping)
            {
                surfaceNormal = other.transform.up;
                curSurfacePos = other.ClosestPoint(transform.position);
                if (!isAtSurface)
                {
                    if (transform.position.y > curSurfacePos.y)
                    {
                        FishyEvents.OnSurfaceReachedFromAir.Invoke(curSurfacePos);
                    }
                    else
                    {
                        Debug.Log("Fishy Height: " + transform.position.y + ", Surface height: " + curSurfacePos.y);
                    }
                }
                isAtSurface = true;
                if (other.CompareTag("PuddleSurface"))
                {
                    onSpillSurface = true;
                }
                onSurfaceThisFrame = true;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                surfaceHeight = other.transform.position.y;
                
                surfaceExitTime = Time.time + surfaceExitGrace;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WaterSurface") || other.CompareTag("PuddleSurface"))
        {
            // Debug.Log("Surface trigger exit");
            isAtSurface = false;
            onSpillSurface = false;
        }
    }

    private void LateUpdate()
    {
        if (!onSurfaceThisFrame && isAtSurface)
        {
            isAtSurface = false;
            onSpillSurface = false;
        }
        else
        {
            // Debug.Log("is on surface this frame");
        }

        if (!inWaterThisFrame && inWater)
        {
            inWater = false;
            isAtSurface = false;
            onSpillSurface = false;
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

        if (snapToX)
        {
            transform.position = Vector3.Lerp(transform.position,
                new Vector3(xSnap, transform.position.y, transform.position.z), turnSmoothTime);
        }

        if (snapToZ)
        {
            transform.position = Vector3.Lerp(transform.position,
                new Vector3(transform.position.x, transform.position.y, zSnap), turnSmoothTime);
        }
    }

    public void LockMovement(bool forward, bool right, bool up)
    {
        forwardLocked = forward;
        if (forward)
        {
            snapRotationSideways = true;
        }
        rightLocked = right;
        upLocked = up;
    }

    #region Helpers

    private bool GroundCheckRaycast()
    {
        if (Physics.CheckSphere(
                groundCheck.position,
                groundDistance,
                ~fishyLayer,
                QueryTriggerInteraction.Ignore))
        {
            return true;
        }
        return false;
    }
    
    float GetFreeSpaceBelow(float maxDistance, Vector3 pos)
    {
        // 1. Get the very bottom center point of the collider in world space
        Vector3 rayStart = new Vector3(sphereCollider.bounds.center.x,
            sphereCollider.bounds.min.y,
            sphereCollider.bounds.center.z);

        // 2. Fire the ray
        if (Physics.Raycast(
                rayStart,
                Vector3.down,
                out RaycastHit hit,
                maxDistance,
                ~fishyLayer,
                QueryTriggerInteraction.Ignore))
        {
            return hit.distance - (transform.position.y - pos.y);
        }

        Debug.DrawRay(rayStart, Vector3.down * maxDistance, Color.red);
        return maxDistance;
    }

    private void RotateTo(Vector3 target)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, 
            Quaternion.LookRotation(target),
            turnSmoothTime * Time.deltaTime);
    }

    private void RotateToSideway()
    {
        Vector3 camRight = CamRightFlat();
        
        Vector3 camLeft  = -camRight;

        Vector3 current = transform.forward;

        Vector3 targetDir =
            Vector3.Dot(current, camRight) > Vector3.Dot(current, camLeft)
                ? camRight
                : camLeft;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(targetDir),
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
        
        // Debug.Log("Fishy's position snapped to " + location);
    }

    public void SnapToX(float x)
    {
        snapToX = true;
        xSnap = x;
    }

    public void SnapToZ(float z)
    {
        snapToZ = true;
        zSnap = z;
    }
    
    public void UnlockMovement()
    {
        snapRotationSideways = false;
        forwardLocked = false;
        rightLocked = false;
        upLocked = false;
        snapToX = false;
        snapToZ = false;
    }
    
    IEnumerator Delay(float t, Action a)
    {
        yield return new WaitForSeconds(t);
        a();
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }

        if (waterCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(waterCheck.position, waterDistance);
        }
    }
#endif
}