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
        }
        
        // Disable all player cameras at start
        DisableAllPlayerCameras();
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
        
        // Transition to main camera
        cameraTransitionCoroutine = StartCoroutine(TransitionToCamera(mainCamera));
        
        activePlayer = null;
        activePlayerCamera = null;
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
}