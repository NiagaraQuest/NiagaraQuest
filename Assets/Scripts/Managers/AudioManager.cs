using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips - Assign in Inspector")]
    [SerializeField] private AudioClip rightAnswerSound;
    [SerializeField] private AudioClip wrongAnswerSound;
    [SerializeField] private AudioClip cardTileSound;
    [SerializeField] private AudioClip movementSound;
    [SerializeField] private AudioClip diceRollingSound;
    [SerializeField] private AudioClip winGameSound;
    [SerializeField] private AudioClip gameplayBackgroundSound;
    [SerializeField] private AudioClip menuButtonSound;
    [SerializeField] private AudioClip bonusCardSound;
    [SerializeField] private AudioClip intersectionTileSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.7f;

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

    private void Start()
    {
        // Auto-play background music when game starts
        PlayGameplayBackground();
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

    public void PlayMovement()
    {
        PlaySFX(movementSound);
    }

    public void PlayDiceRolling()
    {
        PlaySFX(diceRollingSound);
    }

    public void PlayWinGame()
    {
        PlaySFX(winGameSound);
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