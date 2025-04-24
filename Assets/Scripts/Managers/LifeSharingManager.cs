using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LifeSharingManager : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public Button giveLifeButton;
    public GameObject playerSelectionPanel;
    public Transform playerOptionsContainer;
    public GameObject playerOptionPrefab;

    [Header("Return Button")]
    public Button returnButton;
    
    private List<GameObject> createdOptionButtons = new List<GameObject>();
    private bool hasDiceBeenRolledThisTurn = false;
    
    private void Start()
    {
        // Find GameManager if not assigned
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("❌ GameManager not found!");
                this.enabled = false;
                return;
            }
        }
        
        // Setup give life button
        if (giveLifeButton != null)
        {
            giveLifeButton.onClick.AddListener(ShowLifeSharingOptions);
            giveLifeButton.gameObject.SetActive(false); // Hide initially
        }
        else
        {
            Debug.LogError("❌ Give Life Button not assigned!");
            this.enabled = false;
            return;
        }
        
        // Setup return button
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnToGame);
        }
        else
        {
            Debug.LogWarning("⚠️ Return Button not assigned!");
        }
        
        // Hide player selection panel initially
        if (playerSelectionPanel != null)
        {
            playerSelectionPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ Player Selection Panel not assigned!");
        }
    }
    
    // Check conditions every frame
    private void Update()
    {
        // Check if dice have been rolled this turn
        CheckDiceRolledState();
        
        // Update button visibility based on all conditions
        UpdateGiveLifeButtonVisibility();
        
        // Check for Escape key to return to game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Only process Escape if player selection panel is active
            if (playerSelectionPanel != null && playerSelectionPanel.activeSelf)
            {
                ReturnToGame();
            }
        }
    }
    
    // Check if dice have been rolled by monitoring the dice button state
    private void CheckDiceRolledState()
    {
        if (gameManager != null && gameManager.diceManager != null)
        {
            bool isDiceButtonEnabled = gameManager.diceManager.rollButton.interactable;
            
            // If dice button was enabled and is now disabled, dice were rolled
            if (!isDiceButtonEnabled && !hasDiceBeenRolledThisTurn)
            {
                OnDiceRolled();
            }
            
            // If dice button was disabled and is now enabled, new turn started
            if (isDiceButtonEnabled && hasDiceBeenRolledThisTurn)
            {
                OnNewTurn();
            }
        }
    }
    
    // Call this when dice are rolled
    public void OnDiceRolled()
    {
        hasDiceBeenRolledThisTurn = true;
        HidePlayerSelectionPanel();
    }
    
    // Call this when a new turn starts
    public void OnNewTurn()
    {
        hasDiceBeenRolledThisTurn = false;
    }
    
    // Update button visibility based on all required conditions
    public void UpdateGiveLifeButtonVisibility()
    {
        bool shouldShow = CheckAllConditions();
        
        if (giveLifeButton != null && giveLifeButton.gameObject.activeSelf != shouldShow)
        {
            giveLifeButton.gameObject.SetActive(shouldShow);
            
            if (shouldShow)
            {
                // Update button text to show current player lives
                Player currentPlayer = gameManager.selectedPlayer?.GetComponent<Player>();
                if (currentPlayer != null)
                {
                    Text buttonText = giveLifeButton.GetComponentInChildren<Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = $"Give Life ({currentPlayer.lives})";
                    }
                }
            }
        }
    }
    
    // Check all conditions that must be true for the button to appear
    private bool CheckAllConditions()
    {
        // 1. Check if dice have been rolled
        if (hasDiceBeenRolledThisTurn)
            return false;
            
        // 2. Check if GameManager and current player exist
        if (gameManager == null || gameManager.selectedPlayer == null)
            return false;
            
        // 3. Check if current player has 3+ lives
        Player currentPlayer = gameManager.selectedPlayer.GetComponent<Player>();
        if (currentPlayer == null || currentPlayer.lives < 3)
            return false;
            
        // 4. Check if any player has exactly 1 life
        bool existsPlayerWithOneLife = false;
        foreach (GameObject playerObj in gameManager.players)
        {
            if (playerObj == null || playerObj == gameManager.selectedPlayer)
                continue;
                
            Player otherPlayer = playerObj.GetComponent<Player>();
            if (otherPlayer != null && otherPlayer.lives == 1)
            {
                existsPlayerWithOneLife = true;
                break;
            }
        }
        
        return existsPlayerWithOneLife;
    }
    
    private void ShowLifeSharingOptions()
    {
        if (playerSelectionPanel == null || playerOptionsContainer == null || gameManager == null)
            return;
        
        // Don't show options if conditions aren't met
        if (!CheckAllConditions())
        {
            Debug.LogWarning("⚠️ Cannot show life sharing options - conditions not met");
            return;
        }
        
        // Clear any existing options
        ClearPlayerOptions();
        
        // Find players with exactly 1 life
        List<Player> eligiblePlayers = new List<Player>();
        foreach (GameObject playerObj in gameManager.players)
        {
            if (playerObj == null || playerObj == gameManager.selectedPlayer)
                continue;
            
            Player otherPlayer = playerObj.GetComponent<Player>();
            if (otherPlayer != null && otherPlayer.lives == 1)
            {
                eligiblePlayers.Add(otherPlayer);
            }
        }
        
        // Create option for each eligible player
        foreach (Player player in eligiblePlayers)
        {
            GameObject optionObj = Instantiate(playerOptionPrefab, playerOptionsContainer);
            
            // Set text
            Text optionText = optionObj.GetComponentInChildren<Text>();
            if (optionText != null)
            {
                optionText.text = player.gameObject.name;
            }
            
            // Add click handler
            Button optionButton = optionObj.GetComponent<Button>();
            if (optionButton != null)
            {
                Player targetPlayer = player; // Important: Create local variable to capture value correctly
                optionButton.onClick.AddListener(() => GiveLifeToPlayer(targetPlayer));
            }
            
            createdOptionButtons.Add(optionObj);
        }
        
        // Show the panel
        playerSelectionPanel.SetActive(true);
    }
    
    private void ClearPlayerOptions()
    {
        foreach (GameObject optionButton in createdOptionButtons)
        {
            if (optionButton != null)
            {
                Destroy(optionButton);
            }
        }
        
        createdOptionButtons.Clear();
    }
    
    private void GiveLifeToPlayer(Player receivingPlayer)
    {
        if (gameManager.selectedPlayer == null || receivingPlayer == null)
            return;
        
        Player givingPlayer = gameManager.selectedPlayer.GetComponent<Player>();
        if (givingPlayer == null || givingPlayer.lives < 3 || receivingPlayer.lives != 1)
            return;
        
        Debug.Log($"Player {givingPlayer.gameObject.name} giving life to {receivingPlayer.gameObject.name}");
        
        // Execute life transfer
        givingPlayer.lives--;
        receivingPlayer.lives++;
        
        Debug.Log($"New lives - {givingPlayer.gameObject.name}: {givingPlayer.lives}, {receivingPlayer.gameObject.name}: {receivingPlayer.lives}");
        
        // Hide panel and update button
        HidePlayerSelectionPanel();
    }
    
    public void HidePlayerSelectionPanel()
    {
        if (playerSelectionPanel != null)
        {
            playerSelectionPanel.SetActive(false);
        }
        
        ClearPlayerOptions();
    }
    
    // Public method that can be called from a Close button
    public void ClosePlayerSelection()
    {
        HidePlayerSelectionPanel();
    }

    public void ReturnToGame()
    {
        Debug.Log("Player chose to continue without giving a life");
        
        // Hide the player selection panel
        HidePlayerSelectionPanel();
        
        // Hide the give life button
        if (giveLifeButton != null)
        {
            giveLifeButton.gameObject.SetActive(false);
        }
        
        // Set the dice rolled flag to true to prevent the button from reappearing
        hasDiceBeenRolledThisTurn = true;
    }
}