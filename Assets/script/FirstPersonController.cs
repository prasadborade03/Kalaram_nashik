using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Natural human walkthrough first-person controller.
/// Features smooth acceleration/deceleration, realistic bipedal head bobbing,
/// breathing idle motion, camera banking on strafe, randomized stone footstep audio,
/// and support for both Unity's new Input System and legacy Input.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [Tooltip("Natural human walking speed in m/s (ideal for architectural exploration)")]
    [SerializeField] private float walkSpeed = 3.2f;

    [Tooltip("Jogging/sprinting speed with Left Shift")]
    [SerializeField] private float sprintSpeed = 5.5f;

    [Tooltip("Backward movement speed multiplier")]
    [SerializeField] private float backwardSpeedMultiplier = 0.75f;

    [Tooltip("Strafing movement speed multiplier")]
    [SerializeField] private float strafeSpeedMultiplier = 0.85f;

    [Header("Acceleration & Natural Inertia")]
    [Tooltip("How smoothly player speeds up to target speed (lower = smoother inertia)")]
    [SerializeField] private float acceleration = 9f;

    [Tooltip("How smoothly player comes to a halt (lower = more coast/momentum)")]
    [SerializeField] private float deceleration = 11f;

    [Header("Jumping & Gravity")]
    [Tooltip("Allow gentle jumping")]
    [SerializeField] private bool canJump = true;

    [Tooltip("Jump apex height in meters")]
    [SerializeField] private float jumpHeight = 0.9f;

    [Tooltip("Gravity acceleration (m/s^2). Default provides grounded, stable feel")]
    [SerializeField] private float gravity = -18f;

    [Tooltip("Downward grounding force when standing on slopes/stairs")]
    [SerializeField] private float stickToGroundForce = -3.5f;

    [Header("Look & Camera")]
    [Tooltip("Reference to the camera pivot / head Transform")]
    [SerializeField] private Transform cameraHolder;

    [Tooltip("Horizontal look sensitivity")]
    [SerializeField] private float mouseSensitivityX = 1.8f;

    [Tooltip("Vertical look sensitivity")]
    [SerializeField] private float mouseSensitivityY = 1.8f;

    [Tooltip("Enable smooth mouse damping for cinematic walkthroughs")]
    [SerializeField] private bool smoothLook = true;

    [Tooltip("Damping time for smooth look")]
    [SerializeField] private float lookSmoothTime = 0.04f;

    [Tooltip("Minimum vertical pitch angle (looking down)")]
    [SerializeField] private float minPitch = -85f;

    [Tooltip("Maximum vertical pitch angle (looking up)")]
    [SerializeField] private float maxPitch = 85f;

    [Tooltip("Subtle camera roll/tilt when strafing (adds natural body sway)")]
    [SerializeField] private float strafeTiltAmount = 1.2f;

    [Header("Head Bobbing (Walkthrough Feel)")]
    [Tooltip("Enable natural human walking head bob")]
    [SerializeField] private bool enableHeadBob = true;

    [Tooltip("Frequency of footsteps (approx 1.8 - 2.0 steps/sec)")]
    [SerializeField] private float bobFrequency = 1.85f;

    [Tooltip("Horizontal side-to-side head sway amplitude")]
    [SerializeField] private float bobHorizontalAmplitude = 0.025f;

    [Tooltip("Vertical head dip amplitude (occurs twice per stride)")]
    [SerializeField] private float bobVerticalAmplitude = 0.04f;

    [Tooltip("How fast head bobbing resets when stopping")]
    [SerializeField] private float bobResetSpeed = 6f;

    [Header("Idle Breathing")]
    [Tooltip("Subtle breathing sway when standing still so view feels alive")]
    [SerializeField] private bool enableBreathing = true;

    [Tooltip("Breathing cycle frequency (breaths per second)")]
    [SerializeField] private float breathingFrequency = 0.35f;

    [Tooltip("Vertical breathing displacement amplitude")]
    [SerializeField] private float breathingAmplitude = 0.007f;

    [Header("Footsteps Audio (Stone Steps)")]
    [Tooltip("Audio source for playing footstep sounds")]
    [SerializeField] private AudioSource footstepAudioSource;

    [Tooltip("Array of stone footstep sound variations")]
    [SerializeField] private AudioClip[] stoneFootsteps;

    [Tooltip("Time interval between steps while walking (seconds)")]
    [SerializeField] private float walkStepInterval = 0.52f;

    [Tooltip("Time interval between steps while sprinting (seconds)")]
    [SerializeField] private float sprintStepInterval = 0.36f;

    [Tooltip("Base volume for footsteps (kept minimal and atmospheric)")]
    [Range(0f, 1f)]
    [SerializeField] private float footstepVolume = 0.18f;

    [Tooltip("Subtle random volume variation per step")]
    [Range(0f, 0.1f)]
    [SerializeField] private float volumeVariation = 0.03f;

    [Tooltip("Subtle random pitch variation (+/-) per step")]
    [Range(0f, 0.15f)]
    [SerializeField] private float pitchVariation = 0.06f;

    [Header("Cursor Settings")]
    [Tooltip("Lock cursor on start")]
    [SerializeField] private bool autoLockCursor = true;

    // Components & State
    private CharacterController _characterController;
    private Vector3 _currentVelocity;
    private float _verticalVelocity;
    private float _currentPitch;
    private float _targetPitch;
    private float _currentYaw;
    private float _targetYaw;
    private float _currentStrafeTilt;
    private Vector2 _currentMouseLook;
    private Vector2 _mouseLookVelocity;

    // Head bob state
    private float _bobTimer;
    private float _breathingTimer;
    private Vector3 _initialCameraLocalPos;

    // Footsteps state
    private float _stepTimer;
    private int _lastFootstepIndex = -1;
    private bool _isSprinting;

    public float HorizontalSpeed => new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z).magnitude;
    public bool IsGrounded => _characterController.isGrounded;
    public bool IsSprintingActive => _isSprinting && HorizontalSpeed > 0.3f;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (cameraHolder == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraHolder = cam.transform;
            }
        }

        if (cameraHolder != null)
        {
            _initialCameraLocalPos = cameraHolder.localPosition;
        }

        _targetYaw = transform.eulerAngles.y;
        _currentYaw = _targetYaw;

        if (cameraHolder != null)
        {
            _targetPitch = cameraHolder.localEulerAngles.x;
            if (_targetPitch > 180f) _targetPitch -= 360f;
            _currentPitch = _targetPitch;
        }

        SetupAudioSource();
    }

    private void SetupAudioSource()
    {
        if (footstepAudioSource == null)
        {
            footstepAudioSource = GetComponent<AudioSource>();
            if (footstepAudioSource == null)
            {
                footstepAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        footstepAudioSource.playOnAwake = false;
        footstepAudioSource.loop = false;
        footstepAudioSource.spatialBlend = 0f; // Clean 2D playback for player's own footsteps
    }

    private void Start()
    {
        if (autoLockCursor)
        {
            SetCursorLock(true);
        }
    }

    private void Update()
    {
        HandleCursorState();
        HandleLook();
        HandleMovement();
        HandleHeadBobAndBreathing();
        HandleFootsteps();
    }

    private void HandleCursorState()
    {
        // ESC unlocks cursor
        if (IsEscapePressed())
        {
            SetCursorLock(false);
        }

        // Left click re-locks cursor
        if (IsLeftClickPressed() && Cursor.lockState != CursorLockMode.Locked)
        {
            SetCursorLock(true);
        }
    }

    private void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 rawMouse = GetMouseInput();
        Vector2 targetLookDelta = new Vector2(rawMouse.x * mouseSensitivityX, rawMouse.y * mouseSensitivityY);

        if (smoothLook)
        {
            _currentMouseLook = Vector2.SmoothDamp(_currentMouseLook, targetLookDelta, ref _mouseLookVelocity, lookSmoothTime);
        }
        else
        {
            _currentMouseLook = targetLookDelta;
        }

        // Yaw (turn whole player body)
        _targetYaw += _currentMouseLook.x;
        _currentYaw = _targetYaw;
        transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);

        // Pitch (tilt camera up/down)
        _targetPitch -= _currentMouseLook.y;
        _targetPitch = Mathf.Clamp(_targetPitch, minPitch, maxPitch);
        _currentPitch = _targetPitch;

        // Strafe roll banking
        Vector2 inputMove = GetMovementInput();
        float targetTilt = -inputMove.x * strafeTiltAmount;
        _currentStrafeTilt = Mathf.Lerp(_currentStrafeTilt, targetTilt, Time.deltaTime * 6f);

        if (cameraHolder != null)
        {
            cameraHolder.localRotation = Quaternion.Euler(_currentPitch, 0f, _currentStrafeTilt);
        }
    }

    private void HandleMovement()
    {
        Vector2 input = GetMovementInput();
        _isSprinting = IsSprintPressed() && input.y > 0.1f;

        // Calculate target forward & strafe speed
        float baseSpeed = _isSprinting ? sprintSpeed : walkSpeed;
        float forwardSpeed = input.y > 0 ? baseSpeed : baseSpeed * backwardSpeedMultiplier;
        float sideSpeed = baseSpeed * strafeSpeedMultiplier;

        Vector3 moveDirection = (transform.forward * input.y * forwardSpeed) + (transform.right * input.x * sideSpeed);

        // Natural smooth acceleration and deceleration
        float accelRate = input.sqrMagnitude > 0.01f ? acceleration : deceleration;
        _currentVelocity = Vector3.MoveTowards(_currentVelocity, moveDirection, accelRate * Time.deltaTime);

        // Vertical physics
        if (_characterController.isGrounded)
        {
            if (_verticalVelocity < 0f)
            {
                _verticalVelocity = stickToGroundForce;
            }

            if (canJump && IsJumpPressed())
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 finalMotion = _currentVelocity + (Vector3.up * _verticalVelocity);
        _characterController.Move(finalMotion * Time.deltaTime);
    }

    private void HandleHeadBobAndBreathing()
    {
        if (cameraHolder == null) return;

        float groundSpeed = HorizontalSpeed;
        Vector3 targetOffset = Vector3.zero;

        // Bipedal Walking Bob (Figure-8 / dip stride)
        if (enableHeadBob && _characterController.isGrounded && groundSpeed > 0.2f)
        {
            float speedFactor = Mathf.Clamp01(groundSpeed / walkSpeed);
            _bobTimer += Time.deltaTime * bobFrequency * Mathf.PI * 2f * speedFactor;

            // Horizontal bob at 1x frequency, vertical bob at 2x frequency
            float hBob = Mathf.Sin(_bobTimer * 0.5f) * bobHorizontalAmplitude * speedFactor;
            float vBob = Mathf.Sin(_bobTimer) * bobVerticalAmplitude * speedFactor;

            targetOffset += new Vector3(hBob, vBob, 0f);
        }
        else
        {
            _bobTimer = Mathf.MoveTowards(_bobTimer, 0f, Time.deltaTime * bobResetSpeed);
        }

        // Idle Breathing
        if (enableBreathing)
        {
            _breathingTimer += Time.deltaTime * breathingFrequency * Mathf.PI * 2f;
            float breath = Mathf.Sin(_breathingTimer) * breathingAmplitude;
            targetOffset += new Vector3(0f, breath, 0f);
        }

        // Smoothly apply local position
        Vector3 targetLocalPos = _initialCameraLocalPos + targetOffset;
        cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, targetLocalPos, Time.deltaTime * 12f);
    }

    private void HandleFootsteps()
    {
        if (stoneFootsteps == null || stoneFootsteps.Length == 0 || footstepAudioSource == null) return;

        float speed = HorizontalSpeed;

        if (_characterController.isGrounded && speed > 0.25f)
        {
            float interval = _isSprinting ? sprintStepInterval : walkStepInterval;
            _stepTimer += Time.deltaTime;

            if (_stepTimer >= interval)
            {
                PlayRandomFootstep();
                _stepTimer = 0f;
            }
        }
        else
        {
            // Reset timer so next step starts slightly buffered
            _stepTimer = Mathf.MoveTowards(_stepTimer, 0f, Time.deltaTime * 4f);
        }
    }

    private void PlayRandomFootstep()
    {
        if (stoneFootsteps.Length == 0) return;

        // Choose random clip without repeating the immediately previous one
        int clipIndex = Random.Range(0, stoneFootsteps.Length);
        if (stoneFootsteps.Length > 1 && clipIndex == _lastFootstepIndex)
        {
            clipIndex = (clipIndex + 1) % stoneFootsteps.Length;
        }
        _lastFootstepIndex = clipIndex;

        AudioClip clip = stoneFootsteps[clipIndex];
        if (clip == null) return;

        // Subtle randomized pitch (+/- pitchVariation)
        footstepAudioSource.pitch = 1.0f + Random.Range(-pitchVariation, pitchVariation);

        // Minimal, subtle randomized volume
        float vol = Mathf.Clamp01(footstepVolume + Random.Range(-volumeVariation, volumeVariation));
        if (_isSprinting)
        {
            vol = Mathf.Clamp01(vol * 1.2f);
        }

        footstepAudioSource.PlayOneShot(clip, vol);
    }

    #region Input Abstraction (Works with both New Input System & Legacy)

    private Vector2 GetMovementInput()
    {
        Vector2 input = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
            return Vector2.ClampMagnitude(input, 1f);
        }
#endif
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        return Vector2.ClampMagnitude(input, 1f);
    }

    private Vector2 GetMouseInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.delta.ReadValue() * 0.1f;
        }
#endif
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    private bool IsSprintPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            return Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        }
#endif
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private bool IsJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            return Keyboard.current.spaceKey.wasPressedThisFrame;
        }
#endif
        return Input.GetKeyDown(KeyCode.Space);
    }

    private bool IsEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            return Keyboard.current.escapeKey.wasPressedThisFrame;
        }
#endif
        return Input.GetKeyDown(KeyCode.Escape);
    }

    private bool IsLeftClickPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }
#endif
        return Input.GetMouseButtonDown(0);
    }

    #endregion
}
