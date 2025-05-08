using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSound : MonoBehaviour
{
    public static PlayerSound Instance { get; private set; }
    
    [Header("Audio Clip")]
    [SerializeField] private AudioClip movementSound;
    
    [Header("Sound Settings")]
    private float movementVolume = AudioManager.Instance.sfxVolume;
    [SerializeField] private float minDistance = 1.0f;
    [SerializeField] private float maxDistance = 20.0f;
    [SerializeField] private float spatialBlend = 1.0f; // 1.0 = fully 3D, 0.0 = fully 2D
    
    private GameManager gameManager;
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
    }
    
    private void Start()
    {
        // Find the GameManager
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("PlayerSoundManager: GameManager.Instance not found!");
        }
        
        // Wait a frame for all players to initialize
        StartCoroutine(InitializeWithDelay());
    }
    
    
    private IEnumerator InitializeWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        // Try to get GameManager again if needed
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }
        
        if (gameManager != null && gameManager.players != null)
        {
            // Setup audio sources for all players
            foreach (GameObject player in gameManager.players)
            {
                if (player == null) continue;
                
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
            
            Debug.Log($"PlayerSoundManager: Set up {playerAudioSources.Count} player audio sources");
        }
    }
    
    private void Update()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
            if (gameManager == null) return;
        }
        
        // Get the current player
        GameObject currentPlayer = gameManager.selectedPlayer;
        if (currentPlayer == null) return;
        
        // Check if player is moving
        Player playerScript = currentPlayer.GetComponent<Player>();
        if (playerScript == null) return;
        
        // Only log in debug mode to avoid spam
        // Debug.Log($"Player {currentPlayer.name} isMoving: {playerScript.isMoving}");
        
        // Make sure we have an audio source
        AudioSource audioSource;
        audioSource = currentPlayer.GetComponent<AudioSource>();
        if (audioSource != null)
        {
           
            if (audioSource == null) return;
            
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
        movementVolume = AudioManager.Instance.sfxVolume;
        if (audioSource.volume != movementVolume)
        {
            audioSource.volume = movementVolume;
            Debug.Log($"Updated volume for {currentPlayer.name} to {movementVolume}");
        }
    }
    
    // Public method that can be called from Player class when movement begins
    public void PlayMovementSound(GameObject player)
    {
        if (player == null) return;
        
        AudioSource audioSource;
        if (playerAudioSources.TryGetValue(player, out audioSource))
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log($"Explicitly started movement sound for {player.name}");
            }
        }
    }
    
    // Public method that should be called from Player class when movement ends
    public void StopMovementSound(GameObject player)
    {
        if (player == null) return;
        
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
    
    // Update the movement sound when it changes
    public void SetMovementSound(AudioClip newSound)
    {
        movementSound = newSound;
        
        // Update all player audio sources with the new sound
        foreach (var entry in playerAudioSources)
        {
            entry.Value.clip = movementSound;
        }
    }
    
    // Update the volume for all player movement sounds
    public void SetMovementVolume(float volume)
    {
        movementVolume = Mathf.Clamp01(volume);
        
        foreach (var entry in playerAudioSources)
        {
            entry.Value.volume = movementVolume;
        }
    }
}