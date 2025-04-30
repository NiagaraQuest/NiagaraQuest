using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HUDUIManager : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Button closeSettingsButton;

    [Header("Scene References")]
    public string menuSceneName = "MenuScene";
    [Header("UI Scripts")]
    public CameraUIManager cameraUIManager;
    private bool isPaused = false;

    private void Start()
    {
        // Ensure the pause panel is hidden at start
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Ensure the settings panel is hidden at start
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Set up button listeners
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        // Pause button
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        // Resume button
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        // Settings button
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        // Exit button
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitToMenu);

        // Close settings button
        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);
    }

    // Toggle pause state when the pause button is clicked
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    // Pause the game and show the pause panel
    public void PauseGame()
    {
        isPaused = true;
        CameraManager.Instance.DisableViewToggle();
        cameraUIManager.HideCameraSelectionPanel();
        
        // Show the pause panel
        if (pausePanel != null)
            pausePanel.SetActive(true);
        
        // Set time scale to 0 to pause game mechanics
        Time.timeScale = 0f;
        
        // Disable dice roll button if it exists
        DisableDiceRollButton();
        
        Debug.Log("⏸️ Game paused");
    }

    // Resume the game and hide the pause panel
    public void ResumeGame()
    {
        isPaused = false;
        CameraManager.Instance.EnableViewToggle();
        
        // Hide the pause panel
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        // Hide the settings panel if it's open
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        // Restore normal time scale
        Time.timeScale = 1f;
        
        // Re-enable dice roll button if it's the current player's turn
        EnableDiceRollButtonIfNeeded();
        
        Debug.Log("▶️ Game resumed");
    }

    // Open the settings panel
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            
            // Hide the pause panel to avoid UI overlap
            if (pausePanel != null)
                pausePanel.SetActive(false);
            
            Debug.Log("⚙️ Settings opened");
        }
    }

    // Close the settings panel and return to pause menu
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            
            // Show the pause panel again
            if (pausePanel != null)
                pausePanel.SetActive(true);
            
            Debug.Log("⚙️ Settings closed");
        }
    }

    // Exit to the main menu
    public void ExitToMenu()
    {
        // Restore normal time scale before scene change
        Time.timeScale = 1f;
        
        Debug.Log("🚪 Exiting to main menu: " + menuSceneName);
        
        try
        {
            // Load the menu scene
            SceneManager.LoadScene(menuSceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error loading menu scene: {ex.Message}");
        }
    }

    // Disable dice roll button when game is paused
    private void DisableDiceRollButton()
    {
        if (GameManager.Instance != null && GameManager.Instance.diceManager != null)
        {
            GameManager.Instance.diceManager.DisableRollButton();
            Debug.Log("🎲 Dice roll button disabled during pause");
        }
    }

    // Re-enable dice roll button when game is resumed (if it's the player's turn)
    private void EnableDiceRollButtonIfNeeded()
    {
        if (GameManager.Instance != null && GameManager.Instance.diceManager != null)
        {
            // Check if the current turn has already rolled the dice
            if (!GameManager.Instance.hasDiceBeenRolledThisTurn)
            {
                GameManager.Instance.diceManager.EnableRollButton();
                Debug.Log("🎲 Dice roll button re-enabled after pause");
            }
        }
    }

    // Handle ESC key for pausing/resuming
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If settings panel is open, close it and return to pause menu
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            // Otherwise toggle between pause and play
            else
            {
                TogglePause();
            }
            
            Debug.Log($"🔄 ESC key pressed: Game is now {(isPaused ? "paused" : "running")}");
        }
    }

    // Method to manually show/hide the pause button
    public void SetPauseButtonVisible(bool visible)
    {
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(visible);
    }

}