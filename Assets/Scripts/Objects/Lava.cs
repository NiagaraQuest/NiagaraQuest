using UnityEngine;

public class Lava : MonoBehaviour
{
    [Header("Lava Movement")]
    [Tooltip("How fast the lava moves")]
    [Range(0.01f, 2f)]
    public float movementSpeed = 0.2f;
    
    [Tooltip("How much the lava moves")]
    [Range(0.001f, 0.1f)]
    public float movementAmount = 0.01f;
    
    // Material animation
    [Tooltip("Enable material movement effects")]
    public bool animateMaterial = true;
    
    [Tooltip("Speed of texture flow")]
    [Range(0.01f, 1f)]
    public float textureFlowSpeed = 0.05f;
    
    // Internal variables
    private Material lavaMaterial;
    private Vector2 textureOffset = Vector2.zero;
    private Vector3 startPosition;
    private AudioSource audioSource;
    
    void Start()
    {
        // Store original position
        startPosition = transform.position;
        
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on the lava object.");
        }
        
        // Get material if we're animating it
        if (animateMaterial)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                // Create a material instance to avoid modifying the shared material
                lavaMaterial = renderer.material;
            }
        }
    }
    
    void Update()
    {
        // Update audio volume from AudioManager
        if (audioSource != null && AudioManager.Instance != null && audioSource.volume != AudioManager.Instance.sfxVolume)
        {
            audioSource.volume = AudioManager.Instance.sfxVolume;
        }
        
        // Simple position movement to simulate lava bubbling
        float time = Time.time;
        
        // Calculate offset for subtle random movement
        float xOffset = Mathf.PerlinNoise(time * movementSpeed, 0) * 2 - 1;
        float zOffset = Mathf.PerlinNoise(0, time * movementSpeed) * 2 - 1;
        float yOffset = Mathf.PerlinNoise(time * movementSpeed, time * movementSpeed) * 2 - 1;
        
        // Apply the offset to create a subtle bobbing motion
        Vector3 newPosition = startPosition + new Vector3(
            xOffset * movementAmount,
            yOffset * movementAmount,
            zOffset * movementAmount
        );
        
        transform.position = newPosition;
        
        // Animate material if enabled
        if (animateMaterial && lavaMaterial != null)
        {
            // Update texture offset for flowing effect
            textureOffset.x += textureFlowSpeed * Time.deltaTime;
            textureOffset.y += textureFlowSpeed * 0.7f * Time.deltaTime;
            
            // Keep the offset values from getting too large
            if (textureOffset.x > 1f) textureOffset.x -= 1f;
            if (textureOffset.y > 1f) textureOffset.y -= 1f;
            
            // Apply to main texture
            lavaMaterial.mainTextureOffset = textureOffset;
        }
    }
}