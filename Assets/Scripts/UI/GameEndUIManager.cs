using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameEndUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject endPanel;
    public TextMeshProUGUI endMessageText;
    public Button exitButton;
    public Button playAgainButton;
    
    [Header("References")]
    public GameEndManager gameEndManager;
    
    private void Awake()
    {
        // Find or add the GameEndManager component
        if (gameEndManager == null)
        {
            gameEndManager = FindObjectOfType<GameEndManager>();
            if (gameEndManager == null)
            {
                Debug.LogError("❌ GameEndManager not found! Please add it to the scene.");
                return;
            }
        }
        
        // Make sure endPanel is disabled initially
        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("❌ End panel not assigned in GameEndUIManager!");
        }
    }
    
    private void Start()
    {
        SetupButtons();
    }
    
    // Setup all UI buttons
    private void SetupButtons()
    {
        // Try to find buttons if they're not assigned
        if (exitButton == null || playAgainButton == null)
        {
            FindAndSetupButtons();
        }
        
        // Set up button listeners with the correct references to GameEndManager
        if (exitButton != null && gameEndManager != null)
        {
            // Clear existing listeners to avoid duplicates
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(gameEndManager.ExitToMenu);
            Debug.Log("✅ Exit button set up");
        }
        else
        {
            Debug.LogError("❌ Exit button not found or GameEndManager not assigned!");
        }
        
        if (playAgainButton != null && gameEndManager != null)
        {
            // Clear existing listeners to avoid duplicates
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(gameEndManager.RestartGame);
            Debug.Log("✅ Play Again button set up");
        }
        else
        {
            Debug.LogError("❌ Play Again button not found or GameEndManager not assigned!");
        }
    }
    
    // Try to find buttons automatically if they're not assigned in the Inspector
    private void FindAndSetupButtons()
    {
        if (endPanel != null)
        {
            if (exitButton == null)
            {
                exitButton = endPanel.transform.Find("ExitButton")?.GetComponent<Button>();
                if (exitButton != null)
                {
                    Debug.Log("✅ Exit button found automatically");
                }
                else
                {
                    Debug.LogWarning("⚠️ Could not find 'ExitButton' in end panel");
                }
            }
            
            if (playAgainButton == null)
            {
                playAgainButton = endPanel.transform.Find("PlayAgainButton")?.GetComponent<Button>();
                if (playAgainButton != null)
                {
                    Debug.Log("✅ Play Again button found automatically");
                }
                else
                {
                    Debug.LogWarning("⚠️ Could not find 'PlayAgainButton' in end panel");
                }
            }
        }
    }
    
    // Show defeat screen with custom message
    public void ShowDefeatScreen(Player losingPlayer)
    {
        if (endPanel == null)
        {
            Debug.LogError("❌ End panel not assigned in GameEndUIManager!");
            return;
        }
        
        // Clean up all UI panels before showing game end screen
        if (gameEndManager != null)
        {
            gameEndManager.CleanupUIForGameEnd();
        }
        
        // Make sure buttons are set up (in case panel was inactive before)
        SetupButtons();
        
        // Show the panel
        endPanel.SetActive(true);
        
        // Update message text
        if (endMessageText != null)
        {
            string playerName = losingPlayer != null ? losingPlayer.gameObject.name.Replace("Player", "") : "Someone";
            endMessageText.text = $"You lose!\n{playerName} has lost all lives!";
        }
    }
    
    // Show victory screen with custom message
    public void ShowVictoryScreen(Player winningPlayer)
    {
        if (endPanel == null)
        {
            Debug.LogError("❌ End panel not assigned in GameEndUIManager!");
            return;
        }
        
        // Clean up all UI panels before showing game end screen
        if (gameEndManager != null)
        {
            gameEndManager.CleanupUIForGameEnd();
        }
        
        // Make sure buttons are set up (in case panel was inactive before)
        SetupButtons();
        
        // Show the panel
        endPanel.SetActive(true);
        
        // Update message text
        if (endMessageText != null)
        {
            string playerName = winningPlayer != null ? winningPlayer.gameObject.name.Replace("Player", "") : "Someone";
            endMessageText.text = $"Victory!\nWon with the help of {playerName}!";
        }
    }
}