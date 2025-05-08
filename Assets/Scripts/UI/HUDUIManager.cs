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
    public Button restartButton;  // Add new restart button reference
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
    
    // Store the player's movement state before pausing
    private bool wasPlayerMoving = false;
    // Flag to track if dice were rolling when paused
    private bool wereDiceRolling = false;

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
            pauseButton.onClick.AddListener(() => {
                PlayButtonSound();
                TogglePause();
            });

        // Resume button
        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => {
                PlayButtonSound();
                ResumeGame();
            });
            
        // Restart button
        if (restartButton != null)
            restartButton.onClick.AddListener(() => {
                PlayButtonSound();
                RestartGame();
            });

        // Settings button
        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => {
                PlayButtonSound();
                OpenSettings();
            });

        // Exit button
        if (exitButton != null)
            exitButton.onClick.AddListener(() => {
                PlayButtonSound();
                ExitToMenu();
            });

        // Close settings button
        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(() => {
                PlayButtonSound();
                CloseSettings();
            });
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
        
        // Store dice state before pausing
        StoreDiceState();
        
        // Pause player movement
        PausePlayerMovement();
        
        // Pause dice sounds
        PauseDiceSounds();
        
        // Set time scale to 0 to pause game mechanics (do this last)
        Time.timeScale = 0f;
        
        // Disable dice roll button if it exists
        DisableDiceRollButton();
        
        Debug.Log("⏸️ Game paused");
    }

    // Resume the game and hide the pause panel
    public void ResumeGame()
    {
        isPaused = false;
        
        // Hide the pause panel
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        // Hide the settings panel if it's open
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        // Restore normal time scale first so any coroutines can continue
        Time.timeScale = 1f;
        
        CameraManager.Instance.EnableViewToggle();
        
        // Resume player movement if it was moving before
        ResumePlayerMovement();
        
        // Resume dice sounds if they were rolling before
        ResumeDiceSounds();
        
        // Re-enable dice roll button if it's the current player's turn
        EnableDiceRollButtonIfNeeded();
        
        Debug.Log("▶️ Game resumed");
    }

    // Store the dice state before pausing
    private void StoreDiceState()
    {
        if (GameManager.Instance != null && GameManager.Instance.diceManager != null)
        {
            // Check if dice are currently rolling using the DiceHaveFinishedRolling property
            wereDiceRolling = !GameManager.Instance.diceManager.DiceHaveFinishedRolling;
            Debug.Log($"Stored dice state: wereDiceRolling = {wereDiceRolling}");
        }
    }

    // Pause player movement
    private void PausePlayerMovement()
    {
        if (GameManager.Instance != null && GameManager.Instance.selectedPlayer != null)
        {
            GameObject player = GameManager.Instance.selectedPlayer;
            Player playerScript = player.GetComponent<Player>();
            
            if (playerScript != null)
            {
                // Store current movement state to restore later
                wasPlayerMoving = playerScript.isMoving;
                
                // Stop movement
                playerScript.isMoving = false;
                
                // Stop movement sound
                if (PlayerSound.Instance != null)
                {
                    PlayerSound.Instance.StopMovementSound(player);
                }
                
                Debug.Log($"Paused player movement (was moving: {wasPlayerMoving})");
            }
        }
    }
    
    // Resume player movement if it was moving before
    private void ResumePlayerMovement()
    {
        if (GameManager.Instance != null && GameManager.Instance.selectedPlayer != null)
        {
            GameObject player = GameManager.Instance.selectedPlayer;
            Player playerScript = player.GetComponent<Player>();
            
            if (playerScript != null && wasPlayerMoving)
            {
                // Restore movement state only if it was moving before
                playerScript.isMoving = wasPlayerMoving;
                
                // Restart movement sound if needed
                if (wasPlayerMoving && PlayerSound.Instance != null)
                {
                    PlayerSound.Instance.PlayMovementSound(player);
                }
                
                Debug.Log($"Resumed player movement (was moving: {wasPlayerMoving})");
                
                // Reset the flag
                wasPlayerMoving = false;
            }
        }
    }
    
    // Pause dice sounds
    private void PauseDiceSounds()
    {
        if (DiceSound.Instance != null)
        {
            // We need to stop dice sounds regardless of whether they were playing or not
            DiceSound.Instance.StopDiceRolling();
            
            Debug.Log($"Paused dice sounds (were dice rolling: {wereDiceRolling})");
        }
    }
    
    // Resume dice sounds if they were playing
    private void ResumeDiceSounds()
    {
        if (DiceSound.Instance != null && wereDiceRolling)
        {
            // Only restart the rolling sound if it was playing before
            DiceSound.Instance.PlayDiceRolling();
            
            Debug.Log($"Resumed dice sounds (were rolling: {wereDiceRolling})");
            
            // Note: We don't reset the flag here, as the dice rolling coroutine
            // will handle the end of rolling and triggering completion events
        }
    }

    public void RestartGame()
    {
        Debug.Log("🔄 Restarting game with same player profiles");
        
        // Ensure time scale is normal before scene reload
        Time.timeScale = 1f;
        
        // Cache the current active player data
        SaveCurrentPlayerProfiles();
        
        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    

    private void SaveCurrentPlayerProfiles()
    {
        // Get GameManager instance
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || gameManager.players == null)
        {
            Debug.LogWarning("⚠️ GameManager or players list not available!");
            return;
        }
        PlayerPrefs.Save();
        
        Debug.Log("✅ Player profiles preserved for scene reload");
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

    // Re-enable dice roll button when game is resumed (if it's the current player's turn)
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
            // Play button sound when ESC is pressed
            PlayButtonSound();
            
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

    public void PlayButtonSound()
    {
        // Play button sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuButton();
    }
}