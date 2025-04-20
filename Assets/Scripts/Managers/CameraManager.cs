using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    public Camera mainCamera;
    private Camera activePlayerCamera;
    private Player activePlayer;
    
    [Header("Camera Settings")]
    public float transitionDuration = 0.5f; // Time to blend between cameras
    
    [Header("Cursor Camera Controls")]
    public bool enableCursorControl = true;
    public float cursorSensitivity = 2.0f;
    public float smoothTime = 0.1f;
    public float rotationLimit = 60f; // 60 degree limit in all directions
    public int dragMouseButton = 0; // 0 = left, 1 = right, 2 = middle
    
    private Quaternion initialRotation; // Store the initial camera rotation as Quaternion
    private Vector3 currentEulerAngles;
    private Vector3 rotationVelocity = Vector3.zero;
    private bool isDragging = false;
    private Vector2 lastMousePosition;
    private bool isMainCameraActive = true;
    private Coroutine cameraTransitionCoroutine;

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
    }

    void Start()
    {
        // Ensure main camera is active at start
        if (mainCamera != null)
        {
            mainCamera.enabled = true;
            initialRotation = mainCamera.transform.rotation;
            currentEulerAngles = mainCamera.transform.eulerAngles;
        }
        
        // Disable all player cameras at start
        DisableAllPlayerCameras();
        
        // Initially the main camera is active
        isMainCameraActive = true;
    }
    
    void Update()
    {
        // Only process cursor movement when main camera is active and cursor control is enabled
        if (isMainCameraActive && enableCursorControl && mainCamera != null)
        {
            HandleCursorCameraMovement();
        }
    }
    
    private void HandleCursorCameraMovement()
    {
        // Check for mouse button press to start dragging
        if (Input.GetMouseButtonDown(dragMouseButton))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }
        
        // Check for mouse button release to stop dragging
        if (Input.GetMouseButtonUp(dragMouseButton))
        {
            isDragging = false;
        }
        
        // Only rotate camera when dragging
        if (isDragging)
        {
            // Calculate mouse delta from last position
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            
            // Apply sensitivity
            float mouseX = mouseDelta.x * cursorSensitivity * 0.01f;
            float mouseY = mouseDelta.y * cursorSensitivity * 0.01f;
            
            // Update Euler angles based on mouse movement
            currentEulerAngles.y += mouseX;
            currentEulerAngles.x -= mouseY; // Inverted for natural feeling
            
            // Update last mouse position
            lastMousePosition = currentMousePosition;
        }
        
        // Create a rotation from the current Euler angles
        Quaternion targetRotation = Quaternion.Euler(currentEulerAngles);
        
        // Check if we're exceeding the angle limit
        float angle = Quaternion.Angle(initialRotation, targetRotation);
        if (angle > rotationLimit)
        {
            // If we're exceeding the limit, interpolate back to the allowed range
            targetRotation = Quaternion.Slerp(
                targetRotation,
                initialRotation,
                (angle - rotationLimit) / angle
            );
            
            // Update the current Euler angles to match the clamped rotation
            currentEulerAngles = targetRotation.eulerAngles;
        }
        
        // Apply smooth rotation
        mainCamera.transform.rotation = Quaternion.Slerp(
            mainCamera.transform.rotation,
            targetRotation,
            1 - Mathf.Exp(-Time.deltaTime / smoothTime) // More stable than SmoothDamp for rotations
        );
    }
    
    public void SwitchToPlayerCamera(Player player)
    {
        if (player == null) return;
        
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            Debug.LogWarning($"No camera found on player {player.gameObject.name}");
            return;
        }
        
        // Cancel any ongoing transition
        if (cameraTransitionCoroutine != null)
        {
            StopCoroutine(cameraTransitionCoroutine);
        }
        
        Debug.Log($"Switching to {player.gameObject.name}'s camera");
        
        // Transition to player camera
        cameraTransitionCoroutine = StartCoroutine(TransitionToCamera(playerCamera));
        
        // Store active player and camera
        activePlayer = player;
        activePlayerCamera = playerCamera;
        isMainCameraActive = false;
    }
    
    public void SwitchToMainCamera()
    {
        if (mainCamera == null) return;
        
        // Cancel any ongoing transition
        if (cameraTransitionCoroutine != null)
        {
            StopCoroutine(cameraTransitionCoroutine);
        }
        
        Debug.Log("Switching to main camera");
        
        // Reset rotation to initial when switching back to main camera
        currentEulerAngles = initialRotation.eulerAngles;
        
        // Transition to main camera
        cameraTransitionCoroutine = StartCoroutine(TransitionToCamera(mainCamera));
        
        activePlayer = null;
        activePlayerCamera = null;
        isMainCameraActive = true;
    }
    
    private IEnumerator TransitionToCamera(Camera targetCamera)
    {
        // Enable the target camera
        targetCamera.enabled = true;
        
        // Disable all other cameras (except the target)
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam != targetCamera)
            {
                cam.enabled = false;
            }
        }
        
        yield return null; // Wait one frame for camera to activate
        
        // Transition completed
        cameraTransitionCoroutine = null;
    }
    
    private void DisableAllPlayerCameras()
    {
        // Find all player cameras and disable them
        Player[] players = FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            Camera playerCamera = player.GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                playerCamera.enabled = false;
            }
        }
    }
    
    // Toggle cursor control on/off
    public void ToggleCursorControl(bool enable)
    {
        enableCursorControl = enable;
    }
}