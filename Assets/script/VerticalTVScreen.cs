using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controls the 9:16 vertical TV screen, video playback, glowing emission,
/// and smooth transitions between thumbnail and video stream.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VerticalTVScreen : MonoBehaviour
{
    [Header("Video File Configuration")]
    [Tooltip("Name of the video in StreamingAssets or relative path")]
    [SerializeField] private string videoStreamingFileName = "RamNaam108.mp4";

    [Tooltip("Absolute fallback video path if not found in StreamingAssets")]
    [SerializeField] private string absoluteFallbackPath = @"C:\Users\PRASAD BORADE\Downloads\Ram Naam 108 Times in 1 Minute _ राम नाम जप _ Ram Naam Jap with Hanuman Ji _ The Creator Room_1080p.mp4";

    [Header("Render Resources")]
    [SerializeField] private RenderTexture videoRenderTexture;
    [SerializeField] private Texture2D thumbnailTexture;
    [SerializeField] private Renderer[] screenMeshRenderers;
    [SerializeField] private Material screenMaterial;

    [Header("Screen Glow & Lighting")]
    [SerializeField] private Light screenGlowLight;
    [SerializeField] private Color glowColor = new Color(1.0f, 0.72f, 0.42f, 1.0f); // Warm sacred golden amber
    [SerializeField] private float idleEmissionMultiplier = 0.5f;
    [SerializeField] private float playingEmissionMultiplier = 1.3f;
    [SerializeField] private float idleLightIntensity = 0.7f;
    [SerializeField] private float playingLightIntensity = 2.0f;

    [Header("Audio")]
    [SerializeField] private AudioSource tvAudioSource;

    [Header("Playback Behavior")]
    [Tooltip("If true, the video can only be played once per session")]
    [SerializeField] private bool playOnlyOnce = true;

    // Runtime state
    private VideoPlayer _videoPlayer;
    private bool _hasPlayed = false;
    private bool _isPlaying = false;
    private MaterialPropertyBlock _propBlock;
    private Coroutine _glowCoroutine;

    // Shader property IDs
    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int EmissionMapId = Shader.PropertyToID("_EmissionMap");

    public bool HasPlayed => _hasPlayed;
    public bool IsPlaying => _isPlaying;
    public bool CanPlay => !playOnlyOnce || !_hasPlayed;

    public System.Action OnVideoStarted;
    public System.Action OnVideoCompleted;

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        _propBlock = new MaterialPropertyBlock();

        SetupVideoPlayer();
        SetupAudio();
        ShowThumbnail();
    }

    private void SetupVideoPlayer()
    {
        _videoPlayer.playOnAwake = false;
        _videoPlayer.isLooping = false;
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        // Resolve video path: StreamingAssets first, then absolute fallback
        string streamingPath = Path.Combine(Application.streamingAssetsPath, videoStreamingFileName);
        if (File.Exists(streamingPath))
        {
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = streamingPath;
        }
        else if (File.Exists(absoluteFallbackPath))
        {
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = absoluteFallbackPath;
        }
        else
        {
            Debug.LogWarning($"[VerticalTVScreen] Video file not found at '{streamingPath}' or '{absoluteFallbackPath}'");
        }

        if (videoRenderTexture != null)
        {
            _videoPlayer.targetTexture = videoRenderTexture;
        }

        _videoPlayer.loopPointReached += OnVideoEndReached;
        _videoPlayer.errorReceived += (source, message) =>
        {
            Debug.LogError($"[VerticalTVScreen] Video error: {message}");
            ShowThumbnail();
            _isPlaying = false;
        };
    }

    private void SetupAudio()
    {
        if (tvAudioSource == null)
        {
            tvAudioSource = GetComponent<AudioSource>();
            if (tvAudioSource == null)
            {
                tvAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        tvAudioSource.playOnAwake = false;
        tvAudioSource.spatialBlend = 0.85f; // Spatial 3D sound with wide presence
        tvAudioSource.minDistance = 2.0f;
        tvAudioSource.maxDistance = 22.0f;
        tvAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;

        _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        _videoPlayer.EnableAudioTrack(0, true);
        _videoPlayer.SetTargetAudioSource(0, tvAudioSource);
    }

    private void Start()
    {
        ShowThumbnail();
    }

    /// <summary>
    /// Displays the static thumbnail with warm glowing emission.
    /// </summary>
    public void ShowThumbnail()
    {
        if (thumbnailTexture == null) return;

        ApplyTextureToScreens(thumbnailTexture, idleEmissionMultiplier);

        if (screenGlowLight != null)
        {
            screenGlowLight.color = glowColor;
            screenGlowLight.intensity = idleLightIntensity;
        }
    }

    /// <summary>
    /// Starts video playback if permitted.
    /// </summary>
    public void PlayVideo()
    {
        if (_videoPlayer == null) _videoPlayer = GetComponent<VideoPlayer>();
        if (!CanPlay || _isPlaying) return;

        StartCoroutine(StartPlaybackRoutine());
    }

    private IEnumerator StartPlaybackRoutine()
    {
        if (_videoPlayer == null) _videoPlayer = GetComponent<VideoPlayer>();
        _isPlaying = true;

        if (videoRenderTexture != null)
        {
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = videoRenderTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = prev;
        }

        _videoPlayer.Prepare();
        while (!_videoPlayer.isPrepared)
        {
            yield return null;
        }

        if (videoRenderTexture != null)
        {
            ApplyTextureToScreens(videoRenderTexture, playingEmissionMultiplier);
        }

        _videoPlayer.Play();
        OnVideoStarted?.Invoke();

        if (_glowCoroutine != null) StopCoroutine(_glowCoroutine);
        _glowCoroutine = StartCoroutine(AnimateGlow(idleLightIntensity, playingLightIntensity, playingEmissionMultiplier, 0.8f));
    }

    private void OnVideoEndReached(VideoPlayer vp)
    {
        _isPlaying = false;
        _hasPlayed = true;

        ShowThumbnail();

        if (_glowCoroutine != null) StopCoroutine(_glowCoroutine);
        _glowCoroutine = StartCoroutine(AnimateGlow(playingLightIntensity, idleLightIntensity, idleEmissionMultiplier, 1.2f));

        OnVideoCompleted?.Invoke();
    }

    private void ApplyTextureToScreens(Texture tex, float emissionMult)
    {
        if (screenMeshRenderers == null || screenMeshRenderers.Length == 0) return;
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        Color emission = glowColor * emissionMult;

        foreach (var rend in screenMeshRenderers)
        {
            if (rend == null) continue;

            if (rend.material != null)
            {
                rend.material.SetTexture(BaseMapId, tex);
                rend.material.SetTexture(MainTexId, tex);
                rend.material.SetTexture(EmissionMapId, tex);
                rend.material.SetColor(EmissionColorId, emission);
                rend.material.EnableKeyword("_EMISSION");
            }

            rend.GetPropertyBlock(_propBlock);
            _propBlock.SetTexture(BaseMapId, tex);
            _propBlock.SetTexture(MainTexId, tex);
            _propBlock.SetTexture(EmissionMapId, tex);
            _propBlock.SetColor(EmissionColorId, emission);
            rend.SetPropertyBlock(_propBlock);
        }
    }

    private IEnumerator AnimateGlow(float fromIntensity, float toIntensity, float targetEmissionMult, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            if (screenGlowLight != null)
            {
                screenGlowLight.intensity = Mathf.Lerp(fromIntensity, toIntensity, t);
            }
            yield return null;
        }
        if (screenGlowLight != null)
        {
            screenGlowLight.intensity = toIntensity;
        }
    }

    private void OnDestroy()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.loopPointReached -= OnVideoEndReached;
        }
    }
}
