using UnityEngine;

// Attach this to your UI manager GameObject to monitor player life changes
public class PlayerMonitor : MonoBehaviour
{
    [Tooltip("Reference to the HUD manager")]
    public PlayerHUDManager hudManager;

    private GameManager gameManager;

    // Stores previous player lives value
    private class PlayerLifeCache
    {
        public int previousLives;
    }

    // Dictionary to track each player's previous life value
    private System.Collections.Generic.Dictionary<GameObject, PlayerLifeCache> playerCache =
        new System.Collections.Generic.Dictionary<GameObject, PlayerLifeCache>();

    void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("PlayerMonitor: GameManager.Instance not found");
            return;
        }

        // Find HUD manager if not assigned
        if (hudManager == null)
        {
            hudManager = FindObjectOfType<PlayerHUDManager>();
            if (hudManager == null)
            {
                Debug.LogError("PlayerMonitor: PlayerHUDManager not found");
            }
        }

        // Initialize tracking for all players
        InitializeTracking();
    }

    // Set up life tracking for all active players
    private void InitializeTracking()
    {
        if (gameManager.players == null) return;

        // Clear existing cache
        playerCache.Clear();

        foreach (var player in gameManager.players)
        {
            if (player == null) continue;

            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                // Initialize tracking for this player
                PlayerLifeCache cache = new PlayerLifeCache();
                cache.previousLives = playerScript.lives;
                playerCache[player] = cache;

                Debug.Log($"PlayerMonitor: Started tracking lives for {player.name} (current: {playerScript.lives})");
            }
        }
    }

    void Update()
    {
        // Check for players that might have been added after initialization
        if (gameManager.players != null)
        {
            foreach (var player in gameManager.players)
            {
                if (player != null && !playerCache.ContainsKey(player))
                {
                    Player playerScript = player.GetComponent<Player>();
                    if (playerScript != null)
                    {
                        // Add new player to tracking
                        PlayerLifeCache cache = new PlayerLifeCache();
                        cache.previousLives = playerScript.lives;
                        playerCache[player] = cache;

                        Debug.Log($"PlayerMonitor: Added tracking for {player.name} (current: {playerScript.lives})");
                    }
                }
            }
        }

        // Check for life changes in all tracked players
        CheckForLifeChanges();
    }

    // Check if any players' lives have changed
    private void CheckForLifeChanges()
    {
        if (gameManager.players == null || hudManager == null) return;

        foreach (var player in gameManager.players)
        {
            if (player == null) continue;

            Player playerScript = player.GetComponent<Player>();
            if (playerScript == null) continue;

            // Get or create cache
            if (!playerCache.TryGetValue(player, out PlayerLifeCache cache))
            {
                cache = new PlayerLifeCache();
                cache.previousLives = playerScript.lives;
                playerCache[player] = cache;
                continue;
            }

            // Check for life change
            int currentLives = playerScript.lives;
            if (currentLives != cache.previousLives)
            {
                // Life value has changed - notify HUD manager
                hudManager.OnPlayerLifeChanged(player, cache.previousLives, currentLives);

                // Update stored value
                cache.previousLives = currentLives;
            }
        }
    }

    // Call this when game mode changes
    public void OnGameModeChanged()
    {
        // Clear cache
        playerCache.Clear();

        // Refresh HUD setup
        if (hudManager != null)
        {
            hudManager.SetupCorners();
        }

        // Re-initialize tracking
        InitializeTracking();
    }
}