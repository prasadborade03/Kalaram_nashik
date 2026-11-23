using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple and natural Hindu temple bell interaction.
/// When the player stands in the designated area in front of the bell,
/// it plays the authentic bell chime and performs a gentle, natural swing animation.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class TempleBell : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Temple bell audio clip")]
    public AudioClip bellSound;
    [Range(0f, 1f)]
    public float volume = 1.0f;

    [Header("Interaction Area")]
    [Tooltip("Target player Transform (auto-detects 'Player' tag if null)")]
    public Transform player;
    [Tooltip("World position of the interaction area in front of the bell")]
    public Vector3 areaCenter = new Vector3(0.034f, 6.2f, -9.5f);
    [Tooltip("Half-extents of the interaction area box")]
    public Vector3 areaSize = new Vector3(2.5f, 2.5f, 2.5f);
    [Tooltip("Cooldown between rings in seconds")]
    public float cooldown = 3.5f;

    [Header("Simple Natural Animation")]
    [Tooltip("Small natural swing angle in degrees")]
    public float swingAngle = 5.5f;
    [Tooltip("Natural oscillation speed")]
    public float swingSpeed = 2.5f;
    [Tooltip("Damping decay rate")]
    public float decayRate = 1.2f;

    private AudioSource audioSource;
    private Quaternion restRotation;
    private bool isRinging = false;
    private float ringStartTime = -10f;
    private float lastRingTime = -10f;
    private bool playerWasInArea = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.75f; // Rich 3D presence
        audioSource.minDistance = 2.0f;
        audioSource.maxDistance = 40.0f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

        restRotation = transform.localRotation;

        if (bellSound != null)
        {
            audioSource.clip = bellSound;
        }
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        // 1. Check if player is inside the designated area in front of the bell
        if (player != null)
        {
            Vector3 playerPos = player.position;
            bool inArea = Mathf.Abs(playerPos.x - areaCenter.x) <= areaSize.x * 0.5f &&
                         Mathf.Abs(playerPos.y - areaCenter.y) <= areaSize.y * 0.5f &&
                         Mathf.Abs(playerPos.z - areaCenter.z) <= areaSize.z * 0.5f;

            // Auto-ring when player enters the area, or with 'E' key while inside
            if (inArea)
            {
                WalkthroughCrosshair.SetHoverState(true);
                bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
                if (!playerWasInArea || ePressed)
                {
                    Ring();
                }
            }
            else if (playerWasInArea)
            {
                WalkthroughCrosshair.SetHoverState(false);
            }

            playerWasInArea = inArea;
        }

        // 2. Simple, gentle natural swinging animation
        if (isRinging)
        {
            float t = Time.time - ringStartTime;
            float duration = 3.5f;

            if (t < duration)
            {
                // Damped sine wave: small natural pendulum sway
                float decay = Mathf.Exp(-decayRate * t);
                float angle = swingAngle * decay * Mathf.Sin(2f * Mathf.PI * swingSpeed * t);

                // Gentle forward-backward sway with tiny transverse wobble
                float wobble = angle * 0.2f * Mathf.Sin(3f * Mathf.PI * swingSpeed * t);

                Quaternion swing = Quaternion.Euler(angle, 0f, wobble);
                transform.localRotation = restRotation * swing;
            }
            else
            {
                transform.localRotation = restRotation;
                isRinging = false;
            }
        }
    }

    /// <summary>
    /// Rings the temple bell: plays sound and triggers the small animation.
    /// </summary>
    public void Ring()
    {
        if (Time.time - lastRingTime < cooldown) return;

        lastRingTime = Time.time;
        ringStartTime = Time.time;
        isRinging = true;

        if (audioSource != null && bellSound != null)
        {
            audioSource.pitch = Random.Range(0.99f, 1.01f);
            audioSource.PlayOneShot(bellSound, volume);
        }
    }

    /// <summary>
    /// Support for trigger collider attached to interaction zone
    /// </summary>
    public void OnTriggerZoneEnter()
    {
        Ring();
    }

    void OnDrawGizmos()
    {
        // Draw the interaction area in front of the bell
        Gizmos.color = new Color(1f, 0.65f, 0.1f, 0.35f);
        Gizmos.DrawCube(areaCenter, areaSize);
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.8f);
        Gizmos.DrawWireCube(areaCenter, areaSize);
    }
}
