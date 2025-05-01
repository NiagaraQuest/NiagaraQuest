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
        
        // Check for F key to activate life sharing
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Only process F key if the give life button is active and visible
            if (giveLifeButton != null && giveLifeButton.gameObject.activeSelf && giveLifeButton.interactable)
            {
                Debug.Log("🔑 F key pressed - activating life sharing menu");
                ShowLifeSharingOptions();
            }
        }
        
        // If player selection panel is active, check for number keys to select players
        if (playerSelectionPanel != null && playerSelectionPanel.activeSelf)
        {
            // Check for number keys 1-9 for quick selection
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    // If we have that many options, select it
                    if (i < createdOptionButtons.Count)
                    {
                        Button optionButton = createdOptionButtons[i].GetComponent<Button>();
                        if (optionButton != null && optionButton.interactable)
                        {
                            optionButton.onClick.Invoke();
                            Debug.Log($"🔢 Quick-selected player option {i+1} using number key");
                        }
                    }
                }
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
                        buttonText.text = $"Give Life ({currentPlayer.lives}) [F]";
                    }
                }
            }
        }
    }

    public void InitializeWithGameManager(GameManager gm)
    {
        // Set the game manager reference
        gameManager = gm;
        
        Debug.Log("🔄 LifeSharingManager initialized with GameManager");
        
        // Ensure dice haven't been rolled at initialization
        hasDiceBeenRolledThisTurn = false;
        
        // Force button visibility check
        bool conditionsMet = CheckAllConditions();
        Debug.Log($"Life sharing conditions met at initialization: {conditionsMet}");
        
        // Force update button visibility
        if (giveLifeButton != null)
        {
            giveLifeButton.gameObject.SetActive(conditionsMet);
            Debug.Log($"Life sharing button visibility set to: {conditionsMet}");
            
            // Update button text to include F key hint
            Text buttonText = giveLifeButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                Player currentPlayer = gm.selectedPlayer?.GetComponent<Player>();
                if (currentPlayer != null)
                {
                    buttonText.text = $"Give Life ({currentPlayer.lives}) [F]";
                }
            }
            
            // Log current player status
            if (gm != null && gm.selectedPlayer != null)
            {
                Player currentPlayer = gm.selectedPlayer.GetComponent<Player>();
                if (currentPlayer != null)
                {
                    Debug.Log($"Current player: {currentPlayer.gameObject.name}, Lives: {currentPlayer.lives}");
                }
            }
        }
        else
        {
            Debug.LogError("Give Life Button reference is missing!");
        }
    }
    
    // Check all conditions that must be true for the button to appear
    private bool CheckAllConditions()
    {
        Debug.Log("🔍 Checking life sharing button conditions:");
        
        // 1. Check if dice have been rolled
        if (hasDiceBeenRolledThisTurn)
        {
            Debug.Log("⛔ Dice have been rolled this turn - cannot share lives");
            return false;
        }
        
        // 2. Check if GameManager and current player exist
        if (gameManager == null || gameManager.selectedPlayer == null)
        {
            Debug.Log("⛔ GameManager or selected player is null");
            return false;
        }
        
        // 3. Check if current player has 3+ lives
        Player currentPlayer = gameManager.selectedPlayer.GetComponent<Player>();
        if (currentPlayer == null)
        {
            Debug.Log("⛔ Current player component is null");
            return false;
        }
        
        if (currentPlayer.lives < 3)
        {
            Debug.Log($"⛔ Current player {currentPlayer.gameObject.name} only has {currentPlayer.lives} lives (needs at least 3)");
            return false;
        }
        
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
                Debug.Log($"✅ Found eligible player {otherPlayer.gameObject.name} with 1 life");
                break;
            }
        }
        
        if (!existsPlayerWithOneLife)
        {
            Debug.Log("⛔ No players found with exactly 1 life");
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
        int playerIndex = 1; // For number key shortcuts
        foreach (Player player in eligiblePlayers)
        {
            GameObject optionObj = Instantiate(playerOptionPrefab, playerOptionsContainer);
            
            // Set text
            Text optionText = optionObj.GetComponentInChildren<Text>();
            if (optionText != null)
            {
                // Add number for keyboard shortcut if 9 or fewer options
                if (eligiblePlayers.Count <= 9)
                {
                    optionText.text = $"{playerIndex}. {player.gameObject.name}";
                }
                else
                {
                    optionText.text = player.gameObject.name;
                }
            }
            
            // Add click handler
            Button optionButton = optionObj.GetComponent<Button>();
            if (optionButton != null)
            {
                Player targetPlayer = player; // Important: Create local variable to capture value correctly
                optionButton.onClick.AddListener(() => GiveLifeToPlayer(targetPlayer));
            }
            
            createdOptionButtons.Add(optionObj);
            playerIndex++;
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