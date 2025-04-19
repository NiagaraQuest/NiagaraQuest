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
        "Twisted Paths",
        "Path of Clouds"
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
        "Forced to change your path",
        "All players move 3 tiles forward"
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
                // Instead of random swap, request player selection
                RequestPlayerSelection(player, cardType);
                return; // Exit early - effect will be applied after selection
                
            case 4: // Time Freeze
                player.SkipTurns(1);
                break;
                
            case 5: // Mythic Leap
                gameManager.isEffectMovement = true; 
                player.MovePlayer(25);
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
                player.MovePlayerBack();
                break;
                
            case 9: // Shield Bless
                Debug.Log($"🛡️ {player.gameObject.name} is protected by a shield!");
                ApplyProtectedEffect(player);
                break;
                
            case 10: // Twisted Paths
                Debug.Log($"🔀 {player.gameObject.name} must change its direction");
                player.movementDirection = -1 * player.movementDirection;
                break;
                
            case 11: // Path of Clouds
                gameManager.isEffectMovement = true; 
                MoveAllPlayers(3);
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
    
    // Swap positions with a specific player
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
        
        // Store positions
        string tempPath = currentPlayer.currentPath;
        int tempIndex = currentPlayer.currentWaypointIndex;
        
        // Move players to each other's positions
        currentPlayer.transform.position = otherWaypoint.transform.position;
        currentPlayer.currentPath = otherPlayer.currentPath;
        currentPlayer.currentWaypointIndex = otherPlayer.currentWaypointIndex;
        
        otherPlayer.transform.position = currentWaypoint.transform.position;
        otherPlayer.currentPath = tempPath;
        otherPlayer.currentWaypointIndex = tempIndex;
    }
    
    // Original random swap method (kept as fallback)
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
    
    
    private void MoveAllPlayers(int steps)
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || gameManager.players == null)
            return;
            
        Debug.Log($"☁️ Moving all players forward {steps} tiles");
        foreach (GameObject playerObj in gameManager.players)
        {
            Player player = playerObj.GetComponent<Player>();
            gameManager.isEffectMovement = true; 
            if (player != null)
            {
                player.MovePlayer(steps);
            }
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