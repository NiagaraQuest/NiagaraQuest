using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    public Camera mainCamera;
    public Camera diceCamera;
    public Camera viewCamera;
    
    [Header("Region Cameras")]
    public Camera vulkanCamera;
    public Camera atlantaCamera;
    public Camera celestyelCamera;
    public Camera bergCamera;
    
    private Camera activeCamera;
    private Tile.Region currentRegion = Tile.Region.None;
    private Player activePlayer;
    
    [Header("Camera Settings")]
    public float transitionDuration = 0.5f;
    
    [Header("Cursor Camera Controls")]
    public bool enableCursorControl = true;
    public float cursorSensitivity = 2.0f;
    public float smoothTime = 0.1f;
    public float rotationLimit = 60f;
    public int dragMouseButton = 0;
    
    [Header("UI References")]
    public UnityEngine.UI.Button viewToggleButton;
    
    private CameraUIManager cameraUIManager;
    
    private Quaternion initialRotation;
    private Quaternion initialViewRotation;
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
        
        diceManager = FindObjectOfType<DiceManager>();
        if (diceManager != null && diceCamera != null)
        {
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
        
        cameraUIManager = FindObjectOfType<CameraUIManager>();
        if (cameraUIManager == null)
        {
            Debug.LogWarning("CameraUIManager not found in the scene!");
        }
        
        if (viewToggleButton != null)
        {
            viewToggleButton.onClick.RemoveAllListeners();
            viewToggleButton.onClick.AddListener(OnViewToggleButtonClicked);
            UpdateViewButtonState();
        }
        
        // Initialize audio listeners state
        SetupAudioListeners();
    }

    private void OnDestroy()
    {
        if (diceManager != null)
        {
            diceManager.OnDiceRollStart -= OnDiceRollStart;
            diceManager.OnDiceRollComplete -= OnDiceRollComplete;
        }
        
        if (viewToggleButton != null)
        {
            viewToggleButton.onClick.RemoveListener(OnViewToggleButtonClicked);
        }
    }

    void Start()
    {
        if (mainCamera != null)
        {
            mainCamera.enabled = true;
            initialRotation = mainCamera.transform.rotation;
            currentEulerAngles = mainCamera.transform.eulerAngles;
            activeCamera = mainCamera;
        }
        
        if (viewCamera != null)
        {
            initialViewRotation = viewCamera.transform.rotation;
            viewEulerAngles = viewCamera.transform.eulerAngles;
            viewCamera.enabled = false;
        }
        
        DisableAllRegionCameras();
        
        if (diceCamera != null)
        {
            diceCamera.enabled = false;
        }
        
        isMainCameraActive = true;
        isViewCameraActive = false;
        
        UpdateViewButtonState();
        
        // Make sure audio listeners are set up correctly at start
        UpdateAudioListeners(mainCamera);
    }
    
    // New method to set up audio listeners
    private void SetupAudioListeners()
    {
        // Get all cameras with their audio listeners
        Camera[] allCameras = new Camera[] { 
            mainCamera, 
            diceCamera, 
            viewCamera, 
            vulkanCamera, 
            atlantaCamera, 
            celestyelCamera, 
            bergCamera 
        };
        
        // Disable all audio listeners initially
        foreach (Camera cam in allCameras)
        {
            if (cam != null)
            {
                AudioListener listener = cam.GetComponent<AudioListener>();
                
                // Add an audio listener if it doesn't exist
                if (listener == null)
                {
                    listener = cam.gameObject.AddComponent<AudioListener>();
                    Debug.Log($"Added AudioListener to {cam.name}");
                }
                
                // Disable all listeners initially
                listener.enabled = false;
            }
        }
        
        // Enable only the main camera's audio listener
        if (mainCamera != null)
        {
            AudioListener mainListener = mainCamera.GetComponent<AudioListener>();
            if (mainListener != null)
            {
                mainListener.enabled = true;
                Debug.Log($"Enabled AudioListener on {mainCamera.name}");
            }
        }
    }
    
    // New method to update audio listeners when camera changes
    private void UpdateAudioListeners(Camera newActiveCamera)
    {
        if (newActiveCamera == null) return;
        
        // Get all cameras with their audio listeners
        Camera[] allCameras = new Camera[] { 
            mainCamera, 
            diceCamera, 
            viewCamera, 
            vulkanCamera, 
            atlantaCamera, 
            celestyelCamera, 
            bergCamera 
        };
        
        // Disable all audio listeners first
        foreach (Camera cam in allCameras)
        {
            if (cam != null)
            {
                AudioListener listener = cam.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = false;
                }
            }
        }
        
        // Enable only the active camera's audio listener
        AudioListener activeListener = newActiveCamera.GetComponent<AudioListener>();
        if (activeListener != null)
        {
            activeListener.enabled = true;
            Debug.Log($"Enabled AudioListener on {newActiveCamera.name}");
        }
        else
        {
            // Add an audio listener if it doesn't exist
            activeListener = newActiveCamera.gameObject.AddComponent<AudioListener>();
            activeListener.enabled = true;
            Debug.Log($"Added and enabled AudioListener on {newActiveCamera.name}");
        }
    }
    
    void Update()
    {
        if (isMainCameraActive && enableCursorControl && mainCamera != null && activeCamera == mainCamera)
        {
            HandleCursorCameraMovement(mainCamera, ref currentEulerAngles, initialRotation);
        }
        
        if (isViewCameraActive && enableCursorControl && viewCamera != null && activeCamera == viewCamera)
        {
            HandleCursorCameraMovement(viewCamera, ref viewEulerAngles, initialViewRotation);
        }
    }

    
    private void OnViewToggleButtonClicked()
    {
        if (cameraUIManager != null)
        {
            cameraUIManager.ToggleCameraSelectionPanel();
        }
        else
        {
            ToggleViewCamera();
        }
    }
    
    private void HandleCursorCameraMovement(Camera camera, ref Vector3 eulerAngles, Quaternion initialRot)
    {
        if (Input.GetMouseButtonDown(dragMouseButton))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }
        
        if (Input.GetMouseButtonUp(dragMouseButton))
        {
            isDragging = false;
        }
        
        if (isDragging)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            
            float mouseX = mouseDelta.x * cursorSensitivity * 0.01f;
            float mouseY = mouseDelta.y * cursorSensitivity * 0.01f;
            
            eulerAngles.y += mouseX;
            eulerAngles.x -= mouseY;
            
            lastMousePosition = currentMousePosition;
        }
        
        Quaternion targetRotation = Quaternion.Euler(eulerAngles);
        
        float angle = Quaternion.Angle(initialRot, targetRotation);
        if (angle > rotationLimit)
        {
            targetRotation = Quaternion.Slerp(
                targetRotation,
                initialRot,
                (angle - rotationLimit) / angle
            );
            
            eulerAngles = targetRotation.eulerAngles;
        }
        
        camera.transform.rotation = Quaternion.Slerp(
            camera.transform.rotation,
            targetRotation,
            1 - Mathf.Exp(-Time.deltaTime / smoothTime)
        );
    }
    
    public void SwitchToRegionCamera(Tile.Region region, Player player)
    { 
        Camera targetCamera = GetCameraForRegion(region);
        
        if (targetCamera == null)
        {
            Debug.LogWarning($"No camera assigned for region {region}, using main camera");
            targetCamera = mainCamera;
        }
        
        if (cameraTransitionCoroutine != null)
        {
            StopCoroutine(cameraTransitionCoroutine);
        }
        
        Debug.Log($"Switching to {region} region camera");
        
        cameraTransitionCoroutine = StartCoroutine(TransitionToCamera(targetCamera));
        
        activePlayer = player;
        currentRegion = region;
        isMainCameraActive = (targetCamera == mainCamera);
        isViewCameraActive = (targetCamera == viewCamera);
        
        UpdateViewButtonState();
        
        // Update audio listeners when changing cameras
        UpdateAudioListeners(targetCamera);
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
    
    public void SwitchToMainCamera()
    {
        if (mainCamera == null) return;
        
        if (cameraTransitionCoroutine != null)
        {
            StopCoroutine(cameraTransitionCoroutine);
        }
        
        Debug.Log("Switching to main camera");
        
        currentEulerAngles = initialRotation.eulerAngles;
        
        cameraTransitionCoroutine = StartCoroutine(TransitionToCamera(mainCamera));
        
        activePlayer = null;
        currentRegion = Tile.Region.None;
        isMainCameraActive = true;
        isViewCameraActive = false;
        
        UpdateViewButtonState();
        
        // Update audio listeners when changing to main camera
        UpdateAudioListeners(mainCamera);
    }
    
    public void SwitchToDiceCamera()
    {
        if (diceCamera == null)
        {
            Debug.LogWarning("Dice camera is not assigned");
            return;
        }
        
        if (cameraTransitionCoroutine != null)
        {
            StopCoroutine(cameraTransitionCoroutine);
        }
        
        Debug.Log("Switching to dice camera");
        
        cameraTransitionCoroutine = StartCoroutine(TransitionToCamera(diceCamera));
        
        isMainCameraActive = false;
        isViewCameraActive = false;
        
        UpdateViewButtonState();
        
        // Update audio listeners when changing to dice camera
        UpdateAudioListeners(diceCamera);
    }
    
    public void ToggleViewCamera()
    {
        if (viewCamera == null)
        {
            Debug.LogWarning("View camera is not assigned");
            return;
        }
        
        if (isViewCameraActive)
        {
            SwitchToMainCamera();
        }
        else
        {
            if (cameraTransitionCoroutine != null)
            {
                StopCoroutine(cameraTransitionCoroutine);
            }
            
            Debug.Log("Switching to view camera");
            
            cameraTransitionCoroutine = StartCoroutine(TransitionToCamera(viewCamera));
            
            isMainCameraActive = false;
            isViewCameraActive = true;
            
            UpdateViewButtonState();
            
            // Update audio listeners when changing to view camera
            UpdateAudioListeners(viewCamera);
        }
    }
    
    private void UpdateViewButtonState()
    {
        if (viewToggleButton != null)
        {
            viewToggleButton.interactable = (activeCamera != diceCamera);
        }
    }
    
    private IEnumerator TransitionToCamera(Camera targetCamera)
    {
        targetCamera.enabled = true;
        activeCamera = targetCamera;
        
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam != targetCamera)
            {
                cam.enabled = false;
            }
        }
        
        // Update audio listeners during the transition as well
        UpdateAudioListeners(targetCamera);
        
        yield return null;
        
        cameraTransitionCoroutine = null;
    }
    
    private void DisableAllRegionCameras()
    {
        if (vulkanCamera != null) vulkanCamera.enabled = false;
        if (atlantaCamera != null) atlantaCamera.enabled = false;
        if (celestyelCamera != null) celestyelCamera.enabled = false;
        if (bergCamera != null) bergCamera.enabled = false;
    }
    
    public void ToggleCursorControl(bool enable)
    {
        enableCursorControl = enable;
    }

    public void OnDiceRollStart()
    {
        SwitchToDiceCamera();
    }

    public void OnDiceRollComplete(int rollValue)
    {
    }
    
    public void OnPlayerLandedOnTile(Player player, Tile.Region region)
    {
        SwitchToRegionCamera(region, player);
    }
    
    public Camera GetActiveCamera()
    {
        return activeCamera;
    }

    public void EnableViewToggle(){
        if (viewToggleButton != null)
        {
            viewToggleButton.interactable = true;
        }
    }

    public void DisableViewToggle(){
        if (viewToggleButton != null)
        {
            viewToggleButton.interactable = false;
        }
    }
}