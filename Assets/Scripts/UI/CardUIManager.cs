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
    public float autoCloseTime = 3f; // Auto close after 3 seconds if desired

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Hide panels at start
        cardPanel.SetActive(false);
        playerSelectionPanel.SetActive(false);
        
        // Add listener to the Continue button
        continueButton.onClick.AddListener(CloseCardPanel);
        
        // Subscribe to the player selection event
        CardManager.OnPlayerSelectionRequested += ShowPlayerSelectionUI;
    }

    private void OnDestroy()
    {
        // Unsubscribe from event
        CardManager.OnPlayerSelectionRequested -= ShowPlayerSelectionUI;
    }

    // Show the card UI with the given card information
    public void ShowCard(string title, string description, CardTile tile, Player player, int cardType)
    {
        // Cancel any existing auto-close routine
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        
        // Store references
        currentTile = tile;
        currentPlayer = player;
        currentCardType = cardType;
        
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