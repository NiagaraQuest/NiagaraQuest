using UnityEngine;

public class Water : MonoBehaviour
{
    // Internal variables
    private AudioSource audioSource;
    
    void Start()
    {
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on the water object.");
        }
    }
    
    void Update()
    {
        // Update audio volume from AudioManager
        if (audioSource != null && AudioManager.Instance != null && audioSource.volume != AudioManager.Instance.sfxVolume)
        {
            audioSource.volume = AudioManager.Instance.sfxVolume;
        }
    }
}