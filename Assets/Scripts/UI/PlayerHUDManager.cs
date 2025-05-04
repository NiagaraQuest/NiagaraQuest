using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Main controller script for player HUDs - place on your UI manager GameObject
public class PlayerHUDManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerCorner
    {
        public GameObject cornerPanel;
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI regionText;
        public TextMeshProUGUI eloText;
        public Image[] heartImages = new Image[4];
        public Color activeHeartColor = Color.red;
        public Color inactiveHeartColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [HideInInspector] public GameObject linkedPlayer;
    }

    [Header("Corner References")]
    [Tooltip("Top-left corner (Anemo)")]
    public PlayerCorner corner1;
    [Tooltip("Top-right corner (Hydro)")]
    public PlayerCorner corner2;
    [Tooltip("Bottom-left corner (Geo)")]
    public PlayerCorner corner3;
    [Tooltip("Bottom-right corner (Pyro)")]
    public PlayerCorner corner4;

    [Header("Animation Settings")]
    public float heartAnimDuration = 0.5f;
    public float heartPulseScale = 1.5f;
    
    [Header("Setup Settings")]
    [Tooltip("Delay in seconds before setting up HUD corners")]
    public float setupDelay = 0.5f;

    private PlayerCorner[] allCorners;
    private GameManager gameManager;
    private Dictionary<GameObject, PlayerCorner> playerCornerMap = new Dictionary<GameObject, PlayerCorner>();
    private bool cornersSetup = false;

    void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("PlayerHUDManager: GameManager.Instance not found");
            return;
        }

        // Set up corners array
        allCorners = new PlayerCorner[] { corner1, corner2, corner3, corner4 };

        // Hide all HUD corners at the start
        HideAllCorners();
        
        // Wait for GameManager to set up players from PlayerPrefs
        StartCoroutine(SetupCornersDelayed());
    }

    // Hide all corner panels
    private void HideAllCorners()
    {
        foreach (var corner in allCorners)
        {
            if (corner != null && corner.cornerPanel != null)
            {
                corner.cornerPanel.SetActive(false);
            }
        }
        Debug.Log("PlayerHUDManager: All HUD corners hidden at start");
    }

    private IEnumerator SetupCornersDelayed()
    {
        // Wait for GameManager to initialize players from PlayerPrefs
        yield return new WaitForSeconds(setupDelay);

        // Set up the UI for the current game mode
        SetupCorners();
        cornersSetup = true;
    }

    // Update player information each frame (only after setup is complete)
    void Update()
    {
        if (cornersSetup)
        {
            UpdateAllCorners();
        }
    }

    // Set up corner panels based on active players
    public void SetupCorners()
    {
        playerCornerMap.Clear();

        // Get active players from GameManager
        List<GameObject> activePlayers = gameManager.players;
        if (activePlayers == null || activePlayers.Count == 0)
        {
            Debug.LogWarning("PlayerHUDManager: No active players found in GameManager");
            return;
        }


        Dictionary<string, PlayerCorner> typeToCorner = new Dictionary<string, PlayerCorner>
        {
            { "AnemoPlayer", corner1 },  // Top Left
            { "HydroPlayer", corner2 },  // Top Right
            { "GeoPlayer", corner3 },    // Bottom Left
            { "PyroPlayer", corner4 }    // Bottom Right
        };

        // Check which corners should be active based on active players
        foreach (GameObject player in activePlayers)
        {
            if (player == null) continue;

            string playerName = player.name;

            if (typeToCorner.TryGetValue(playerName, out PlayerCorner corner))
            {
                if (corner.cornerPanel != null)
                {
                    // Link player to corner
                    corner.linkedPlayer = player;
                    playerCornerMap[player] = corner;

                    // Activate corner
                    corner.cornerPanel.SetActive(true);

                    // Initial update
                    UpdateCorner(corner);

                    Debug.Log($"PlayerHUDManager: Assigned {playerName} to corner and activated HUD");
                }
            }
        }

        // Ensure corners match the game mode
        AdjustCornersForGameMode();

        // Log which corners are active
        Debug.Log($"PlayerHUDManager: Active corners: {playerCornerMap.Count}/{allCorners.Length} for game mode {gameManager.currentGameMode}");
    }

    // Adjust corner positions based on game mode if needed
    private void AdjustCornersForGameMode()
    {
        switch (gameManager.currentGameMode)
        {
            case GameManager.GameMode.TwoPlayers:
                // Two players - Check if we need to redistribute corners
                if (playerCornerMap.Count == 2)
                {
                    // Ensure we're using the top corners for better visibility
                    bool hasTopLeft = corner1.linkedPlayer != null;
                    bool hasTopRight = corner2.linkedPlayer != null;
                    bool hasBottomLeft = corner3.linkedPlayer != null;
                    bool hasBottomRight = corner4.linkedPlayer != null;

                    // If we have only bottom corners active, move them to top
                    if (!hasTopLeft && !hasTopRight && (hasBottomLeft || hasBottomRight))
                    {
                        // In a real implementation, you might want to adjust corner positions
                        // For now, we'll just log it
                        Debug.Log("Two player mode with only bottom corners - consider rearranging UI");
                    }
                }
                break;

            case GameManager.GameMode.ThreePlayers:
                // Three players - potentially adjust corner positions
                Debug.Log("Three player mode - corners set up according to active players");
                break;

            case GameManager.GameMode.FourPlayers:
                // All corners should be active
                Debug.Log("Four player mode - all corners should be active");
                break;
        }
    }

    // Update all corner displays
    private void UpdateAllCorners()
    {
        foreach (var corner in allCorners)
        {
            if (corner.cornerPanel != null && corner.cornerPanel.activeSelf && corner.linkedPlayer != null)
            {
                UpdateCorner(corner);
            }
        }
    }

    // Update a specific corner's display
    private void UpdateCorner(PlayerCorner corner)
    {
        if (corner.linkedPlayer == null) return;

        Player playerScript = corner.linkedPlayer.GetComponent<Player>();
        if (playerScript == null) return;

        // Update player name - use profile from PlayerPrefs if available
        if (corner.playerNameText != null)
        {
            if (playerScript.playerProfile != null)
            {
                // Use profile name from the assigned profile
                corner.playerNameText.text = playerScript.playerProfile.Username;
            }
            else
            {
                // Fallback to element type if profile not available
                corner.playerNameText.text = corner.linkedPlayer.name.Replace("Player", "");
            }
        }

        // Update region text
        if (corner.regionText != null)
        {
            GameObject currentWaypoint = playerScript.GetCurrentWaypoint();
            if (currentWaypoint != null)
            {
                Tile tile = currentWaypoint.GetComponent<Tile>();
                if (tile != null)
                {
                    // Include the waypoint index next to the region name
                    corner.regionText.text = $"{tile.region}:{playerScript.currentWaypointIndex}";
                }
                else
                {
                    corner.regionText.text = $"???:{playerScript.currentWaypointIndex}";
                }
            }
        }

        // Update ELO text - use profile ELO from PlayerPrefs
        if (corner.eloText != null && playerScript.playerProfile != null)
        {
            corner.eloText.text = playerScript.playerProfile.Elo.ToString();

            // Optional: Format ELO with label
            // corner.eloText.text = $"ELO: {playerScript.playerProfile.Elo}";
        }

        // Update hearts based on current lives
        UpdateHearts(corner, playerScript.lives);

    }
    

    // Update heart images based on current lives
    private void UpdateHearts(PlayerCorner corner, int currentLives)
    {
        if (corner.heartImages == null) return;

        for (int i = 0; i < corner.heartImages.Length; i++)
        {
            if (corner.heartImages[i] != null)
            {
                bool isActive = i < currentLives;
                corner.heartImages[i].color = isActive ? corner.activeHeartColor : corner.inactiveHeartColor;
            }
        }
        
        // Handle 4-player mode where we show one less heart
        if (gameManager.currentGameMode == GameManager.GameMode.FourPlayers)
        {
            // Make the last heart invisible in 4-player mode (since max lives is 3)
            if (corner.heartImages.Length > 3 && corner.heartImages[3] != null)
            {
                corner.heartImages[3].color = new Color(0, 0, 0, 0);
            }
        }
    }

    // Called when a player loses/gains a life
    public void OnPlayerLifeChanged(GameObject player, int oldValue, int newValue)
    {
        if (playerCornerMap.TryGetValue(player, out PlayerCorner corner))
        {
            // Update hearts with animation
            UpdateHearts(corner, newValue);

            // Animate hearts that changed
            int startIndex = Mathf.Min(oldValue, newValue);
            int endIndex = Mathf.Max(oldValue, newValue);
            bool isGaining = newValue > oldValue;

            for (int i = startIndex; i < endIndex && i < corner.heartImages.Length; i++)
            {
                if (corner.heartImages[i] != null)
                {
                    StartCoroutine(AnimateHeart(corner.heartImages[i], isGaining));
                }
            }
        }
    }

    // Animate heart change
    private IEnumerator AnimateHeart(Image heartImage, bool isGaining)
    {
        float duration = heartAnimDuration;
        float time = 0;
        Vector3 originalScale = heartImage.transform.localScale;
        Vector3 pulseScale = originalScale * heartPulseScale;
        Color originalColor = heartImage.color;
        Color flashColor = isGaining ? Color.white : Color.black;

        while (time < duration)
        {
            float t = time / duration;

            // First half: pulse out
            if (t < 0.5f)
            {
                float phase = t * 2;
                heartImage.transform.localScale = Vector3.Lerp(originalScale, pulseScale, phase);
                heartImage.color = Color.Lerp(originalColor, flashColor, phase);
            }
            // Second half: pulse in
            else
            {
                float phase = (t - 0.5f) * 2;
                heartImage.transform.localScale = Vector3.Lerp(pulseScale, originalScale, phase);
                heartImage.color = Color.Lerp(flashColor, originalColor, phase);
            }

            time += Time.deltaTime;
            yield return null;
        }

        // Reset to final state
        heartImage.transform.localScale = originalScale;
        heartImage.color = originalColor;
    }
}