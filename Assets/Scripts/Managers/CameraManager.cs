using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    public Camera mainCamera;
    public Camera diceCamera; // Camera for viewing dice rolls
    public Camera viewCamera; // Additional camera for alternative view
    
    [Header("Region Cameras")]
    public Camera vulkanCamera; // Camera for Vulkan region
    public Camera atlantaCamera; // Camera for Atlanta region
    public Camera celestyelCamera; // Camera for Celestyel region
    public Camera bergCamera; // Camera for Berg region
    
    private Camera activeCamera;
    private Tile.Region currentRegion = Tile.Region.None;
    private Player activePlayer;
    
    [Header("Camera Settings")]
    public float transitionDuration = 0.5f; // Time to blend between cameras
    
    [Header("Cursor Camera Controls")]
    public bool enableCursorControl = true;
    public float cursorSensitivity = 2.0f;
    public float smoothTime = 0.1f;
    public float rotationLimit = 60f; // 60 degree limit in all directions
    public int dragMouseButton = 0; // 0 = left, 1 = right, 2 = middle
    
    [Header("UI References")]
    public UnityEngine.UI.Button viewToggleButton; // Button to toggle view camera
    
    private Quaternion initialRotation; // Store the initial camera rotation as Quaternion
    private Quaternion initialViewRotation; // Store the view camera's initial rotation
    private Vector3 currentEulerAngles;
    private Vector3 viewEulerAngles;
    private Vector3 rotationVelocity = Vector3.zero;
    private bool isDragging = false;
    private Vector2 lastMousePosition;
    private bool isMainCameraActive = true;
    private bool isViewCameraActive = false;
    private Coroutine cameraTransitionCoroutine;
    private DiceManager diceManager;

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
        
        // Set up dice camera integration
        diceManager = FindObjectOfType<DiceManager>();
        if (diceManager != null && diceCamera != null)
        {
            // Subscribe to both dice roll start and completion events
            diceManager.OnDiceRollStart += OnDiceRollStart;
            diceManager.OnDiceRollComplete += OnDiceRollComplete;
        }
        else
        {
            if (diceCamera == null)
                Debug.LogWarning("Dice camera is not assigned to CameraManager!");
            if (diceManager == null)
                Debug.LogWarning("DiceManager not found in the scene!");
        }
        
        // Set up view toggle button
        if (viewToggleButton != null)
        {
            viewToggleButton.onClick.AddListener(ToggleViewCamera);
            UpdateViewButtonState();
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (diceManager != null)
        {
            diceManager.OnDiceRollStart -= OnDiceRollStart;
            diceManager.OnDiceRollComplete -= OnDiceRollComplete;
        }
        
        // Clean up view button listener
        if (viewToggleButton != null)
        {
            viewToggleButton.onClick.RemoveListener(ToggleViewCamera);
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
            activeCamera = mainCamera;
        }
        
        // Store initial view camera rotation
        if (viewCamera != null)
        {
            initialViewRotation = viewCamera.transform.rotation;
            viewEulerAngles = viewCamera.transform.eulerAngles;
            viewCamera.enabled = false;
        }
        
        // Disable all region cameras at start
        DisableAllRegionCameras();
        
        // Disable dice camera at start
        if (diceCamera != null)
        {
            diceCamera.enabled = false;
        }
        
        // Initially the main camera is active
        isMainCameraActive = true;
        isViewCameraActive = false;
        
        // Update button state
        UpdateViewButtonState();
    }
    
    void Update()
    {
        // Process cursor movement for main camera
        if (isMainCameraActive && enableCursorControl && mainCamera != null && activeCamera == mainCamera)
        {
            HandleCursorCameraMovement(mainCamera, ref currentEulerAngles, initialRotation);
        }
        
        // Process cursor movement for view camera
        if (isViewCameraActive && enableCursorControl && viewCamera != null && activeCamera == viewCamera)
        {
            HandleCursorCameraMovement(viewCamera, ref viewEulerAngles, initialViewRotation);
        }
    }
    
    private void HandleCursorCameraMovement(Camera camera, ref Vector3 eulerAngles, Quaternion initialRot)
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
            eulerAngles.y += mouseX;
            eulerAngles.x -= mouseY; // Inverted for natural feeling
            
            // Update last mouse position
            lastMousePosition = currentMousePosition;
        }
        
        // Create a rotation from the current Euler angles
        Quaternion targetRotation = Quaternion.Euler(eulerAngles);
        
        // Check if we're exceeding the angle limit
        float angle = Quaternion.Angle(initialRot, targetRotation);
        if (angle > rotationLimit)
        {
            // If we're exceeding the limit, interpolate back to the allowed range
            targetRotation = Quaternion.Slerp(
                targetRotation,
                initialRot,
                (angle - rotationLimit) / angle
            );
            
            // Update the current Euler angles to match the clamped rotation
            eulerAngles = targetRotation.eulerAngles;
        }
        
        // Apply smooth rotation
        camera.transform.rotation = Quaternion.Slerp(
            camera.transform.rotation,
            targetRotation,
            1 - Mathf.Exp(-Time.deltaTime / smoothTime) // More stable than SmoothDamp for rotations
        );
    }
    
    // Method to switch camera based on region
    public void SwitchToRegionCamera(Tile.Region region, Player player)
    {
        if (region == currentRegion && activeCamera != mainCamera && activeCamera != viewCamera)
        {
            // Already using the correct region camera
            return;
        }
        
        Camera targetCamera = GetCameraForRegion(region);
        
        if (targetCamera == null)
        {
            Debug.LogWarning($"No camera assigned for region {region}, using main camera");
            targetCamera = mainCamera;
        }
        
        // Cancel any ongoing transition
        if (cameraTransitionCoroutine != null)
        {
            StopCoroutine(cameraTransitionCoroutine);
        }
        
        Debug.Log($"Switching to {region} region camera");
        
        // Transition to region camera
        cameraTransitionCoroutine = StartCoroutine(TransitionToCamera(targetCamera));
        
        // Store active player and region
        activePlayer = player;
        currentRegion = region;
        isMainCameraActive = (targetCamera == mainCamera);
        isViewCameraActive = (targetCamera == viewCamera);
        
        // Update button state
        UpdateViewButtonState();
    }
    
    private Camera GetCameraForRegion(Tile.Region region)
    {
        switch (region)
        {
            case Tile.Region.Vulkan:
                return vulkanCamera;
            case Tile.Region.Atlanta:
                return atlantaCamera;
            case Tile.Region.Celestyel:
                return celestyelCamera;
            case Tile.Region.Berg:
                return bergCamera;
            default:
                return mainCamera;
        }
    }
    
    // This is the method that can be called from any script to switch to main camera
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
        currentRegion = Tile.Region.None;
        isMainCameraActive = true;
        isViewCameraActive = false;
        
        // Update button state
        UpdateViewButtonState();
    }
    
    public void SwitchToDiceCamera()
    {
        if (diceCamera == null)
        {
            Debug.LogWarning("Dice camera is not assigned");
            return;
        }
        
        // Cancel any ongoing transition
        if (cameraTransitionCoroutine != null)
        {
            StopCoroutine(cameraTransitionCoroutine);
        }
        
        Debug.Log("Switching to dice camera");
        
        // Transition to dice camera
        cameraTransitionCoroutine = StartCoroutine(TransitionToCamera(diceCamera));
        
        // Not changing activePlayer or currentRegion since this is temporary
        isMainCameraActive = false;
        isViewCameraActive = false;
        
        // Update button state
        UpdateViewButtonState();
    }
    
    // Method to toggle view camera
    public void ToggleViewCamera()
    {
        if (viewCamera == null)
        {
            Debug.LogWarning("View camera is not assigned");
            return;
        }
        
        // If we're already in view camera, switch back to main
        if (isViewCameraActive)
        {
            SwitchToMainCamera();
        }
        // Otherwise, switch to view camera
        else
        {
            // Cancel any ongoing transition
            if (cameraTransitionCoroutine != null)
            {
                StopCoroutine(cameraTransitionCoroutine);
            }
            
            Debug.Log("Switching to view camera");
            
            // Transition to view camera
            cameraTransitionCoroutine = StartCoroutine(TransitionToCamera(viewCamera));
            
            isMainCameraActive = false;
            isViewCameraActive = true;
            
            // Update button state
            UpdateViewButtonState();
        }
    }
    
    // Update the state of the view button based on active camera
    private void UpdateViewButtonState()
    {
        if (viewToggleButton != null)
        {
            // Enable view button when safe to do so (not during dice roll)
            viewToggleButton.interactable = (activeCamera != diceCamera);
        }
    }
    
    private IEnumerator TransitionToCamera(Camera targetCamera)
    {
        // Enable the target camera
        targetCamera.enabled = true;
        activeCamera = targetCamera;
        
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
    
    private void DisableAllRegionCameras()
    {
        if (vulkanCamera != null) vulkanCamera.enabled = false;
        if (atlantaCamera != null) atlantaCamera.enabled = false;
        if (celestyelCamera != null) celestyelCamera.enabled = false;
        if (bergCamera != null) bergCamera.enabled = false;
    }
    
    // Toggle cursor control on/off
    public void ToggleCursorControl(bool enable)
    {
        enableCursorControl = enable;
    }

    // Called when dice start rolling
    public void OnDiceRollStart()
    {
        SwitchToDiceCamera();
    }

    // Called when dice have finished rolling
    public void OnDiceRollComplete(int rollValue)
    {
        // No need to switch cameras here - the GameManager will handle this 
        // by checking the player's current tile before movement starts
        // This ensures we immediately switch to the correct region camera when the player moves
    }
    
    // Called when a player lands on a tile
    public void OnPlayerLandedOnTile(Player player, Tile.Region region)
    {
        // Switch camera based on the region of the tile
        SwitchToRegionCamera(region, player);
    }
}