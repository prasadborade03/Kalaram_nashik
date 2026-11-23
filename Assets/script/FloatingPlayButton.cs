using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimalist VR-style floating play button in glowing orange and light pink aesthetic.
/// Billboards to face the player, displays clear interaction instructions,
/// and triggers video playback on cursor click or 'E' key without any unwanted chime.
/// </summary>
public class FloatingPlayButton : MonoBehaviour
{
    [Header("Target Screen")]
    [SerializeField] private VerticalTVScreen tvScreen;

    [Header("Visual Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform badgeTransform;
    [SerializeField] private Image buttonBackground;
    [SerializeField] private Text instructionText;
    [SerializeField] private Light floatingGlowLight;

    [Header("Color Palette (Orange & Light Pink)")]
    [SerializeField] private Color normalColor = new Color(1.0f, 0.44f, 0.22f, 0.95f);    // Aesthetic Saffron Orange
    [SerializeField] private Color hoverColor = new Color(1.0f, 0.58f, 0.72f, 1.0f);     // Radiant Lotus Pink
    [SerializeField] private Color glowLightColor = new Color(1.0f, 0.65f, 0.55f, 1.0f); // Warm peach/pink light

    [Header("Floating Animation")]
    [SerializeField] private float floatFrequency = 2.0f;
    [SerializeField] private float floatAmplitude = 0.035f;
    [SerializeField] private float breathSpeed = 1.8f;
    [SerializeField] private float breathAmount = 0.03f;
    [SerializeField] private float distanceFromScreen = 0.75f;

    // State
    private bool _isPlayerInRange = false;
    private bool _justEnteredRange = false;
    private bool _isHovered = false;
    private bool _isClicked = false;
    private Vector3 _baseWorldPos;
    private Camera _cachedCamera;
    private float _currentAlpha = 0f;
    private float _targetAlpha = 0f;

    public bool IsHovered => _isHovered;
    public bool IsActive => _isPlayerInRange && !_isClicked && (tvScreen == null || tvScreen.CanPlay);

    private void Awake()
    {
        _baseWorldPos = transform.position;

        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        }

        SetVisualAlpha(0f);
    }

    private void Start()
    {
        _cachedCamera = Camera.main;
        if (tvScreen != null)
        {
            _baseWorldPos = tvScreen.transform.position + new Vector3(0f, 0f, -distanceFromScreen);
            tvScreen.OnVideoStarted += HandleVideoStarted;
        }
    }

    private void HandleVideoStarted()
    {
        _isClicked = true;
        SetVisualAlpha(0f);
    }

    private void OnDestroy()
    {
        if (tvScreen != null)
        {
            tvScreen.OnVideoStarted -= HandleVideoStarted;
        }
    }

    private void LateUpdate()
    {
        UpdatePositionAndBillboard();
        UpdateAnimations();
        UpdateVisibility();
    }

    /// <summary>
    /// Smoothly positions the floating button between the player and screen,
    /// and billboards to face the player's camera like VR floating UI.
    /// </summary>
    private void UpdatePositionAndBillboard()
    {
        if (_cachedCamera == null)
        {
            _cachedCamera = Camera.main;
            if (_cachedCamera == null) return;
        }

        // Dynamically position directly in front of the player towards the TV
        if (tvScreen != null && _isPlayerInRange)
        {
            Vector3 tvCenter = tvScreen.transform.position;
            Vector3 camPos = _cachedCamera.transform.position;
            Vector3 toTV = (tvCenter - camPos);
            float distToTV = toTV.magnitude;
            Vector3 horizontalDir = new Vector3(toTV.x, 0f, toTV.z).normalized;

            if (horizontalDir.sqrMagnitude > 0.01f)
            {
                // Float comfortably ~1.2m in front of the user (or halfway if very close)
                float floatDist = Mathf.Clamp(distToTV * 0.45f, 0.65f, 1.3f);
                Vector3 desiredPos = camPos + horizontalDir * floatDist;
                desiredPos.y = camPos.y - 0.06f; // Exactly at eye level
                if (_justEnteredRange)
                {
                    _baseWorldPos = desiredPos;
                    _justEnteredRange = false;
                }
                else
                {
                    _baseWorldPos = Vector3.Lerp(_baseWorldPos, desiredPos, Time.deltaTime * 8f);
                }
            }
        }

        // Floating hover motion
        float hoverY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = _baseWorldPos + new Vector3(0f, hoverY, 0f);

        // Billboard rotation: Face camera directly and perpendicularly
        transform.rotation = Quaternion.Slerp(transform.rotation, _cachedCamera.transform.rotation, Time.deltaTime * 14f);
    }

    private void UpdateAnimations()
    {
        if (_currentAlpha <= 0.01f) return;

        // Subtle breathing scale on badge
        if (badgeTransform != null)
        {
            float breath = 1.0f + Mathf.Sin(Time.time * breathSpeed) * breathAmount;
            float baseScale = _isHovered ? 1.15f : 1.0f;
            badgeTransform.localScale = Vector3.one * (baseScale * breath);
        }

        // Color transition on hover
        if (buttonBackground != null)
        {
            Color targetCol = _isHovered ? hoverColor : normalColor;
            buttonBackground.color = Color.Lerp(buttonBackground.color, targetCol, Time.deltaTime * 10f);
        }

        if (floatingGlowLight != null)
        {
            floatingGlowLight.color = glowLightColor;
            float targetLightIntensity = _isHovered ? 1.4f : 0.7f;
            floatingGlowLight.intensity = Mathf.Lerp(floatingGlowLight.intensity, targetLightIntensity, Time.deltaTime * 8f);
        }
    }

    private void UpdateVisibility()
    {
        bool shouldShow = IsActive;
        _targetAlpha = shouldShow ? 1f : 0f;
        _currentAlpha = Mathf.MoveTowards(_currentAlpha, _targetAlpha, Time.deltaTime * (shouldShow ? 4.0f : 5.0f));
        SetVisualAlpha(_currentAlpha);
    }

    public void SetPlayerInRange(bool inRange)
    {
        if (inRange && !_isPlayerInRange)
        {
            _justEnteredRange = true;
        }
        _isPlayerInRange = inRange;
        if (!inRange)
        {
            _isHovered = false;
        }
    }

    public void SetHovered(bool hovered)
    {
        _isHovered = hovered;
    }

    public void TriggerClick()
    {
        if (_isClicked || !IsActive) return;

        StartCoroutine(ClickRoutine());
    }

    private IEnumerator ClickRoutine()
    {
        _isClicked = true;

        // Punch bounce animation
        if (badgeTransform != null)
        {
            float punchDuration = 0.18f;
            float elapsed = 0f;
            Vector3 orig = badgeTransform.localScale;
            while (elapsed < punchDuration)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / punchDuration;
                badgeTransform.localScale = orig * (1f + Mathf.Sin(p * Mathf.PI) * 0.25f);
                yield return null;
            }
        }

        // Trigger TV video start (NO BELL SOUND)
        if (tvScreen != null)
        {
            tvScreen.PlayVideo();
        }

        // Smooth fade out
        float fadeDuration = 0.3f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            float t = fadeElapsed / fadeDuration;
            SetVisualAlpha(1f - t);
            yield return null;
        }

        SetVisualAlpha(0f);
    }

    private void SetVisualAlpha(float a)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = a;
            canvasGroup.interactable = a > 0.5f;
            canvasGroup.blocksRaycasts = a > 0.5f;
        }

        if (floatingGlowLight != null)
        {
            floatingGlowLight.enabled = a > 0.05f;
        }
    }
}
