using UnityEngine;

public class Tree : MonoBehaviour
{
    [Header("Wind Settings")]
    [Tooltip("How much the tree sways")]
    [Range(0.01f, 3f)]
    public float swayAmount = 0.5f;
    
    [Tooltip("How fast the tree sways")]
    [Range(0.1f, 3f)]
    public float swaySpeed = 0.5f;
    
    [Tooltip("Controls how much the leaves/top sways more than the trunk")]
    [Range(0f, 3f)]
    public float swayGradient = 1.5f;
    
    [Header("Randomization")]
    [Tooltip("Makes each tree sway slightly differently")]
    public bool randomizeMotion = true;
    
    // Internal variables
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float timeOffset;
    private AudioSource audioSource;
    private Transform[] childTransforms;
    private Vector3[] childStartPositions;
    private Quaternion[] childStartRotations;
    
    void Start()
    {
        // Store original position and rotation
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        // Add random time offset for variation
        timeOffset = randomizeMotion ? Random.Range(0f, 100f) : 0f;
        
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on the tree object.");
        }
        
        // If tree has child objects (branches, leaves, etc.), store their transforms
        StoreChildTransforms();
    }
    
    void StoreChildTransforms()
    {
        // Skip if no children
        if (transform.childCount == 0) return;
        
        // Get all children
        childTransforms = new Transform[transform.childCount];
        childStartPositions = new Vector3[transform.childCount];
        childStartRotations = new Quaternion[transform.childCount];
        
        // Store their initial transforms
        for (int i = 0; i < transform.childCount; i++)
        {
            childTransforms[i] = transform.GetChild(i);
            childStartPositions[i] = childTransforms[i].localPosition;
            childStartRotations[i] = childTransforms[i].localRotation;
        }
    }
    
    void Update()
    {
        // Update audio volume from AudioManager
        if (audioSource != null && AudioManager.Instance != null && audioSource.volume != AudioManager.Instance.sfxVolume)
        {
            audioSource.volume = AudioManager.Instance.sfxVolume;
        }
        
        // Calculate time with offset
        float time = Time.time + timeOffset;
        
        // Apply main tree sway
        ApplyTreeSway(time);
        
        // Apply gradient sway to children if any
        if (childTransforms != null && childTransforms.Length > 0)
        {
            ApplyChildrenSway(time);
        }
    }
    
    void ApplyTreeSway(float time)
    {
        // Calculate wind direction based on time
        float windX = Mathf.Sin(time * swaySpeed * 0.5f);
        float windZ = Mathf.Sin(time * swaySpeed * 0.3f + 0.5f);
        
        // Calculate sway amount based on wind
        Vector3 swayDir = new Vector3(windX, 0, windZ).normalized * swayAmount;
        
        // Apply small position offset (for smaller trees or bushes)
        Vector3 newPosition = startPosition;
        if (swayAmount < 0.5f) // Only move position for small trees/bushes
        {
            newPosition += new Vector3(
                swayDir.x * 0.05f,
                0,
                swayDir.z * 0.05f
            );
        }
        transform.position = newPosition;
        
        // Apply rotation sway (main tree trunk movement)
        float swayAngleX = windZ * swayAmount * 2f; // Forward/backward tilt
        float swayAngleZ = -windX * swayAmount * 2f; // Left/right tilt
        
        Quaternion targetRotation = startRotation * Quaternion.Euler(swayAngleX, 0, swayAngleZ);
        transform.rotation = targetRotation;
    }
    
    void ApplyChildrenSway(float time)
    {
        // For each child, apply progressively stronger sway
        for (int i = 0; i < childTransforms.Length; i++)
        {
            // Calculate relative height - assumes tree grows in Y direction
            float relativeHeight = 0;
            if (transform.localScale.y > 0)
            {
                relativeHeight = childStartPositions[i].y / transform.localScale.y;
            }
            relativeHeight = Mathf.Clamp01(relativeHeight); // 0 at bottom, 1 at top
            
            // More sway for higher parts of the tree
            float childSwayFactor = Mathf.Lerp(1, swayGradient, relativeHeight);
            
            // Unique variation for each branch
            float branchOffset = i * 0.1f;
            
            // Calculate wind direction with variation
            float windX = Mathf.Sin(time * swaySpeed * 0.5f + branchOffset);
            float windZ = Mathf.Sin(time * swaySpeed * 0.3f + 0.5f + branchOffset);
            
            // Apply rotation sway with height gradient
            float swayAngleX = windZ * swayAmount * childSwayFactor;
            float swayAngleZ = -windX * swayAmount * childSwayFactor;
            
            // Additional higher frequency, lower amplitude motion for leaves
            float leafShake = Mathf.Sin(time * swaySpeed * 2f + i) * swayAmount * 0.2f * relativeHeight;
            
            Quaternion targetRotation = childStartRotations[i] * Quaternion.Euler(swayAngleX, leafShake, swayAngleZ);
            childTransforms[i].localRotation = targetRotation;
        }
    }
}