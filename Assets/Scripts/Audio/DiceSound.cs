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
    
 
    private float diceVolume = AudioManager.Instance.sfxVolume;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        InitializeAudioSources();
    }
    
    private void InitializeAudioSources()
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
    
    public void PlayDiceRolling()
    {
        if (diceRollingSound != null)
        {
            diceRollingSource.Stop();
            diceRollingSource.clip = diceRollingSound;
            diceRollingSource.loop = false;
            diceRollingSource.Play();
        }
        else
        {
            Debug.LogWarning("Dice rolling sound clip not assigned!");
        }
    }
    
    public void StopDiceRolling()
    {
        diceRollingSource.Stop();
    }
    
    public void PlayDiceLanding()
    {
        if (diceLandingSound != null)
        {
            // Use PlayOneShot to allow multiple landing sounds to overlap
            diceLandingSource.PlayOneShot(diceLandingSound);
        }
        else
        {
            Debug.LogWarning("Dice landing sound clip not assigned!");
        }
    }
    
}