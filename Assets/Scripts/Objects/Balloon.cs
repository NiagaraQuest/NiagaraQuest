using UnityEngine;

public class Balloon : MonoBehaviour
{
    [Header("Floating Movement")]
    [Tooltip("How high the balloon bobs up and down")]
    [Range(0.01f, 1f)]
    public float bobHeight = 0.2f;
    
    [Tooltip("How fast the balloon bobs up and down")]
    [Range(0.1f, 5f)]
    public float bobSpeed = 1f;
    
    [Tooltip("How much the balloon drifts horizontally")]
    [Range(0.01f, 1f)]
    public float driftAmount = 0.1f;
    
    [Tooltip("How fast the balloon drifts horizontally")]
    [Range(0.1f, 5f)]
    public float driftSpeed = 0.7f;
    
    [Header("Rotation")]
    [Tooltip("How much the balloon rotates")]
    [Range(0.1f, 10f)]
    public float rotationAmount = 2f;
    
    [Tooltip("How fast the balloon rotates")]
    [Range(0.1f, 5f)]
    public float rotationSpeed = 0.5f;
    
    [Header("Wind Effect")]
    [Tooltip("Direction of the wind")]
    public Vector3 windDirection = new Vector3(1f, 0f, 1f);
    
    [Tooltip("Strength of the wind effect")]
    [Range(0f, 1f)]
    public float windStrength = 0.2f;
    
    [Tooltip("How fast the wind strength changes")]
    [Range(0.01f, 1f)]
    public float windVariation = 0.1f;
    
    [Header("Random Motion")]
    [Tooltip("Makes each balloon move uniquely")]
    public bool useRandomSeed = true;
    
    [Tooltip("Seed for random movement (ignored if useRandomSeed is true)")]
    public int randomSeed = 0;
    
    // Private variables
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float timeOffset;
    private float bobPhase;
    private float driftPhaseX;
    private float driftPhaseZ;
    private float rotPhaseX;
    private float rotPhaseY;
    private float rotPhaseZ;
    private float currentWindStrength;

    void Start()
    {
        // Store original position and rotation
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        // Initialize with random offsets if enabled
        if (useRandomSeed)
        {
            randomSeed = Random.Range(0, 10000);
        }
        
        // Use the seed to generate consistent but varied random values
        Random.InitState(randomSeed);
        
        // Create random phase offsets for more natural movement
        timeOffset = Random.Range(0f, 100f);
        bobPhase = Random.Range(0f, Mathf.PI * 2);
        driftPhaseX = Random.Range(0f, Mathf.PI * 2);
        driftPhaseZ = Random.Range(0f, Mathf.PI * 2);
        rotPhaseX = Random.Range(0f, Mathf.PI * 2);
        rotPhaseY = Random.Range(0f, Mathf.PI * 2);
        rotPhaseZ = Random.Range(0f, Mathf.PI * 2);
        
        // Normalize wind direction
        if (windDirection.magnitude > 0)
        {
            windDirection.Normalize();
        }
        else
        {
            windDirection = new Vector3(1f, 0f, 1f).normalized;
        }
    }

    void Update()
    {
        float time = Time.time + timeOffset;
        
        // Calculate bobbing motion (vertical movement)
        float yPos = startPosition.y + Mathf.Sin(time * bobSpeed + bobPhase) * bobHeight;
        
        // Calculate drifting motion (horizontal movement)
        float xPos = startPosition.x + Mathf.Sin(time * driftSpeed * 0.7f + driftPhaseX) * driftAmount;
        float zPos = startPosition.z + Mathf.Sin(time * driftSpeed * 0.5f + driftPhaseZ) * driftAmount;
        
        // Calculate wind effect
        currentWindStrength = Mathf.Clamp01(windStrength + Mathf.Sin(time * windVariation) * 0.1f);
        Vector3 windEffect = windDirection * Mathf.Sin(time * 0.3f) * currentWindStrength;
        
        // Combine all movement effects
        Vector3 newPosition = new Vector3(xPos, yPos, zPos) + windEffect;
        transform.position = newPosition;
        
        // Calculate rotation changes
        float rotX = Mathf.Sin(time * rotationSpeed * 0.7f + rotPhaseX) * rotationAmount;
        float rotY = Mathf.Sin(time * rotationSpeed * 0.5f + rotPhaseY) * rotationAmount;
        float rotZ = Mathf.Sin(time * rotationSpeed * 0.6f + rotPhaseZ) * rotationAmount;
        
        // Apply rotation
        Quaternion newRotation = startRotation * Quaternion.Euler(rotX, rotY, rotZ);
        transform.rotation = newRotation;
    }
    
    // Visualize the movement range in the editor
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, driftAmount + bobHeight);
            
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            Gizmos.DrawRay(transform.position, windDirection * windStrength);
        }
    }
    
    // Reset to original position and rotation
    public void ResetPosition()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
    }
}