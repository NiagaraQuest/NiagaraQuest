﻿using UnityEngine;

public class CardTile : Tile
{
    private bool isProcessingCard = false;

    public override void OnPlayerLands()
    {
        base.OnPlayerLands();
        DrawCard();
    }

    private void DrawCard()
    {
        if (isProcessingCard)
        {
            Debug.LogWarning("⚠️ Already processing a card");
            return;
        }
        
        isProcessingCard = true;
        
        // Get the player who landed on this tile
        Player currentPlayer = FindPlayerOnTile();
        
        if (currentPlayer == null)
        {
            Debug.LogError("❌ No player found on this card tile!");
            isProcessingCard = false;
            return;
        }
        
        // Draw a random card using CardManager
        CardManager cardManager = CardManager.Instance;
        if (cardManager != null)
        {
            int cardType = cardManager.DrawRandomCard();
            string cardName = cardManager.GetCardName(cardType);
            string cardDescription = cardManager.GetCardDescription(cardType);
            
            // Show the card UI
            CardUIManager.Instance.ShowCard(cardName, cardDescription, this, currentPlayer, cardType);
        }
        else
        {
            Debug.LogError("❌ CardManager not found!");
            isProcessingCard = false;
        }
    }
    
    private Player FindPlayerOnTile()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || gameManager.players == null)
            return null;
            
        GameObject tilePrefab = this.gameObject;
        
        // Check which player is on this tile
        foreach (GameObject playerObj in gameManager.players)
        {
            Player player = playerObj.GetComponent<Player>();
            if (player != null)
            {
                GameObject currentWaypoint = player.GetCurrentWaypoint();
                if (currentWaypoint == tilePrefab)
                {
                    return player;
                }
            }
        }
        
        // Fallback to selected player
        return gameManager.selectedPlayer?.GetComponent<Player>();
    }
    
    // This method is called by the CardUIManager
    public void ApplyCardEffect(int cardType, Player player)
    {
        CardManager cardManager = CardManager.Instance;
        if (cardManager != null)
        {
            cardManager.ApplyCardEffect(cardType, player);
        }
        
        // Continue the game
        ContinueGame();
    }
    
    // Continue the game after card effect is applied
    public void ContinueGame()
    {
        Debug.Log("✅ Continuing game after card effect");
        isProcessingCard = false;
        

    }
}