using UnityEngine;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Player Corner Displays")]
    public PlayerCornerDisplay topLeftCorner;
    public PlayerCornerDisplay topRightCorner;
    public PlayerCornerDisplay bottomLeftCorner;
    public PlayerCornerDisplay bottomRightCorner;
    private List<PlayerCornerDisplay> allCorners = new List<PlayerCornerDisplay>();

    [Header("Additional UI Elements")]
    public GameObject pauseButton;
    public GameObject diceButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize the list of all corners
        InitializeCornersList();
    }

    private void Start()
    {
        // Wait a frame to make sure GameManager is fully initialized
        Invoke("SetupPlayerCorners", 0.1f);
    }

    private void InitializeCornersList()
    {
        allCorners.Clear();

        if (topLeftCorner != null) allCorners.Add(topLeftCorner);
        if (topRightCorner != null) allCorners.Add(topRightCorner);
        if (bottomLeftCorner != null) allCorners.Add(bottomLeftCorner);
        if (bottomRightCorner != null) allCorners.Add(bottomRightCorner);

        // Try to find corners automatically if they're not assigned
        if (allCorners.Count == 0)
        {
            Debug.LogWarning("⚠️ No corners assigned in HUDManager. Trying to find them automatically...");
            PlayerCornerDisplay[] foundCorners = GetComponentsInChildren<PlayerCornerDisplay>();

            if (foundCorners.Length > 0)
            {
                allCorners.AddRange(foundCorners);
                Debug.Log($"✅ Found {foundCorners.Length} player corner displays automatically.");
            }
            else
            {
                Debug.LogError("❌ No PlayerCornerDisplay components found in children!");
            }
        }
    }

    private void SetupPlayerCorners()
    {
        // Make sure GameManager exists
        if (GameManager.Instance == null || GameManager.Instance.players == null)
        {
            Debug.LogError("❌ GameManager not initialized or no players available!");
            return;
        }

        List<GameObject> players = GameManager.Instance.players;

        // Make sure we have players to assign
        if (players.Count == 0)
        {
            Debug.LogError("❌ No players found in GameManager!");
            return;
        }

        // Assign players to corners based on available corners and players
        for (int i = 0; i < Mathf.Min(players.Count, allCorners.Count); i++)
        {
            GameObject playerObj = players[i];
            if (playerObj != null)
            {
                Player player = playerObj.GetComponent<Player>();
                if (player != null && allCorners[i] != null)
                {
                    allCorners[i].SetPlayer(player);
                    Debug.Log($"✅ Assigned {playerObj.name} to corner {i + 1}");
                }
            }
        }

        // Hide unused corners if there are fewer players than corners
        for (int i = players.Count; i < allCorners.Count; i++)
        {
            if (allCorners[i] != null)
            {
                allCorners[i].gameObject.SetActive(false);
                Debug.Log($"✅ Disabled unused corner {i + 1}");
            }
        }
    }

    // Called when a player's lives change
    public void UpdatePlayerLives(Player player)
    {
        foreach (PlayerCornerDisplay corner in allCorners)
        {
            if (corner != null && corner.targetPlayer == player)
            {
                // The corner's HeartDisplayController will handle the update automatically
                // but we can force a refresh just to be safe
                HeartDisplayController heartController = corner.GetComponent<HeartDisplayController>();
                if (heartController != null)
                {
                    heartController.UpdateHearts();
                }
                break;
            }
        }
    }

    // Called when a player's ELO changes
    public void UpdatePlayerELO(Player player)
    {
        foreach (PlayerCornerDisplay corner in allCorners)
        {
            if (corner != null && corner.targetPlayer == player)
            {
                // Force an update of the display
                corner.UpdateDisplay();
                break;
            }
        }
    }

    // Method to show/hide the pause button
    public void SetPauseButtonVisible(bool visible)
    {
        if (pauseButton != null)
        {
            pauseButton.SetActive(visible);
        }
    }

    // Method to show/hide the dice button
    public void SetDiceButtonVisible(bool visible)
    {
        if (diceButton != null)
        {
            diceButton.SetActive(visible);
        }
    }
}