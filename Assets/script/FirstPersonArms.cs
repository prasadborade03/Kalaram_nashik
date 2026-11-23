using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controls first-person hands/arms presentation, procedural sway, bobbing,
/// and feeds movement speed, sprinting, and action triggers into the arms Animator.
/// </summary>
public class FirstPersonArms : MonoBehaviour
{
    [Header("Animation")]
    [Tooltip("Animator on the arms hierarchy")]
    [SerializeField] private Animator animator;
    [Tooltip("Parameter name in Animator Controller for movement speed")]
    [SerializeField] private string speedParam = "Speed";
    [Tooltip("Parameter name in Animator Controller for sprinting state")]
    [SerializeField] private string sprintingParam = "IsSprinting";
    [Tooltip("Trigger name in Animator Controller for inspect hands action")]
    [SerializeField] private string inspectTrigger = "Inspect";

    [Header("Procedural Sway Settings")]
    [Tooltip("How much the arms lag/sway when rotating the camera")]
    [SerializeField] private float swayAmount = 1.2f;
    [Tooltip("Maximum rotation angle for sway")]
    [SerializeField] private float maxSwayAngle = 4.5f;
    [Tooltip("Speed at which arms return to rest position")]
    [SerializeField] private float swaySmoothing = 8f;

    [Header("Positional Sway Settings")]
    [Tooltip("How much the arms physically shift position when looking around")]
    [SerializeField] private float positionalSwayAmount = 0.012f;
    [Tooltip("Max positional displacement from looking")]
    [SerializeField] private float maxPositionalSway = 0.035f;

    [Header("Default Rest Transform")]
    [Tooltip("Local offset relative to camera")]
    [SerializeField] private Vector3 restPosition = new Vector3(0f, -1.50f, 0.06f);
    [Tooltip("Local rotation relative to camera")]
    [SerializeField] private Vector3 restRotation = Vector3.zero;

    private Quaternion _targetSwayRotation;
    private Vector3 _targetSwayPosition;
    private FirstPersonController _playerController;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        _playerController = GetComponentInParent<FirstPersonController>();
        transform.localPosition = restPosition;
        transform.localRotation = Quaternion.Euler(restRotation);
    }

    private void Update()
    {
        HandleMouseSway();
        UpdateAnimationState();
        HandleActionInputs();
    }

    private void HandleMouseSway()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, restPosition, Time.deltaTime * swaySmoothing);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(restRotation), Time.deltaTime * swaySmoothing);
            return;
        }

        Vector2 mouseDelta = GetMouseDelta();

        // Calculate rotational sway (pitch, yaw, roll)
        float swayX = -mouseDelta.x * swayAmount;
        float swayY = -mouseDelta.y * swayAmount;
        swayX = Mathf.Clamp(swayX, -maxSwayAngle, maxSwayAngle);
        swayY = Mathf.Clamp(swayY, -maxSwayAngle, maxSwayAngle);

        Quaternion swayRot = Quaternion.Euler(new Vector3(swayY, swayX, swayX * 0.5f));
        _targetSwayRotation = Quaternion.Euler(restRotation) * swayRot;

        // Calculate positional sway
        float posX = -mouseDelta.x * positionalSwayAmount;
        float posY = -mouseDelta.y * positionalSwayAmount;
        posX = Mathf.Clamp(posX, -maxPositionalSway, maxPositionalSway);
        posY = Mathf.Clamp(posY, -maxPositionalSway, maxPositionalSway);

        _targetSwayPosition = restPosition + new Vector3(posX, posY, 0f);

        // Smoothly interpolate towards target
        transform.localPosition = Vector3.Lerp(transform.localPosition, _targetSwayPosition, Time.deltaTime * swaySmoothing);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, _targetSwayRotation, Time.deltaTime * swaySmoothing);
    }

    private void UpdateAnimationState()
    {
        if (animator == null) return;

        float currentSpeed = 0f;
        bool isSprinting = false;

        if (_playerController != null)
        {
            currentSpeed = _playerController.HorizontalSpeed;
            isSprinting = _playerController.IsSprintingActive;
        }

        animator.SetFloat(speedParam, currentSpeed);
        animator.SetBool(sprintingParam, isSprinting);
    }

    private void HandleActionInputs()
    {
        if (animator == null || Cursor.lockState != CursorLockMode.Locked) return;

        // F or I key: Inspect hands
        if (IsInspectPressed())
        {
            animator.SetTrigger(inspectTrigger);
        }
    }

    private bool IsInspectPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            return Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.iKey.wasPressedThisFrame;
        }
#endif
        return Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.I);
    }

    private Vector2 GetMouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.delta.ReadValue() * 0.08f;
        }
#endif
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    public void SetRestPosition(Vector3 pos, Vector3 rot)
    {
        restPosition = pos;
        restRotation = rot;
    }
}
