using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Proximity detection and cursor interaction zone for the Vertical TV.
/// Clean, reliable raycast and input handling supporting both Unity's New Input System
/// and Legacy Input without throwing any exceptions or polluting IMGUI.
/// </summary>
public class TVInteractionZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FloatingPlayButton playButton;
    [SerializeField] private Collider playButtonCollider;
    [SerializeField] private VerticalTVScreen tvScreen;

    [Header("Detection Settings")]
    [SerializeField] private float maxInteractDistance = 6.5f;
    [SerializeField] private LayerMask interactLayerMask = ~0;

    private bool _isPlayerInside = false;
    private Camera _mainCamera;
    private float _initTime = 0f;

    private void Start()
    {
        _mainCamera = Camera.main;
        _initTime = Time.time;
        if (tvScreen == null)
        {
            tvScreen = GetComponentInParent<VerticalTVScreen>();
        }
    }

    private void Update()
    {
        // Grace period on spawn to ignore window-focus clicks
        if (Time.time - _initTime < 0.6f) return;

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) return;
        }

        // Deterministic distance check between player camera and TV screen
        float dist = Vector3.Distance(_mainCamera.transform.position, transform.position);
        bool inRange = dist <= maxInteractDistance;

        if (_isPlayerInside != inRange)
        {
            _isPlayerInside = inRange;
            if (playButton != null)
            {
                playButton.SetPlayerInRange(inRange);
            }
            if (!inRange)
            {
                WalkthroughCrosshair.SetHoverState(false);
            }
        }

        if (!_isPlayerInside || playButton == null || !playButton.IsActive)
        {
            WalkthroughCrosshair.SetHoverState(false);
            return;
        }

        bool isHovering = CheckRaycastHover();
        playButton.SetHovered(isHovering);
        WalkthroughCrosshair.SetHoverState(isHovering);

        if ((isHovering && IsLeftClickPressed()) || IsEPressed())
        {
            playButton.TriggerClick();
        }
    }

    private bool CheckRaycastHover()
    {
        if (_mainCamera == null) return false;

        Ray ray;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }
        else
        {
            Vector3 mousePos = GetMousePosition();
            ray = _mainCamera.ScreenPointToRay(mousePos);
        }

        RaycastHit[] hits = Physics.RaycastAll(ray, maxInteractDistance, interactLayerMask, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0) return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            // Hit the floating play button collider or any of its child visuals
            if (hit.collider == playButtonCollider || (playButton != null && hit.transform.IsChildOf(playButton.transform)))
            {
                return true;
            }

            // Hit the TV screen or housing
            if (tvScreen != null && (hit.transform == tvScreen.transform || hit.transform.IsChildOf(tvScreen.transform)))
            {
                return true;
            }

            // Ignore other trigger zones
            if (hit.collider.isTrigger)
            {
                continue;
            }

            // Solid obstacle closer than the TV/button
            return false;
        }

        return false;
    }

    private bool IsLeftClickPressed()
    {
        // Ignore clicks when player is clicking into Game View to lock/focus cursor
        if (Cursor.lockState != CursorLockMode.Locked) return false;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
        return false;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private bool IsEPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) return true;
        return false;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    private Vector3 GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            return new Vector3(pos.x, pos.y, 0f);
        }
        return new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
#else
        return Input.mousePosition;
#endif
    }
}
