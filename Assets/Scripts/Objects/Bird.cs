using UnityEngine;

public class Bird : MonoBehaviour
{
    [Header("Flight Settings")]
    [Tooltip("How much the bird moves while flying/perched")]
    [Range(0.01f, 1f)]
    public float movementAmount = 0.1f;
    
    [Tooltip("How fast the bird moves")]
    [Range(0.1f, 5f)]
    public float movementSpeed = 2f;
    
    [Header("Wing Animation")]
    [Tooltip("Enable wing flapping motion")]
    public bool animateWings = true;
    
    [Tooltip("How fast the wings flap")]
    [Range(0.1f, 10f)]
    public float flapSpeed = 5f;
    
    [Tooltip("How much the wings move")]
    [Range(0.1f, 45f)]
    public float flapAmount = 20f;
    
    [Header("Head Movement")]
    [Tooltip("Enable head bobbing and turning")]
    public bool animateHead = true;
    
    [Tooltip("How much the head moves")]
    [Range(0.1f, 30f)]
    public float headMovementAmount = 15f;
    
    // References to bird parts (optional)
    [Header("Bird Parts (Optional)")]
    public Transform leftWing;
    public Transform rightWing;
    public Transform head;
    
    // Internal variables
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float timeOffset;
    private AudioSource audioSource;
    
    // Wing and head initial rotations
    private Quaternion leftWingStartRotation;
    private Quaternion rightWingStartRotation;
    private Quaternion headStartRotation;
    
    void Start()
    {
        // Store original position and rotation
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        // Add random time offset for variation
        timeOffset = Random.Range(0f, 100f);
        
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on the bird object.");
        }
        
        // Store initial rotations of wings and head if assigned
        if (leftWing != null) leftWingStartRotation = leftWing.localRotation;
        if (rightWing != null) rightWingStartRotation = rightWing.localRotation;
        if (head != null) headStartRotation = head.localRotation;
    }
    
    void Update()
    {
        // Update audio volume from AudioManager
        if (audioSource != null && AudioManager.Instance != null && audioSource.volume != AudioManager.Instance.sfxVolume)
        {
            audioSource.volume = AudioManager.Instance.sfxVolume;
        }
        
        // Calculate time with offset for variation
        float time = Time.time + timeOffset;
        
        // Apply bird body movement
        ApplyBirdBodyMovement(time);
        
        // Animate wings if enabled and references exist
        if (animateWings)
        {
            AnimateWings(time);
        }
        
        // Animate head if enabled and reference exists
        if (animateHead && head != null)
        {
            AnimateHead(time);
        }
    }
    
    void ApplyBirdBodyMovement(float time)
    {
        // Calculate subtle body movement
        float xMovement = Mathf.Sin(time * movementSpeed * 0.7f) * movementAmount * 0.5f;
        float yMovement = Mathf.Sin(time * movementSpeed) * movementAmount;
        float zMovement = Mathf.Sin(time * movementSpeed * 0.5f) * movementAmount * 0.3f;
        
        // Apply position changes for gentle bobbing
        Vector3 newPosition = startPosition + new Vector3(xMovement, yMovement, zMovement);
        transform.position = newPosition;
        
        // Apply subtle rotation changes
        float pitchAngle = Mathf.Sin(time * movementSpeed * 0.6f) * 2f;
        float rollAngle = Mathf.Sin(time * movementSpeed * 0.4f) * 3f;
        float yawAngle = Mathf.Sin(time * movementSpeed * 0.3f) * 2f;
        
        Quaternion targetRotation = startRotation * Quaternion.Euler(pitchAngle, yawAngle, rollAngle);
        transform.rotation = targetRotation;
    }
    
    void AnimateWings(float time)
    {
        if (leftWing == null && rightWing == null) return;
        
        // Calculate wing flapping motion
        float flapAngle = Mathf.Sin(time * flapSpeed) * flapAmount;
        
        // Apply to left wing if available
        if (leftWing != null)
        {
            Quaternion leftFlapRotation = leftWingStartRotation * Quaternion.Euler(0, 0, flapAngle);
            leftWing.localRotation = leftFlapRotation;
        }
        
        // Apply to right wing if available (inverted angle for right wing)
        if (rightWing != null)
        {
            Quaternion rightFlapRotation = rightWingStartRotation * Quaternion.Euler(0, 0, -flapAngle);
            rightWing.localRotation = rightFlapRotation;
        }
    }
    
    void AnimateHead(float time)
    {
        // Calculate head bobbing and looking around
        float headBob = Mathf.Sin(time * movementSpeed * 1.5f) * headMovementAmount * 0.3f;
        float headTurn = Mathf.Sin(time * movementSpeed * 0.4f) * headMovementAmount;
        float headTilt = Mathf.Sin(time * movementSpeed * 0.3f + 0.5f) * headMovementAmount * 0.5f;
        
        // Apply head movement
        Quaternion headRotation = headStartRotation * Quaternion.Euler(headBob, headTurn, headTilt);
        head.localRotation = headRotation;
    }
}