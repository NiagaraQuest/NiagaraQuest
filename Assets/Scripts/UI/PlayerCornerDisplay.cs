using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(HeartDisplayController))]
public class PlayerCornerDisplay : MonoBehaviour
{
    [Header("Player Information")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI regionText;
    public TextMeshProUGUI eloText;

    [Header("References")]
    public Player targetPlayer;

    private HeartDisplayController heartController;
    private Tile.Region lastRegion = Tile.Region.None;
    private int lastElo = -1;
    private bool isInitialized = false;
    private float updateInterval = 0.5f; // Update display every half second instead of every frame
    private float nextUpdateTime = 0f;

    private void Awake()
    {
        // Get the heart controller component
        heartController = GetComponent<HeartDisplayController>();
    }

    private void Start()
    {
        // Wait for GameManager to initialize
        Invoke("InitializeDisplay", 0.5f);
    }

    private void InitializeDisplay()
    {
        // Only initialize if GameManager is available
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager not found, waiting...");
            Invoke("InitializeDisplay", 1.0f);
            return;
        }

        // If no player is assigned, try to find the associated player
        if (targetPlayer == null)
        {
            FindAssociatedPlayer();
        }

        // Make sure the heart controller uses the same player
        if (heartController != null && targetPlayer != null)
        {
            heartController.targetPlayer = targetPlayer;
        }

        // Set initial player name if available
        if (targetPlayer != null && playerNameText != null)
        {
            string playerType = DeterminePlayerType(targetPlayer);
            playerNameText.text = playerType;
        }

        isInitialized = true;
        nextUpdateTime = Time.time + updateInterval;
    }

    private void Update()
    {
        // Skip if not initialized
        if (!isInitialized) return;

        // Skip if no GameManager
        if (GameManager.Instance == null) return;

        // Skip during dice rolls and player movement
        if (GameManager.Instance.hasDiceBeenRolledThisTurn &&
            !GameManager.Instance.isEffectMovement)
        {
            return;
        }

        // Check if any player is currently moving
        foreach (GameObject playerObj in GameManager.Instance.players)
        {
            if (playerObj == null) continue;

            Player player = playerObj.GetComponent<Player>();
            if (player != null && player.isMoving)
            {
                return; // Skip update if any player is moving
            }
        }

        // Only update at specific intervals to reduce performance impact
        if (Time.time >= nextUpdateTime)
        {
            // Update display with robust error handling
            SafeUpdateDisplay();
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    private void FindAssociatedPlayer()
    {
        if (GameManager.Instance == null || GameManager.Instance.players == null) return;

        // Try to determine which corner we are by name
        string cornerName = gameObject.name.ToLower();
        int cornerIndex = -1;

        if (cornerName.Contains("topleft") || cornerName.Contains("top left"))
            cornerIndex = 0;
        else if (cornerName.Contains("topright") || cornerName.Contains("top right"))
            cornerIndex = 1;
        else if (cornerName.Contains("bottomleft") || cornerName.Contains("bottom left"))
            cornerIndex = 2;
        else if (cornerName.Contains("bottomright") || cornerName.Contains("bottom right"))
            cornerIndex = 3;

        // Get player based on corner index if valid
        if (cornerIndex >= 0 && cornerIndex < GameManager.Instance.players.Count)
        {
            GameObject playerObj = GameManager.Instance.players[cornerIndex];
            if (playerObj != null)
            {
                targetPlayer = playerObj.GetComponent<Player>();
                if (targetPlayer != null)
                {
                    Debug.Log($"✅ PlayerCornerDisplay associated with {playerObj.name} based on corner position");
                    return;
                }
            }
        }

        // If still not found, just take the first available player (fallback)
        foreach (GameObject playerObj in GameManager.Instance.players)
        {
            if (playerObj != null)
            {
                Player player = playerObj.GetComponent<Player>();
                if (player != null)
                {
                    targetPlayer = player;
                    Debug.Log($"⚠️ PlayerCornerDisplay using fallback player: {playerObj.name}");
                    return;
                }
            }
        }
    }

    private void SafeUpdateDisplay()
    {
        try
        {
            UpdateDisplay();
        }
        catch (System.Exception ex)
        {
            // Just log and continue
            Debug.LogWarning($"Error updating display: {ex.Message}");
        }
    }

    public void UpdateDisplay()
    {
        if (targetPlayer == null) return;

        try
        {
            // Skip update if player is moving
            if (targetPlayer.isMoving)
                return;

            // Update region text if player has a current waypoint
            if (regionText != null)
            {
                try
                {
                    GameObject currentWaypoint = null;

                    // Use a safe getter for the current waypoint
                    try
                    {
                        currentWaypoint = targetPlayer.GetCurrentWaypoint();
                    }
                    catch (System.Exception)
                    {
                        // Ignore errors from GetCurrentWaypoint
                    }

                    if (currentWaypoint != null)
                    {
                        Tile tile = currentWaypoint.GetComponent<Tile>();
                        if (tile != null && tile.region != lastRegion)
                        {
                            lastRegion = tile.region;
                            regionText.text = tile.region.ToString();
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Error updating region text: {e.Message}");
                }
            }

            // Update ELO text if player has a profile
            if (eloText != null && targetPlayer.playerProfile != null)
            {
                try
                {
                    int currentElo = targetPlayer.playerProfile.Elo;
                    if (currentElo != lastElo)
                    {
                        lastElo = currentElo;
                        eloText.text = currentElo.ToString();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Error updating ELO text: {e.Message}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error in PlayerCornerDisplay.UpdateDisplay: {ex.Message}");
        }
    }

    private string DeterminePlayerType(Player player)
    {
        if (player == null) return "Unknown";

        try
        {
            if (player is PyroPlayer) return "Pyro";
            if (player is HydroPlayer) return "Hydro";
            if (player is AnemoPlayer) return "Anemo";
            if (player is GeoPlayer) return "Geo";
            return player.gameObject.name.Replace("Player", "");
        }
        catch
        {
            return player.gameObject.name.Replace("Player", "");
        }
    }

    // Public method to manually set the player for this corner
    public void SetPlayer(Player player)
    {
        if (player == null) return;

        targetPlayer = player;

        // Update the hearts controller too
        if (heartController != null)
        {
            heartController.SetPlayer(player);
        }

        // Update player name immediately
        if (playerNameText != null)
        {
            string playerType = DeterminePlayerType(player);
            playerNameText.text = playerType;
        }

        // Reset tracking variables
        lastRegion = Tile.Region.None;
        lastElo = -1;
    }
}