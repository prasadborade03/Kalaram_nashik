using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Permanent, minimalist center crosshair dot displayed throughout the entire walkthrough game.
/// Provides visual aiming reference and dynamically highlights when aiming at interactable items.
/// </summary>
[ExecuteAlways]
public class WalkthroughCrosshair : MonoBehaviour
{
    public static WalkthroughCrosshair Instance { get; private set; }

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(1.0f, 1.0f, 1.0f, 0.85f);
    [SerializeField] private Color hoverColor = new Color(1.0f, 0.45f, 0.65f, 1.0f); // Radiant Orange-Pink

    [Header("Sizes")]
    [SerializeField] private float normalSize = 6f;
    [SerializeField] private float hoverSize = 10f;

    [SerializeField] private Image crosshairImage;
    [SerializeField] private RectTransform rectTransform;
    private bool _isHovered = false;

    private void Awake()
    {
        Instance = this;
        EnsureHUDCanvas();
    }

    private void Start()
    {
        EnsureHUDCanvas();
    }

    private void EnsureHUDCanvas()
    {
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("WalkthroughHUD_Canvas");
            canvasObj.transform.SetParent(transform, false);
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Always on top

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Create Dot Image
            GameObject dotObj = new GameObject("CenterCrosshairDot");
            dotObj.transform.SetParent(canvasObj.transform, false);
            crosshairImage = dotObj.AddComponent<Image>();
            rectTransform = dotObj.GetComponent<RectTransform>();

            // Center anchors
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(normalSize, normalSize);

            // Circular dot sprite
#if UNITY_EDITOR
            var dotSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/texture/UI_CrosshairDot.png");
            if (dotSprite != null) crosshairImage.sprite = dotSprite;
#endif
            if (crosshairImage.sprite == null) crosshairImage.sprite = CreateDotSprite();
            crosshairImage.color = normalColor;
            crosshairImage.raycastTarget = false;
        }
        else
        {
            crosshairImage = canvas.GetComponentInChildren<Image>();
            if (crosshairImage != null)
            {
                rectTransform = crosshairImage.rectTransform;
                if (crosshairImage.sprite == null)
                {
#if UNITY_EDITOR
                    var dotSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/texture/UI_CrosshairDot.png");
                    if (dotSprite != null) crosshairImage.sprite = dotSprite;
#endif
                    if (crosshairImage.sprite == null) crosshairImage.sprite = CreateDotSprite();
                }
            }
        }
    }

    private void Update()
    {
        if (crosshairImage == null || rectTransform == null) return;

        Color targetColor = _isHovered ? hoverColor : normalColor;
        float targetSize = _isHovered ? hoverSize : normalSize;

        crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, Time.deltaTime * 12f);
        rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, new Vector2(targetSize, targetSize), Time.deltaTime * 12f);
    }

    public static void SetHoverState(bool hovered)
    {
        if (Instance != null)
        {
            Instance._isHovered = hovered;
        }
    }

    private Sprite CreateDotSprite()
    {
        int res = 32;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(res * 0.5f - 0.5f, res * 0.5f - 0.5f);
        float radius = res * 0.42f;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist > radius)
                {
                    // Dark soft drop shadow rim
                    float shadowAlpha = Mathf.Clamp01((radius + 2.5f - dist) / 2.5f) * 0.45f;
                    tex.SetPixel(x, y, new Color(0f, 0f, 0f, shadowAlpha));
                }
                else
                {
                    // Anti-aliased white core
                    float alpha = Mathf.Clamp01((radius - dist) / 1.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
    }
}
