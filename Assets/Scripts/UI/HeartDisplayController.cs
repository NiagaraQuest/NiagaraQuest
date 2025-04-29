using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeartDisplayController : MonoBehaviour
{
    [Header("Hearts Setup")]
    public List<Image> heartImages = new List<Image>();
    public Color activeColor = Color.white; // Default full color (no tint)
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Default grayscale with transparency

    [Header("References")]
    public Player targetPlayer;

    private int lastMaxLives = -1;
    private int lastPlayerLives = -1;
    private bool isInitialized = false;
    private float updateInterval = 0.5f; // Update interval in seconds
    private float nextUpdateTime = 0f;

    private void Awake()
    {
        // Auto-find heart images if not set
        if (heartImages.Count == 0)
        {
            Image[] foundHearts = GetComponentsInChildren<Image>();
            foreach (Image img in foundHearts)
            {
                // Only add images with "Heart" in the name to avoid adding other UI elements
                if (img.gameObject.name.Contains("Heart"))
                {
                    heartImages.Add(img);
                }
            }
        }
    }

    private void Start()
    {
        // Wait for GameManager to be fully initialized
        Invoke("InitializeController", 0.5f);
    }

    private void InitializeController()
    {
        // Only initialize if GameManager is available
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager not found, waiting...");
            Invoke("InitializeController", 1.0f);
            return;
        }

        // If no player is assigned, try to find one
        if (targetPlayer == null)
        {
            FindAssociatedPlayer();
        }

        // Set flag for initialization
        isInitialized = true;
        nextUpdateTime = Time.time;

        // Initial updates
        UpdateHeartVisibility();
        SafeUpdateHearts();
    }

    private void Update()
    {
        // Skip if not initialized
        if (!isInitialized) return;

        // Skip if no GameManager
        if (GameManager.Instance == null) return;

        // Skip updates during dice rolls and player movement
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

        // Only update at intervals
        if (Time.time >= nextUpdateTime)
        {
            try
            {
                // Only check for changes in max lives
                if (GameManager.Instance.maxLives != lastMaxLives)
                {
                    lastMaxLives = GameManager.Instance.maxLives;
                    UpdateHeartVisibility();
                }

                // Only update hearts when lives change or first time
                if (targetPlayer != null && (targetPlayer.lives != lastPlayerLives || lastPlayerLives == -1))
                {
                    lastPlayerLives = targetPlayer.lives;
                    SafeUpdateHearts();
                }

                nextUpdateTime = Time.time + updateInterval;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error in HeartDisplayController: {e.Message}");
            }
        }
    }

    private void FindAssociatedPlayer()
    {
        if (GameManager.Instance == null || GameManager.Instance.players == null) return;

        // Try to use the gameObject's name to identify which corner we're in
        string cornerName = gameObject.name.ToLower();
        int playerIndex = -1;

        if (cornerName.Contains("topleft")) playerIndex = 0;
        else if (cornerName.Contains("topright")) playerIndex = 1;
        else if (cornerName.Contains("bottomleft")) playerIndex = 2;
        else if (cornerName.Contains("bottomright")) playerIndex = 3;

        // Get player based on corner position
        if (playerIndex >= 0 && playerIndex < GameManager.Instance.players.Count)
        {
            GameObject playerObj = GameManager.Instance.players[playerIndex];
            if (playerObj != null)
            {
                targetPlayer = playerObj.GetComponent<Player>();
                if (targetPlayer != null)
                {
                    Debug.Log($"✅ HeartDisplay associated with {playerObj.name}");
                    return;
                }
            }
        }

        // Fallback: just try to find any valid player
        foreach (GameObject playerObj in GameManager.Instance.players)
        {
            if (playerObj == null) continue;

            Player player = playerObj.GetComponent<Player>();
            if (player != null)
            {
                targetPlayer = player;
                Debug.Log($"⚠️ HeartDisplay fallback to player: {playerObj.name}");
                return;
            }
        }

        Debug.LogWarning("⚠️ No player found for this heart display!");
    }

    private void SafeUpdateHearts()
    {
        try
        {
            UpdateHearts();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Error updating hearts: {ex.Message}");
        }
    }

    public void UpdateHearts()
    {
        if (targetPlayer == null || heartImages.Count == 0)
            return;

        try
        {
            int currentLives = targetPlayer.lives;
            int maxLives = GameManager.Instance != null ? GameManager.Instance.maxLives : heartImages.Count;

            // Update each heart image based on current lives
            for (int i = 0; i < heartImages.Count; i++)
            {
                if (heartImages[i] != null && heartImages[i].gameObject.activeSelf)
                {
                    // If this heart index is less than current lives, it should be active
                    bool isActive = i < currentLives;

                    // Set the appropriate color
                    heartImages[i].color = isActive ? activeColor : inactiveColor;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating hearts: {e.Message}");
        }
    }

    // Update heart visibility based on the game mode
    private void UpdateHeartVisibility()
    {
        if (GameManager.Instance == null || heartImages.Count < 4)
            return;

        try
        {
            // Hide the fourth heart if max lives is 3 (4-player mode)
            if (heartImages.Count >= 4 && heartImages[3] != null)
            {
                bool showFourthHeart = (GameManager.Instance.maxLives > 3);
                heartImages[3].gameObject.SetActive(showFourthHeart);

                Debug.Log($"Fourth heart visibility set to: {showFourthHeart} (Max Lives: {GameManager.Instance.maxLives})");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating heart visibility: {e.Message}");
        }
    }

    // Public method to manually set the associated player
    public void SetPlayer(Player player)
    {
        if (player == null) return;

        targetPlayer = player;
        lastPlayerLives = -1; // Force update

        if (isInitialized)
        {
            UpdateHeartVisibility();
            SafeUpdateHearts();
        }
    }
}