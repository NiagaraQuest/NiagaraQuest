using UnityEngine;

public class Boat : MonoBehaviour
{
    [Header("Floating Settings")]
    [Tooltip("How high the boat bobs up and down")]
    [Range(0.01f, 1f)]
    public float bobHeight = 0.2f;
    
    [Tooltip("How fast the boat bobs up and down")]
    [Range(0.1f, 3f)]
    public float bobSpeed = 0.5f;
    
    [Header("Rocking Settings")]
    [Tooltip("How much the boat tilts from side to side")]
    [Range(0.1f, 10f)]
    public float rockAngle = 3f;
    
    [Tooltip("How fast the boat rocks from side to side")]
    [Range(0.1f, 3f)]
    public float rockSpeed = 0.4f;
    
    [Header("Forward/Back Pitch")]
    [Tooltip("How much the boat pitches forward and back")]
    [Range(0.1f, 10f)]
    public float pitchAngle = 2f;
    
    [Tooltip("How fast the boat pitches forward and back")]
    [Range(0.1f, 3f)]
    public float pitchSpeed = 0.3f;
    
    [Header("Water Settings")]
    [Tooltip("Controls how calm/rough the water is")]
    [Range(0.1f, 3f)]
    public float waterRoughness = 1f;
    
    // Internal variables
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float timeOffset;
    private AudioSource audioSource;
    
    void Start()
    {
        // Store original position and rotation
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        // Add random time offset for variation
        timeOffset = Random.Range(0f, 100f);
        
        // Get the AudioSource component if present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on the boat object.");
        }
    }
    
    void Update()
    {
        // Update audio volume from AudioManager if available
        if (audioSource != null && AudioManager.Instance != null)
        {
            audioSource.volume = AudioManager.Instance.sfxVolume;
        }
        
        // Calculate time with offset
        float time = Time.time + timeOffset;
        
        // Calculate bobbing motion (up/down movement)
        float bobbingY = Mathf.Sin(time * bobSpeed) * bobHeight * waterRoughness;
        
        // Add small random horizontal movement for more natural effect
        float swayX = Mathf.Sin(time * 0.3f) * 0.05f * waterRoughness;
        float swayZ = Mathf.Sin(time * 0.2f) * 0.05f * waterRoughness;
        
        // Apply position changes
        Vector3 newPosition = startPosition + new Vector3(swayX, bobbingY, swayZ);
        transform.position = newPosition;
        
        // Calculate rocking rotation (side to side)
        float rockValue = Mathf.Sin(time * rockSpeed) * rockAngle * waterRoughness;
        
        // Calculate pitch rotation (front to back)
        float pitchValue = Mathf.Sin(time * pitchSpeed + 0.5f) * pitchAngle * waterRoughness;
        
        // Apply rotation
        Quaternion targetRotation = startRotation * Quaternion.Euler(pitchValue, 0f, rockValue);
        transform.rotation = targetRotation;
    }
}