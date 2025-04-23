using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class GameEndManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string menuSceneName = "MenuScene";
    public string gameSceneName = "GameScene";

    [Header("Profile Management")]
    public bool preservePlayerProfiles = true;
    
    // Reference to the GameManager
    private GameManager gameManager;
    
    // Static dictionary to store profiles between game sessions
    private static Dictionary<string, Profile> savedProfiles = new Dictionary<string, Profile>();
    
    private void Awake()
    {
        // Get GameManager instance
        if (GameManager.Instance != null)
        {
            gameManager = GameManager.Instance;
        }
        else
        {
            Debug.LogError("❌ GameManager not found! GameEndManager requires GameManager to work properly.");
        }
    }
    
    private void Start()
    {
        // Check if there are saved profiles at start
        if (HasSavedProfiles() && gameManager != null)
        {
            Debug.Log("🔄 GameEndManager detected saved profiles from previous game");
            LoadSavedProfiles(gameManager);
        }
    }
    
    // Save current player profiles to static dictionary for persistence
    private void SaveCurrentProfiles()
    {
        if (!preservePlayerProfiles || gameManager == null)
            return;
            
        savedProfiles.Clear();
        int savedCount = 0;
        
        try
        {
            foreach (GameObject playerObj in gameManager.players)
            {
                if (playerObj == null) continue;
                
                Player player = playerObj.GetComponent<Player>();
                if (player != null && player.playerProfile != null)
                {
                    string playerKey = playerObj.name;
                    savedProfiles[playerKey] = player.playerProfile;
                    savedCount++;
                    Debug.Log($"💾 Saved profile for {playerKey}: {player.playerProfile.Username} (ID: {player.playerProfile.Id})");
                }
            }
            
            Debug.Log($"💾 Successfully saved {savedCount} player profiles for next game");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error saving profiles: {ex.Message}");
            // Clear savedProfiles if there was an error to avoid partial data
            savedProfiles.Clear();
        }
    }
    
    // Load saved player profiles from the static dictionary
    public static void LoadSavedProfiles(GameManager newGameManager)
    {
        if (savedProfiles == null || savedProfiles.Count == 0)
        {
            Debug.Log("ℹ️ No saved profiles found, using default profiles");
            return;
        }
        
        int loadedCount = 0;
        try
        {
            foreach (GameObject playerObj in newGameManager.players)
            {
                if (playerObj == null) continue;
                
                string playerKey = playerObj.name;
                if (savedProfiles.TryGetValue(playerKey, out Profile savedProfile))
                {
                    Player player = playerObj.GetComponent<Player>();
                    if (player != null)
                    {
                        player.playerProfile = savedProfile;
                        player.debugProfileName = savedProfile.Username;
                        loadedCount++;
                        Debug.Log($"✅ Loaded saved profile for {playerKey}: {savedProfile.Username} (ID: {savedProfile.Id})");
                    }
                }
            }
            
            Debug.Log($"✅ Successfully loaded {loadedCount}/{savedProfiles.Count} saved profiles for the new game");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error loading profiles: {ex.Message}");
        }
    }
    
    // Exit to the main menu
    public void ExitToMenu()
    {
        Debug.Log("🔙 Exiting to main menu: " + menuSceneName);
        
        // Clear saved profiles when exiting to menu
        savedProfiles.Clear();
        
        // Handle any cleanup before scene change
        try
        {
            // Load the menu scene
            SceneManager.LoadScene(menuSceneName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error loading menu scene: {ex.Message}");
        }
    }
    
    // Restart the game with the same profiles
    public void RestartGame()
    {
        Debug.Log("🔄 Restarting game with same profiles: " + gameSceneName);
        
        // Save profiles before restarting if option is enabled
        if (preservePlayerProfiles)
        {
            SaveCurrentProfiles();
        }
        
        try
        {
            // Load the game scene
            SceneManager.LoadScene(gameSceneName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error loading game scene: {ex.Message}");
        }
    }
    
    // Check if there are saved profiles
    public static bool HasSavedProfiles()
    {
        return savedProfiles != null && savedProfiles.Count > 0;
    }
    
    // Get count of saved profiles (for debugging)
    public static int GetSavedProfilesCount()
    {
        return savedProfiles != null ? savedProfiles.Count : 0;
    }
    
    // Clear all saved profiles
    public static void ClearSavedProfiles()
    {
        if (savedProfiles != null)
        {
            savedProfiles.Clear();
            Debug.Log("🧹 All saved profiles cleared");
        }
    }
}