using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerHUDManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerHUDCorner
    {
        public GameObject cornerRoot;
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI regionText;
        public TextMeshProUGUI eloText;
        public List<Image> heartImages;
        public Player associatedPlayer;
    }

    [Header("Player HUD Corners")]
    public List<PlayerHUDCorner> playerCorners = new List<PlayerHUDCorner>();

    [Header("Heart Visuals")]
    public Color activeHeartColor = Color.red;
    public Color inactiveHeartColor = Color.gray;
    public bool useGrayscaleInstead = true; // If true, use grayscale instead of changing color

    private void Start()
    {
        // Initialize HUD based on active players
        InitializeHUD();
    }

    private void Update()
    {
        // Update player HUD info (region, lives, etc.) each frame
        UpdatePlayerHUDs();
    }

    private void InitializeHUD()
    {
        // Get all active players from GameManager
        if (GameManager.Instance != null && GameManager.Instance.players != null)
        {
            if (GameManager.Instance.players.Count > playerCorners.Count)
            {
                Debug.LogWarning("⚠️ More players than available HUD corners! Some players won't have HUD displays.");
            }

            // Associate each player with a corner
            for (int i = 0; i < Mathf.Min(GameManager.Instance.players.Count, playerCorners.Count); i++)
            {
                GameObject playerObj = GameManager.Instance.players[i];
                if (playerObj != null)
                {
                    Player player = playerObj.GetComponent<Player>();
                    if (player != null)
                    {
                        playerCorners[i].associatedPlayer = player;

                        // Set initial player name
                        if (playerCorners[i].playerNameText != null)
                        {
                            playerCorners[i].playerNameText.text = playerObj.name.Replace("Player", "");
                        }

                        Debug.Log($"✅ Associated {playerObj.name} with HUD corner {i + 1}");
                    }
                }
            }
        }
        else
        {
            Debug.LogError("❌ GameManager not found or no players available!");
        }
    }

    private void UpdatePlayerHUDs()
    {
        foreach (PlayerHUDCorner corner in playerCorners)
        {
            if (corner.associatedPlayer == null || corner.cornerRoot == null)
                continue;

            // Update region text
            if (corner.regionText != null)
            {
                // Get current tile from player
                GameObject currentWaypoint = corner.associatedPlayer.GetCurrentWaypoint();
                if (currentWaypoint != null)
                {
                    Tile tile = currentWaypoint.GetComponent<Tile>();
                    if (tile != null)
                    {
                        corner.regionText.text = tile.region.ToString();
                    }
                }
            }

            // Update ELO if the player has a profile
            if (corner.eloText != null && corner.associatedPlayer.playerProfile != null)
            {
                corner.eloText.text = "ELO: " + corner.associatedPlayer.playerProfile.Elo.ToString();
            }

            // Update hearts based on current lives
            UpdateHearts(corner);
        }
    }

    private void UpdateHearts(PlayerHUDCorner corner)
    {
        if (corner.heartImages.Count == 0 || corner.associatedPlayer == null)
            return;

        int maxLives = GameManager.Instance.maxLives;
        int currentLives = corner.associatedPlayer.lives;

        // Make sure we don't try to update more hearts than we have images for
        int heartsToUpdate = Mathf.Min(maxLives, corner.heartImages.Count);

        for (int i = 0; i < heartsToUpdate; i++)
        {
            Image heartImage = corner.heartImages[i];
            if (heartImage != null)
            {
                if (i < currentLives)
                {
                    // Heart is active
                    if (useGrayscaleInstead)
                    {
                        // Remove grayscale effect
                        heartImage.color = Color.white; // Normal color (no tint)
                    }
                    else
                    {
                        // Use active color
                        heartImage.color = activeHeartColor;
                    }
                }
                else
                {
                    // Heart is inactive
                    if (useGrayscaleInstead)
                    {
                        // Apply grayscale by using a gray tint
                        heartImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Gray with transparency
                    }
                    else
                    {
                        // Use inactive color
                        heartImage.color = inactiveHeartColor;
                    }
                }
            }
        }
    }
}