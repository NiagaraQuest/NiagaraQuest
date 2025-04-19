using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CardUIManager : MonoBehaviour
{
    public static CardUIManager Instance;

    [Header("Card UI Elements")]
    public GameObject cardPanel;
    public TextMeshProUGUI cardTitleText;
    public TextMeshProUGUI cardDescriptionText;
    public Button continueButton;

    [Header("Player Selection UI")]
    public GameObject playerSelectionPanel;
    public TextMeshProUGUI selectionPromptText;
    public Transform playerButtonsContainer;
    public Button playerButtonPrefab;

    private CardTile currentTile;
    private int currentCardType;
    private Player currentPlayer;
    private Coroutine autoCloseCoroutine;

    [Header("Auto Close")]
    public float autoCloseTime = 3f;

    [Header("Gambler Card UI")]
    public GameObject gambleChoicePanel;
    public TextMeshProUGUI gamblePromptText;
    public Button gambleYesButton;
    public Button gambleNoButton;
    public GameObject gambleResultPanel;
    public TextMeshProUGUI gambleResultText;
    public Button gambleResultOkButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Hide all panels at start
        cardPanel.SetActive(false);
        playerSelectionPanel.SetActive(false);
        gambleChoicePanel.SetActive(false);
        gambleResultPanel.SetActive(false);
        
        // Add listener to the Continue button
        continueButton.onClick.AddListener(CloseCardPanel);
        
        // Subscribe to the player selection event
        CardManager.OnPlayerSelectionRequested += ShowPlayerSelectionUI;
        
        // Setup gambler button listeners
        if (gambleYesButton != null)
            gambleYesButton.onClick.AddListener(OnGambleYes);
        if (gambleNoButton != null)
            gambleNoButton.onClick.AddListener(OnGambleNo);
        if (gambleResultOkButton != null)
            gambleResultOkButton.onClick.AddListener(() => OnGambleResultClosed(false, false)); // Default values, will be overridden
    }

    private void OnDestroy()
    {
        // Unsubscribe from event
        CardManager.OnPlayerSelectionRequested -= ShowPlayerSelectionUI;
    }

    // Show the card UI with the given card information
    public void ShowCard(string title, string description, CardTile tile, Player player, int cardType)
    {
        // Store references
        currentTile = tile;
        currentPlayer = player;
        currentCardType = cardType;
        
        // Check if this is the Gambler card (assuming it's index 2)
        if (cardType == 2) // Gambler card
        {
            // Skip showing the regular card panel and show the gamble choice directly
            ShowGambleChoice(player);
            return;
        }
        
        // Cancel any existing auto-close routine
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        
        // Set UI text
        cardTitleText.text = title;
        cardDescriptionText.text = description;
        
        // Show the panel
        cardPanel.SetActive(true);
        
        // Auto-close after delay if set
        if (autoCloseTime > 0)
        {
            autoCloseCoroutine = StartCoroutine(AutoCloseCard(autoCloseTime));
        }
    }

    // Show player selection UI
    private void ShowPlayerSelectionUI(Player player, int cardType)
    {
        // Store current player and card type
        currentPlayer = player;
        currentCardType = cardType;
        
        // Clear existing buttons
        foreach (Transform child in playerButtonsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Set selection prompt
        selectionPromptText.text = "Select a player to swap positions with:";
        
        // Create buttons for each other player
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null && gameManager.players != null)
        {
            bool hasOptions = false;
            
            foreach (GameObject playerObj in gameManager.players)
            {
                Player otherPlayer = playerObj.GetComponent<Player>();
                
                // Skip the current player
                if (otherPlayer == player) continue;
                
                hasOptions = true;
                
                // Create button
                Button playerButton = Instantiate(playerButtonPrefab, playerButtonsContainer);
                TextMeshProUGUI buttonText = playerButton.GetComponentInChildren<TextMeshProUGUI>();
                
                if (buttonText != null)
                {
                    buttonText.text = otherPlayer.gameObject.name;
                }
                
                // Set up click handler
                playerButton.onClick.AddListener(() => OnPlayerSelected(otherPlayer));
            }
            
            if (!hasOptions)
            {
                // No other players available
                TextMeshProUGUI messageText = Instantiate(playerButtonPrefab, playerButtonsContainer)
                    .GetComponentInChildren<TextMeshProUGUI>();
                if (messageText != null)
                {
                    messageText.text = "No other players available!";
                }
                
                // Add a close button
                Button closeButton = Instantiate(playerButtonPrefab, playerButtonsContainer);
                TextMeshProUGUI closeText = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (closeText != null)
                {
                    closeText.text = "Close";
                }
                closeButton.onClick.AddListener(ClosePlayerSelectionUI);
            }
        }
        
        // Show the player selection panel
        playerSelectionPanel.SetActive(true);
    }

    public void ShowGambleChoice(Player player)
    {
        currentPlayer = player;
        
        // Make sure other panels are closed
        cardPanel.SetActive(false);
        playerSelectionPanel.SetActive(false);
        gambleResultPanel.SetActive(false);
        
        // Set up the prompt text
        gamblePromptText.text = "Do you want to gamble a life?\n\n50% chance to gain a life\n50% chance to lose a life";
        
        // Set up button listeners
        gambleYesButton.onClick.RemoveAllListeners();
        gambleNoButton.onClick.RemoveAllListeners();
        
        gambleYesButton.onClick.AddListener(OnGambleYes);
        gambleNoButton.onClick.AddListener(OnGambleNo);
        
        // Show the gamble choice panel
        gambleChoicePanel.SetActive(true);
    }

    // Called when the player chooses to gamble
    private void OnGambleYes()
    {
        // Hide the choice panel
        gambleChoicePanel.SetActive(false);
        
        // Determine the result (50/50 chance)
        bool win = Random.Range(0, 2) == 0;
        
        // Get the max lives from GameManager
        int maxLives = GameManager.Instance.maxLives;
        bool hasMaxLives = currentPlayer.lives >= maxLives;
        
        // Apply the effect
        if (win)
        {
            // Win case
            if (hasMaxLives)
            {
                // Already at max lives, move player forward 8 tiles
                ShowGambleResult(true, true);
            }
            else
            {
                // Gain a life
                currentPlayer.GainLife();
                ShowGambleResult(true, false);
            }
        }
        else
        {
            // Lose case
            currentPlayer.LoseLife();
            ShowGambleResult(false, false);
            
            // Check if player lost all lives
            if (currentPlayer.lives <= 0)
            {
                GameManager.Instance.CheckPlayerLives();
            }
        }
    }

    // Called when the player chooses not to gamble
    private void OnGambleNo()
    {
        // Hide the choice panel
        gambleChoicePanel.SetActive(false);
        
        // Continue game flow
        if (currentTile != null)
        {
            currentTile.ContinueGame();
        }
    }

    // Show the gamble result panel
    private void ShowGambleResult(bool win, bool moveForward)
    {
        // Store the result parameters for the OK button callback
        bool finalWin = win;
        bool finalMoveForward = moveForward;
        
        string resultMessage;
        
        if (win)
        {
            if (moveForward)
            {
                resultMessage = "<color=#4CAF50>You won!</color>\n\nYou already have maximum lives.\nYou will move forward 8 tiles!";
            }
            else
            {
                resultMessage = "<color=#4CAF50>You won!</color>\n\nYou gained 1 life!";
            }
        }
        else
        {
            resultMessage = "<color=#F44336>You lost!</color>\n\nYou lost 1 life!";
        }
        
        // Set up the result text
        gambleResultText.text = resultMessage;
        
        // Set up button listener
        gambleResultOkButton.onClick.RemoveAllListeners();
        gambleResultOkButton.onClick.AddListener(() => OnGambleResultClosed(finalWin, finalMoveForward));
        
        // Show the panel
        gambleResultPanel.SetActive(true);
    }

    private void OnGambleResultClosed(bool win, bool moveForward)
    {
        // Hide the result panel
        gambleResultPanel.SetActive(false);
        
        // Move player if needed
        if (win && moveForward)
        {
            // Move player forward 8 tiles
            currentPlayer.MovePlayer(8);
            GameManager.Instance.isEffectMovement = true;
        }
        
        // Continue game flow
        if (currentTile != null)
        {
            currentTile.ContinueGame();
        }
    }
    
    // Called when a player is selected from the UI
    private void OnPlayerSelected(Player selectedPlayer)
    {
        // Close the selection UI
        playerSelectionPanel.SetActive(false);
        
        // Perform the swap
        CardManager.Instance.SwapWithSpecificPlayer(currentPlayer, selectedPlayer);
        
        // Continue game flow
        if (currentTile != null)
        {
            currentTile.ContinueGame();
        }
    }
    
    // Close the player selection UI without making a selection
    private void ClosePlayerSelectionUI()
    {
        playerSelectionPanel.SetActive(false);
        
        // Fall back to random swap
        CardManager.Instance.ApplyCardEffect(currentCardType, currentPlayer);
        
        // Continue game flow
        if (currentTile != null)
        {
            currentTile.ContinueGame();
        }
    }

    // Close the card panel and apply the effect
    public void CloseCardPanel()
    {
        // Cancel auto-close if running
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        
        // Hide the panel
        cardPanel.SetActive(false);
        
        // Apply the card effect
        if (currentTile != null)
            currentTile.ApplyCardEffect(currentCardType, currentPlayer);
    }
    
    private IEnumerator AutoCloseCard(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseCardPanel();
    }
    
    private void OnDisable()
    {
        // Clean up coroutines
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }
}