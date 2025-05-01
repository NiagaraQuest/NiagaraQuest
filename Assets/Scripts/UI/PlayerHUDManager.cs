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
    [Tooltip("Top-left corner (usually Pyro)")]
    public PlayerCorner corner1;
    [Tooltip("Top-right corner (usually Hydro)")]
    public PlayerCorner corner2;
    [Tooltip("Bottom-left corner (usually Anemo)")]
    public PlayerCorner corner3;
    [Tooltip("Bottom-right corner (usually Geo)")]
    public PlayerCorner corner4;

    [Header("Animation Settings")]
    public float heartAnimDuration = 0.5f;
    public float heartPulseScale = 1.5f;

    private PlayerCorner[] allCorners;
    private GameManager gameManager;
    private Dictionary<GameObject, PlayerCorner> playerCornerMap = new Dictionary<GameObject, PlayerCorner>();

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

        // Set up the UI for the current game mode
        SetupCorners();
    }

    // Update player information each frame
    void Update()
    {
        UpdateAllCorners();
    }

    // Set up corner panels based on active players
    public void SetupCorners()
    {
        playerCornerMap.Clear();

        // First, hide all corners
        foreach (var corner in allCorners)
        {
            if (corner.cornerPanel != null)
            {
                corner.cornerPanel.SetActive(false);
            }
        }

        // Get active players
        List<GameObject> activePlayers = gameManager.players;
        if (activePlayers == null || activePlayers.Count == 0)
        {
            Debug.LogWarning("PlayerHUDManager: No active players found");
            return;
        }

        // Map player types to corners
        Dictionary<string, PlayerCorner> typeToCorner = new Dictionary<string, PlayerCorner>
        {
            { "Pyro", corner1 },
            { "Hydro", corner2 },
            { "Anemo", corner3 },
            { "Geo", corner4 }
        };

        // Assign players to their appropriate corners
        foreach (GameObject player in activePlayers)
        {
            if (player == null) continue;

            string playerName = player.name;

            // Find matching corner by player type
            foreach (string type in typeToCorner.Keys)
            {
                if (playerName.Contains(type))
                {
                    PlayerCorner corner = typeToCorner[type];
                    if (corner.cornerPanel != null)
                    {
                        // Link player to corner
                        corner.linkedPlayer = player;
                        playerCornerMap[player] = corner;

                        // Activate corner
                        corner.cornerPanel.SetActive(true);

                        // Initial update
                        UpdateCorner(corner);

                        Debug.Log($"PlayerHUDManager: Assigned {playerName} to corner {type}");
                    }
                    break;
                }
            }
        }

        // Log which corners are active
        Debug.Log($"PlayerHUDManager: Active corners: {playerCornerMap.Count}/{allCorners.Length}");
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

        // Update player name
        if (corner.playerNameText != null)
        {
            if (playerScript.playerProfile != null)
            {
                corner.playerNameText.text = playerScript.playerProfile.Username;
            }
            else
            {
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
                    corner.regionText.text = tile.region.ToString();
                }
                else
                {
                    corner.regionText.text = "???";
                }
            }
        }

        // Update ELO text
        if (corner.eloText != null && playerScript.playerProfile != null)
        {
            corner.eloText.text = playerScript.playerProfile.Elo.ToString();
        }

        // Update hearts
        UpdateHearts(corner, playerScript.lives);

        // Highlight current player
        bool isCurrentPlayer = (gameManager.selectedPlayer == corner.linkedPlayer);
        HighlightCorner(corner, isCurrentPlayer);
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
    }

    // Highlight the current player's corner
    private void HighlightCorner(PlayerCorner corner, bool isActive)
    {
        if (corner.cornerPanel != null)
        {
            // Simple highlight - scale up slightly
            corner.cornerPanel.transform.localScale = isActive ?
                new Vector3(1.1f, 1.1f, 1.1f) :
                Vector3.one;
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