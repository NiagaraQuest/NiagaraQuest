using UnityEngine;
using UnityEngine.UI;

public class CameraUIManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject cameraSelectionPanel;
    
    [Header("Camera Buttons")]
    public Button mainCameraButton;
    public Button viewCameraButton;
    public Button vulkanCameraButton;
    public Button atlantaCameraButton;
    public Button celestyelCameraButton;
    public Button bergCameraButton;
    public Button returnButton;
    
    private CameraManager cameraManager;
    
    private void Start()
    {
        cameraManager = CameraManager.Instance;
        if (cameraManager == null)
        {
            Debug.LogError("CameraManager not found! CameraUIManager requires a CameraManager instance.");
            return;
        }
        
        if (cameraSelectionPanel != null)
        {
            cameraSelectionPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Camera selection panel not assigned!");
        }
        
        SetupButtonListeners();
    }
    
    private void SetupButtonListeners()
    {
        if (mainCameraButton != null)
        {
            mainCameraButton.onClick.AddListener(() => {
                cameraManager.SwitchToMainCamera();
                HideCameraSelectionPanel();
            });
        }
        
        if (viewCameraButton != null)
        {
            viewCameraButton.onClick.AddListener(() => {
                cameraManager.SwitchToMainCamera();
                cameraManager.ToggleViewCamera();
                HideCameraSelectionPanel();
            });
        }
        
        if (vulkanCameraButton != null)
        {
            vulkanCameraButton.onClick.AddListener(() => {
                Player currentPlayer = GameManager.Instance?.GetCurrentPlayer();
                cameraManager.SwitchToRegionCamera(Tile.Region.Vulkan, currentPlayer);
                HideCameraSelectionPanel();
            });
        }
        
        if (atlantaCameraButton != null)
        {
            atlantaCameraButton.onClick.AddListener(() => {
                Player currentPlayer = GameManager.Instance?.GetCurrentPlayer();
                cameraManager.SwitchToRegionCamera(Tile.Region.Atlanta, currentPlayer);
                HideCameraSelectionPanel();
            });
        }
        
        if (celestyelCameraButton != null)
        {
            celestyelCameraButton.onClick.AddListener(() => {
                Player currentPlayer = GameManager.Instance?.GetCurrentPlayer();
                cameraManager.SwitchToRegionCamera(Tile.Region.Celestyel, currentPlayer);
                HideCameraSelectionPanel();
            });
        }
        
        if (bergCameraButton != null)
        {
            bergCameraButton.onClick.AddListener(() => {
                Player currentPlayer = GameManager.Instance?.GetCurrentPlayer();
                cameraManager.SwitchToRegionCamera(Tile.Region.Berg, currentPlayer);
                HideCameraSelectionPanel();
            });
        }
        
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(HideCameraSelectionPanel);
        }
    }
    
    public void ShowCameraSelectionPanel()
    {
        if (cameraSelectionPanel != null)
        {
            cameraSelectionPanel.SetActive(true);
        }
    }
    
    public void HideCameraSelectionPanel()
    {
        if (cameraSelectionPanel != null)
        {
            cameraSelectionPanel.SetActive(false);
        }
    }
    
    public void ToggleCameraSelectionPanel()
    {
        if (cameraSelectionPanel != null)
        {
            bool newState = !cameraSelectionPanel.activeSelf;
            cameraSelectionPanel.SetActive(newState);
        }
    }
}