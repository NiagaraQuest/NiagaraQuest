using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    
    public delegate void PlayerSelectionRequested(Player currentPlayer, int cardType);
    public static event PlayerSelectionRequested OnPlayerSelectionRequested;
    private List<Player> protectedPlayers = new List<Player>();
    
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
    
    // Card information arrays
    private readonly string[] cardNames = {
        "Life Potion",
        "Poison",
        "The Gambler",
        "Echo of the Past",
        "Time Freeze",
        "Mythic Leap",
        "The Punishment",
        "The Reward",
        "Cursed Steps",
        "Shield Bless",
        "Twisted Paths"
    };
    
    private readonly string[] cardDescriptions = {
        "+1 life",
        "-1 life",
        "Gamble one life",
        "Swap position with another player",
        "Skip your next turn",
        "Move 6 tiles forward",
        "Return to the starting tile",
        "Get an extra turn",
        "Move 6 tiles backward",
        "Protected from losing a life",
        "Forced to change your path"
    };
    
    // Draw a random card and return its type
    public int DrawRandomCard()
    {
        return Random.Range(0, cardNames.Length);
    }
    
    // Get card name by type
    public string GetCardName(int cardType)
    {
        if (cardType >= 0 && cardType < cardNames.Length)
            return cardNames[cardType];
        return "Unknown Card";
    }
    
    // Get card description by type
    public string GetCardDescription(int cardType)
    {
        if (cardType >= 0 && cardType < cardDescriptions.Length)
            return cardDescriptions[cardType];
        return "Unknown effect";
    }
    
    // Apply card effect based on type
    public void ApplyCardEffect(int cardType, Player player)
    {
        GameManager gameManager = GameManager.Instance;
        
        Debug.Log($"🃏 Applying card effect for {player.gameObject.name}: {GetCardName(cardType)}");
        
        switch (cardType)
        {
            case 0: // Life Potion
                player.GainLife();
                break;
                
            case 1: // Poison
                player.LoseLife();
                break;
                
            case 2: // The Gambler
                CardUIManager.Instance.ShowGambleChoice(player);
                break;
                
            case 3: // Echo of the Past (Swap position)
                RequestPlayerSelection(player, cardType);
                return;
                
            case 4: // Time Freeze
                player.SkipTurns(1);
                break;
                
            case 5: // Mythic Leap
                gameManager.isEffectMovement = true; 
                player.MovePlayer(6);
                break;
                
            case 6: // The Punishment
                MoveToStart(player);
                break;
                
            case 7: // The Reward
                Debug.Log($"🎮 {player.gameObject.name} gets an extra turn!");
                gameManager.isEffectMovement = true;
                gameManager.RollDiceAgain(player);
                break;
                
            case 8: // Cursed Steps
                gameManager.isEffectMovement = true;
                player.MovePlayerBack(6);
                break;
                
            case 9: // Shield Bless
                Debug.Log($"🛡️ {player.gameObject.name} is protected by a shield!");
                ApplyProtectedEffect(player);
                break;
                
            case 10: // Twisted Paths
                Debug.Log($"🔀 {player.gameObject.name} must change its direction");
                player.movementDirection = -1 * player.movementDirection;
                break;
        }
    }
    
    // Request player selection UI
    private void RequestPlayerSelection(Player currentPlayer, int cardType)
    {
        if (OnPlayerSelectionRequested != null)
        {
            OnPlayerSelectionRequested(currentPlayer, cardType);
        }
        else
        {
            Debug.LogWarning("No listeners for player selection!");
            // Fallback to random swap if no UI is available
            SwapWithRandomPlayer(currentPlayer);
        }
    }
    
    public void SwapWithSpecificPlayer(Player currentPlayer, Player otherPlayer)
    {
        if (currentPlayer == otherPlayer)
        {
            Debug.LogWarning("Cannot swap with yourself!");
            return;
        }
        
        Debug.Log($"🔄 Swapping positions between {currentPlayer.gameObject.name} and {otherPlayer.gameObject.name}");
        
        // Get current positions
        GameObject currentWaypoint = currentPlayer.GetCurrentWaypoint();
        GameObject otherWaypoint = otherPlayer.GetCurrentWaypoint();
        
        // Store current player's data
        string tempPath = currentPlayer.currentPath;
        int tempIndex = currentPlayer.currentWaypointIndex;
        int tempDirection = currentPlayer.movementDirection;

        
        // Transfer other player's data to current player
        currentPlayer.transform.position = otherWaypoint.transform.position;
        currentPlayer.currentPath = otherPlayer.currentPath;
        currentPlayer.currentWaypointIndex = otherPlayer.currentWaypointIndex;
        currentPlayer.movementDirection = otherPlayer.movementDirection;
        // Transfer stored current player's data to other player
        otherPlayer.transform.position = currentWaypoint.transform.position;
        otherPlayer.currentPath = tempPath;
        otherPlayer.currentWaypointIndex = tempIndex;
        otherPlayer.movementDirection = tempDirection;
        
        Debug.Log($"🔄 Swap completed! Directions: {currentPlayer.gameObject.name}({currentPlayer.movementDirection}) and {otherPlayer.gameObject.name}({otherPlayer.movementDirection})");
    }
    

    private void SwapWithRandomPlayer(Player targetPlayer)
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || gameManager.players.Count <= 1)
            return;
            
        // Find a different player to swap with
        Player otherPlayer = null;
        int randomIndex;
        GameObject playerObj;
        
        do {
            randomIndex = Random.Range(0, gameManager.players.Count);
            playerObj = gameManager.players[randomIndex];
            otherPlayer = playerObj.GetComponent<Player>();
        } while (otherPlayer == targetPlayer);
        
        // Perform the swap
        SwapWithSpecificPlayer(targetPlayer, otherPlayer);
    }
    
    private void MoveToStart(Player player)
    {
        // Move player to start of their path
        GameObject startTile = player.gameBoard.GetTile(player.currentPath, 0);
        if (startTile != null)
        {
            player.transform.position = startTile.transform.position;
            player.currentWaypointIndex = 0;
        }
    }
    

    public void ApplyProtectedEffect(Player player)
    {
        if (player == null)
            return;
        
        Debug.Log($"🛡️ {player.gameObject.name} is now protected from the next wrong answer penalty!");
        
        // Add player to the protected list if not already there
        if (!protectedPlayers.Contains(player))
        {
            protectedPlayers.Add(player);
        }
    }

    public bool IsPlayerProtected(Player player)
    {
        return protectedPlayers.Contains(player);
    }

    // Use protection if available (call this when player answers incorrectly)
    public bool UseProtectionIfAvailable(Player player)
    {
        if (protectedPlayers.Contains(player))
        {
            Debug.Log($"🛡️ {player.gameObject.name}'s protection activated! Penalty avoided.");
            protectedPlayers.Remove(player);
            return true;
        }
        return false;
    }
}