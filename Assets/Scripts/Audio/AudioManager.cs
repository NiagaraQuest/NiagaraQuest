using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips - Assign in Inspector")]
    [SerializeField] private AudioClip menuBackgroundSound;
    [SerializeField] private AudioClip rightAnswerSound;
    [SerializeField] private AudioClip wrongAnswerSound;
    [SerializeField] private AudioClip cardTileSound;
    [SerializeField] private AudioClip winGameSound;
    [SerializeField] private AudioClip loseGameSound;

    [SerializeField] private AudioClip gameplayBackgroundSound;
    [SerializeField] private AudioClip gameplayIntenseBackgroundSound; // New intense music for low lives/time running out
    [SerializeField] private AudioClip menuButtonSound;
    [SerializeField] private AudioClip bonusCardSound;
    [SerializeField] private AudioClip intersectionTileSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] public float musicVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] public float sfxVolume = 0.7f;

    [Header("Gameplay Music Settings")]
    [SerializeField] private float musicChangeTimeThreshold = 300f; // 5 minutes in seconds
    [SerializeField] private int lowLifeThreshold = 1; // When a player has this many lives or fewer, change music

    private float gameplayStartTime;
    private bool isIntenseMusicPlaying = false;
    private GameManager gameManager;
    private bool isGameScene = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            EnsureSingleAudioListener();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Register for scene change events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void PlayMusicForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // Check which scene is active
        if (currentSceneName == "MenuScene")
        {
            isGameScene = false;
            TransitionToMenuMusic();
        }
        else // Assume it's the game scene
        {
            isGameScene = true;
            gameplayStartTime = Time.time;
            TransitionToGameplayMusic();
            
            // Try to find GameManager - it might not be initialized yet, so we'll also check in Update
            if (GameManager.Instance != null)
            {
                gameManager = GameManager.Instance;
            }
        }
    }
    
    private void Start()
    {
        PlayMusicForCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }
    
    private void Update()
    {
        // Only check for music change conditions in game scenes
        if (!isGameScene) return;
        
        // If we don't have GameManager yet, try to find it
        if (gameManager == null && GameManager.Instance != null)
        {
            gameManager = GameManager.Instance;
        }
        
        // Skip if GameManager is still not available
        if (gameManager == null) return;
        
        if (!isIntenseMusicPlaying)
        {
            bool shouldPlayIntense = false;
            
            // Check time condition
            float timeElapsed = Time.time - gameplayStartTime;
            if (timeElapsed >= musicChangeTimeThreshold)
            {
                Debug.Log($"Time threshold reached: {timeElapsed:F1} seconds. Switching to intense music.");
                shouldPlayIntense = true;
            }
            
            // Check player life condition
            if (!shouldPlayIntense && gameManager.players != null)
            {
                foreach (GameObject playerObj in gameManager.players)
                {
                    if (playerObj != null)
                    {
                        Player player = playerObj.GetComponent<Player>();
                        if (player != null && player.lives <= lowLifeThreshold)
                        {
                            Debug.Log($"Player {player.gameObject.name} has low life ({player.lives}). Switching to intense music.");
                            shouldPlayIntense = true;
                            break;
                        }
                    }
                }
            }
            
            // Change music if needed
            if (shouldPlayIntense)
            {
                TransitionToIntenseGameplayMusic();
                isIntenseMusicPlaying = true;
            }
        }
    }

    private float transitionDuration = 1.0f; // Adjust this for faster/slower transition
    
    public void TransitionToMenuMusic()
    {
        StartCoroutine(CrossFadeMusic(menuBackgroundSound));
    }
    
    public void TransitionToGameplayMusic()
    {
        StartCoroutine(CrossFadeMusic(gameplayBackgroundSound));
        isIntenseMusicPlaying = false;
    }

    public void TransitionToIntenseGameplayMusic()
    {
        if (gameplayIntenseBackgroundSound != null)
        {
            StartCoroutine(CrossFadeMusic(gameplayIntenseBackgroundSound));
            isIntenseMusicPlaying = true;
        }
        else
        {
            Debug.LogWarning("No intense gameplay music assigned!");
        }
    }
    
    // Coroutine for smooth music transition
    private System.Collections.IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        // Skip if the clip is null
        if (newClip == null)
        {
            Debug.LogWarning("Tried to transition to a null audio clip");
            yield break;
        }
        
        // Store the original volume
        float originalVolume = musicSource.volume;
        
        // If a different song is already playing, fade it out
        if (musicSource.isPlaying && musicSource.clip != newClip)
        {
            // Fade out current music
            float fadeTime = 0f;
            while (fadeTime < transitionDuration)
            {
                fadeTime += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(originalVolume, 0f, fadeTime / transitionDuration);
                yield return null;
            }
            
            // Change to new clip
            musicSource.Stop();
            musicSource.clip = newClip;
            musicSource.volume = 0f;
            musicSource.Play();
            
            // Fade in new music
            fadeTime = 0f;
            while (fadeTime < transitionDuration)
            {
                fadeTime += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, originalVolume, fadeTime / transitionDuration);
                yield return null;
            }
        }
        else // No music is playing or same clip
        {
            // Just start the music
            musicSource.clip = newClip;
            musicSource.volume = originalVolume;
            musicSource.Play();
        }
        
        // Ensure volume is set back to original
        musicSource.volume = originalVolume;
    }
    
    // Utility method to manually switch music with transition
    public void SwitchBackgroundMusic(bool toMenu)
    {
        if (toMenu)
        {
            TransitionToMenuMusic();
        }
        else
        {
            TransitionToGameplayMusic();
        }
    }
    
    // Manually force switch to intense gameplay music
    public void SwitchToIntenseMusic()
    {
        TransitionToIntenseGameplayMusic();
    }
    
    // Reset the game timer - call this if game restarts
    public void ResetGameTimer()
    {
        gameplayStartTime = Time.time;
        isIntenseMusicPlaying = false;
        if (isGameScene)
        {
            TransitionToGameplayMusic(); // Switch back to normal gameplay music
        }
    }
    
    // Add these getter methods for UI
    public float GetMusicVolume()
    {
        return musicVolume;
    }
    
    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    private void EnsureSingleAudioListener()
    {
        // Find all AudioListeners in the scene using the newer recommended method
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        
        // If there's more than one listener
        if (listeners.Length > 1)
        {
            Debug.LogWarning($"Found {listeners.Length} AudioListeners in the scene. Removing extras...");
            
            // Keep the first one (usually the main camera) and disable the rest
            for (int i = 1; i < listeners.Length; i++)
            {
                listeners[i].enabled = false;
                Debug.Log($"Disabled AudioListener on {listeners[i].gameObject.name}");
            }
        }
    }

    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.volume = sfxVolume;
        }

        // Remove any AudioListener that might have been added to this GameObject
        AudioListener listener = GetComponent<AudioListener>();
        if (listener != null)
        {
            Destroy(listener);
            Debug.Log("Removed AudioListener from AudioManager GameObject");
        }
    }

    // Music Controls
    public void PlayGameplayBackground()
    {
        if (gameplayBackgroundSound != null)
        {
            musicSource.clip = gameplayBackgroundSound;
            musicSource.Play();
            isIntenseMusicPlaying = false;
        }
    }

    public void PlayIntenseGameplayBackground()
    {
        if (gameplayIntenseBackgroundSound != null)
        {
            musicSource.clip = gameplayIntenseBackgroundSound;
            musicSource.Play();
            isIntenseMusicPlaying = true;
        }
    }

    public void PlayMenuBackground()
    {
        if (menuBackgroundSound != null)
        {
            musicSource.clip = menuBackgroundSound;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    // SFX Controls
    public void PlayRightAnswer()
    {
        PlaySFX(rightAnswerSound);
    }

    public void PlayWrongAnswer()
    {
        PlaySFX(wrongAnswerSound);
    }

    public void PlayCardTile()
    {
        PlaySFX(cardTileSound);
    }


    public void PlayWinGame()
    {
        PlaySFX(winGameSound);
    }

    public void PlayLoseGame()
    {
        PlaySFX(loseGameSound); 
    }
    
    public void PlayMenuButton()
    {
        PlaySFX(menuButtonSound);
    }

    public void PlayBonusCard()
    {
        PlaySFX(bonusCardSound);
    }

    public void PlayIntersectionTile()
    {
        PlaySFX(intersectionTileSound);
    }
    
    private void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Attempted to play a null audio clip");
        }
    }

    // Utility Methods
    public void MuteAll()
    {
        musicSource.mute = true;
        sfxSource.mute = true;
    }

    public void UnmuteAll()
    {
        musicSource.mute = false;
        sfxSource.mute = false;
    }

    public void PauseAll()
    {
        musicSource.Pause();
        sfxSource.Pause();
    }

    public void ResumeAll()
    {
        musicSource.UnPause();
        sfxSource.UnPause();
    }
}