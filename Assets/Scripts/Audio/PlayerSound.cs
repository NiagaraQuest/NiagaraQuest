using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSound : MonoBehaviour
{
    public static PlayerSound Instance { get; private set; }
    
    [Header("Audio Clip")]
    [SerializeField] private AudioClip movementSound;
    
    [Header("Sound Settings")]
    [SerializeField] private float defaultMovementVolume = 0.5f; // Fallback volume if AudioManager not found
    private float movementVolume;
    [SerializeField] private float minDistance = 1.0f;
    [SerializeField] private float maxDistance = 20.0f;
    [SerializeField] private float spatialBlend = 1.0f; // 1.0 = fully 3D, 0.0 = fully 2D
    
    private GameManager gameManager;
    private AudioManager audioManager;
    private Dictionary<GameObject, AudioSource> playerAudioSources = new Dictionary<GameObject, AudioSource>();
    
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

        // Initialize with default volume, will update when AudioManager is found
        movementVolume = defaultMovementVolume;
    }
    
    private void Start()
    {
        // Try to find needed managers
        FindManagers();
        
        // Wait a frame for all players to initialize
        StartCoroutine(InitializeWithDelay());
    }
    
    private void FindManagers()
    {
        // Try to find GameManager
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }
        
        // Try to find AudioManager and get volume
        if (audioManager == null)
        {
            audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                movementVolume = audioManager.sfxVolume;
                Debug.Log("PlayerSound: Found AudioManager, using sfxVolume: " + movementVolume);
            }
            else
            {
                Debug.LogWarning("PlayerSound: AudioManager not found, using default volume: " + defaultMovementVolume);
            }
        }
    }
    
    private IEnumerator InitializeWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        // Try to get managers again if needed
        FindManagers();
        
        if (gameManager != null && gameManager.players != null)
        {
            // Setup audio sources for all players
            foreach (GameObject player in gameManager.players)
            {
                if (player == null) continue;
                
                try
                {
                    // Get or add AudioSource component
                    AudioSource audioSource = player.GetComponent<AudioSource>();
                    if (audioSource == null)
                    {
                        audioSource = player.AddComponent<AudioSource>();
                    }
                    
                    // Configure the AudioSource for movement sound
                    audioSource.clip = movementSound;
                    audioSource.volume = movementVolume;
                    audioSource.loop = true;
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = spatialBlend;
                    audioSource.minDistance = minDistance;
                    audioSource.maxDistance = maxDistance;
                    audioSource.rolloffMode = AudioRolloffMode.Linear;
                    
                    // Add to dictionary
                    playerAudioSources[player] = audioSource;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"PlayerSound: Error setting up audio for {player.name}: {e.Message}");
                }
            }
            
            Debug.Log($"PlayerSound: Set up {playerAudioSources.Count} player audio sources");
        }
        else
        {
            Debug.LogWarning("PlayerSound: GameManager or players list not found during initialization");
        }
    }
    
    private void Update()
    {
        try
        {
            // Try to find managers if they are null
            if (gameManager == null || audioManager == null)
            {
                FindManagers();
                if (gameManager == null || audioManager == null) return;
            }
            
            // Get the current player
            GameObject currentPlayer = gameManager.selectedPlayer;
            if (currentPlayer == null) return;
            
            // Check if player is moving
            Player playerScript = currentPlayer.GetComponent<Player>();
            if (playerScript == null) return;
            
            // Make sure we have an audio source
            AudioSource audioSource;
            if (!playerAudioSources.TryGetValue(currentPlayer, out audioSource))
            {
                audioSource = currentPlayer.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    // Create a new audio source if needed
                    audioSource = currentPlayer.AddComponent<AudioSource>();
                    ConfigureAudioSource(audioSource);
                }
                
                // Add it to our dictionary
                playerAudioSources[currentPlayer] = audioSource;
            }
            
            // Critical fix: Check isMoving state and update sound accordingly
            if (playerScript.isMoving)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                    Debug.Log($"Started movement sound for {currentPlayer.name}");
                }
            }
            else
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                    Debug.Log($"Stopped movement sound for {currentPlayer.name}");
                }
            }

            // Update the volume based on AudioManager settings
            if (audioManager != null && audioSource.volume != audioManager.sfxVolume)
            {
                movementVolume = audioManager.sfxVolume;
                audioSource.volume = movementVolume;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerSound: Error in Update: {e.Message}");
        }
    }
    
    private void ConfigureAudioSource(AudioSource audioSource)
    {
        if (audioSource == null) return;
        
        audioSource.clip = movementSound;
        audioSource.volume = movementVolume;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = spatialBlend;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }
    
    // Public method that can be called from Player class when movement begins
    public void PlayMovementSound(GameObject player)
    {
        if (player == null) return;
        
        try
        {
            AudioSource audioSource;
            if (playerAudioSources.TryGetValue(player, out audioSource))
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                    Debug.Log($"Explicitly started movement sound for {player.name}");
                }
            }
            else
            {
                // Try to get or add an audio source if not in dictionary
                audioSource = player.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = player.AddComponent<AudioSource>();
                    ConfigureAudioSource(audioSource);
                }
                
                playerAudioSources[player] = audioSource;
                audioSource.Play();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerSound: Error playing movement sound: {e.Message}");
        }
    }
    
    // Public method that should be called from Player class when movement ends
    public void StopMovementSound(GameObject player)
    {
        if (player == null) return;
        
        try
        {
            AudioSource audioSource;
            if (playerAudioSources.TryGetValue(player, out audioSource))
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                    Debug.Log($"Explicitly stopped movement sound for {player.name}");
                }
            }
            else
            {
                // Fallback to finding the audio source directly
                audioSource = player.GetComponent<AudioSource>();
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                    Debug.Log($"Explicitly stopped movement sound for {player.name} (fallback)");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerSound: Error stopping movement sound: {e.Message}");
        }
    }
    
    // Update the movement sound when it changes
    public void SetMovementSound(AudioClip newSound)
    {
        if (newSound == null) return;
        
        try
        {
            movementSound = newSound;
            
            // Update all player audio sources with the new sound
            foreach (var entry in playerAudioSources)
            {
                if (entry.Key != null && entry.Value != null)
                {
                    entry.Value.clip = movementSound;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerSound: Error setting movement sound: {e.Message}");
        }
    }
    
    // Update the volume for all player movement sounds
    public void SetMovementVolume(float volume)
    {
        try
        {
            movementVolume = Mathf.Clamp01(volume);
            
            foreach (var entry in playerAudioSources)
            {
                if (entry.Key != null && entry.Value != null)
                {
                    entry.Value.volume = movementVolume;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerSound: Error setting movement volume: {e.Message}");
        }
    }
}