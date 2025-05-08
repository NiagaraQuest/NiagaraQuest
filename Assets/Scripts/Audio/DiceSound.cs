using UnityEngine;

public class DiceSound : MonoBehaviour
{
    public static DiceSound Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource diceRollingSource;
    [SerializeField] private AudioSource diceLandingSource;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip diceRollingSound;
    [SerializeField] private AudioClip diceLandingSound;
    
    [Header("Sound Settings")]
    [SerializeField] private float defaultDiceVolume = 0.5f; // Fallback volume if AudioManager not found
    private float diceVolume;
    private AudioManager audioManager;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize with default volume
        diceVolume = defaultDiceVolume;
        
        InitializeAudioSources();
    }

    private void Start()
    {
        // Try to find AudioManager in Start (might not be available in Awake)
        FindAudioManager();
    }

    private void FindAudioManager()
    {
        try
        {
            audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                diceVolume = audioManager.sfxVolume;
                UpdateAudioSourceVolumes();
                Debug.Log("DiceSound: Found AudioManager, using sfxVolume: " + diceVolume);
            }
            else
            {
                Debug.LogWarning("DiceSound: AudioManager not found, using default volume: " + defaultDiceVolume);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DiceSound: Error finding AudioManager: {e.Message}");
        }
    }

    private void Update()
    {
        try
        {
            // Try to find AudioManager if it's null
            if (audioManager == null)
            {
                FindAudioManager();
            }
            
            // Update volume if it changes in AudioManager
            if (audioManager != null && diceVolume != audioManager.sfxVolume)
            {
                diceVolume = audioManager.sfxVolume;
                UpdateAudioSourceVolumes();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DiceSound: Error in Update: {e.Message}");
        }
    }

    private void UpdateAudioSourceVolumes()
    {
        try
        {
            if (diceRollingSource != null)
            {
                diceRollingSource.volume = diceVolume;
            }
            
            if (diceLandingSource != null)
            {
                diceLandingSource.volume = diceVolume;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DiceSound: Error updating audio volumes: {e.Message}");
        }
    }
    
    private void InitializeAudioSources()
    {
        try
        {
            // Setup rolling sound source
            if (diceRollingSource == null)
            {
                diceRollingSource = gameObject.AddComponent<AudioSource>();
                diceRollingSource.volume = diceVolume;
            }
            
            // Setup landing sound source (separate so sounds can overlap)
            if (diceLandingSource == null)
            {
                diceLandingSource = gameObject.AddComponent<AudioSource>();
                diceLandingSource.volume = diceVolume;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DiceSound: Error initializing audio sources: {e.Message}");
        }
    }
    
    public void PlayDiceRolling()
    {
        try
        {
            if (diceRollingSource == null)
            {
                diceRollingSource = gameObject.AddComponent<AudioSource>();
                diceRollingSource.volume = diceVolume;
            }
            
            if (diceRollingSound != null)
            {
                diceRollingSource.Stop();
                diceRollingSource.clip = diceRollingSound;
                diceRollingSource.loop = false;
                diceRollingSource.Play();
            }
            else
            {
                Debug.LogWarning("DiceSound: Dice rolling sound clip not assigned!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DiceSound: Error playing dice rolling sound: {e.Message}");
        }
    }
    
    public void StopDiceRolling()
    {
        try
        {
            if (diceRollingSource != null)
            {
                diceRollingSource.Stop();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DiceSound: Error stopping dice rolling sound: {e.Message}");
        }
    }
    
    public void PlayDiceLanding()
    {
        try
        {
            if (diceLandingSource == null)
            {
                diceLandingSource = gameObject.AddComponent<AudioSource>();
                diceLandingSource.volume = diceVolume;
            }
            
            if (diceLandingSound != null)
            {
                // Use PlayOneShot to allow multiple landing sounds to overlap
                diceLandingSource.PlayOneShot(diceLandingSound);
            }
            else
            {
                Debug.LogWarning("DiceSound: Dice landing sound clip not assigned!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DiceSound: Error playing dice landing sound: {e.Message}");
        }
    }
}