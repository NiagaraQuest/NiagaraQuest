using UnityEngine;

public class Turbine : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Speed of the turbine rotation in degrees per second")]
    public float rotationSpeed = 30f;
    
    [Tooltip("Axis around which the turbine rotates")]
    public Vector3 rotationAxis = Vector3.up;
    
    [Header("Speed Variation")]
    [Tooltip("Enable natural speed variations")]
    public bool useSpeedVariation = true;
    
    [Tooltip("How much the speed can vary from the base speed")]
    [Range(0f, 0.5f)]
    public float speedVariationAmount = 0.1f;
    
    [Tooltip("How quickly the speed varies")]
    [Range(0.01f, 1f)]
    public float speedVariationRate = 0.2f;
    
    [Header("Startup")]
    [Tooltip("Should the turbine start at full speed or gradually accelerate")]
    public bool gradualStartup = true;
    
    [Tooltip("How long it takes to reach full speed in seconds")]
    [Range(0.1f, 10f)]
    public float startupTime = 3f;
    
    // Internal variables
    private float currentSpeed;
    private float startTime;
    private AudioSource audioSource;
    private float timeOffset;
    
    void Start()
    {
        // Initialize rotation speed
        currentSpeed = gradualStartup ? 0f : rotationSpeed;
        
        // Record start time for gradual startup
        startTime = Time.time;
        
        // Add random time offset for variation
        timeOffset = Random.Range(0f, 100f);
        
        // Normalize rotation axis
        if (rotationAxis.magnitude > 0)
        {
            rotationAxis.Normalize();
        }
        else
        {
            rotationAxis = Vector3.up; // Default to Y-axis
        }
        
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on the turbine object.");
        }
    }
    
    void Update()
    {
        // Update audio volume from AudioManager
        if (audioSource != null && AudioManager.Instance != null)
        {
            audioSource.volume = AudioManager.Instance.sfxVolume;
        }
        
        // Handle gradual startup
        if (gradualStartup && Time.time - startTime < startupTime)
        {
            float startupProgress = (Time.time - startTime) / startupTime;
            currentSpeed = Mathf.Lerp(0f, rotationSpeed, startupProgress);
        }
        else if (!useSpeedVariation)
        {
            currentSpeed = rotationSpeed;
        }
        
        // Apply speed variation if enabled
        if (useSpeedVariation && Time.time - startTime >= startupTime)
        {
            float time = Time.time + timeOffset;
            float variation = Mathf.Sin(time * speedVariationRate) * speedVariationAmount + 1f;
            currentSpeed = rotationSpeed * variation;
        }
        
        // Apply rotation
        transform.Rotate(rotationAxis, currentSpeed * Time.deltaTime);
        
        // Adjust audio pitch based on speed if audio source exists
        if (audioSource != null)
        {
            float speedRatio = currentSpeed / rotationSpeed;
            audioSource.pitch = Mathf.Lerp(0.8f, 1.2f, speedRatio);
        }
    }
}