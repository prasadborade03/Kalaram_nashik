using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the immersive, peaceful ambient soundscape of the Kalaram Temple:
/// 1. Indian classical bansuri (flute) continuous subtle loop
/// 2. Gentle outdoor courtyard birds chirping
/// 3. Minimal, subtle mantra chanting with serene spacing
/// 4. Reverent temple bell / chime when performing Hindu Namaste greeting
/// </summary>
public class TempleAmbience : MonoBehaviour
{
    public static TempleAmbience Instance { get; private set; }

    [Header("Indian Flute (Bansuri)")]
    [Tooltip("Looping bansuri flute audio clip")]
    [SerializeField] private AudioClip fluteClip;
    [Range(0f, 1f)]
    [SerializeField] private float fluteVolume = 0.18f;

    [Header("Courtyard Birds & Peace")]
    [Tooltip("Looping gentle birds chirping audio clip")]
    [SerializeField] private AudioClip birdsClip;
    [Range(0f, 1f)]
    [SerializeField] private float birdsVolume = 0.14f;

    [Header("Temple Mantra Chanting")]
    [Tooltip("Sacred mantra chanting audio clip")]
    [SerializeField] private AudioClip mantraClip;
    [Range(0f, 1f)]
    [SerializeField] private float mantraVolume = 0.12f;
    [Tooltip("Delay in seconds between mantra chants (peaceful spacing)")]
    [SerializeField] private float mantraInterval = 12f;

    [Header("Namaste / Greeting Chime")]
    [Tooltip("Soft resonant temple bell chime played on Namaste greeting")]
    [SerializeField] private AudioClip greetingChimeClip;
    [Range(0f, 1f)]
    [SerializeField] private float greetingVolume = 0.22f;

    private AudioSource _fluteSource;
    private AudioSource _birdsSource;
    private AudioSource _mantraSource;
    private AudioSource _chimeSource;
    private Coroutine _mantraRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SetupAudioSources();
    }

    private void Start()
    {
        StartAmbience();
    }

    private void SetupAudioSources()
    {
        _fluteSource = gameObject.AddComponent<AudioSource>();
        _fluteSource.playOnAwake = false;
        _fluteSource.loop = true;
        _fluteSource.volume = fluteVolume;
        _fluteSource.spatialBlend = 0f; // 2D ambient
        _fluteSource.clip = fluteClip;

        _birdsSource = gameObject.AddComponent<AudioSource>();
        _birdsSource.playOnAwake = false;
        _birdsSource.loop = true;
        _birdsSource.volume = birdsVolume;
        _birdsSource.spatialBlend = 0f; // 2D ambient
        _birdsSource.clip = birdsClip;

        _mantraSource = gameObject.AddComponent<AudioSource>();
        _mantraSource.playOnAwake = false;
        _mantraSource.loop = false;
        _mantraSource.volume = mantraVolume;
        _mantraSource.spatialBlend = 0f;
        _mantraSource.clip = mantraClip;

        _chimeSource = gameObject.AddComponent<AudioSource>();
        _chimeSource.playOnAwake = false;
        _chimeSource.loop = false;
        _chimeSource.volume = greetingVolume;
        _chimeSource.spatialBlend = 0f;
        _chimeSource.clip = greetingChimeClip;
    }

    public void StartAmbience()
    {
        if (_fluteSource != null && fluteClip != null)
        {
            _fluteSource.Play();
        }

        if (_birdsSource != null && birdsClip != null)
        {
            _birdsSource.Play();
        }

        if (_mantraRoutine != null)
        {
            StopCoroutine(_mantraRoutine);
        }
        _mantraRoutine = StartCoroutine(MantraChantLoop());
    }

    private IEnumerator MantraChantLoop()
    {
        // Initial gentle delay before first chant starts
        yield return new WaitForSeconds(3f);

        while (true)
        {
            if (_mantraSource != null && mantraClip != null)
            {
                _mantraSource.Play();
                yield return new WaitForSeconds(mantraClip.length);
            }
            else
            {
                yield return new WaitForSeconds(10f);
            }

            // Serene pause between chants
            yield return new WaitForSeconds(mantraInterval);
        }
    }

    /// <summary>
    /// Plays the sacred temple bell chime when Namaste gesture is performed.
    /// </summary>
    public void PlayGreetingChime()
    {
        if (_chimeSource != null && greetingChimeClip != null)
        {
            _chimeSource.pitch = Random.Range(0.98f, 1.02f);
            _chimeSource.PlayOneShot(greetingChimeClip, greetingVolume);
        }
    }

    private void OnValidate()
    {
        if (_fluteSource != null) _fluteSource.volume = fluteVolume;
        if (_birdsSource != null) _birdsSource.volume = birdsVolume;
        if (_mantraSource != null) _mantraSource.volume = mantraVolume;
        if (_chimeSource != null) _chimeSource.volume = greetingVolume;
    }
}
