using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Threading.Tasks;
using System.Linq;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameMode
    {
        TwoPlayers,
        ThreePlayers,
        FourPlayers
    }

    [Header("Game Settings")]
    public GameMode currentGameMode;
    public int maxLives;
    public int twoPlayersInitialLives = 4;
    public int threeOrFourPlayersInitialLives = 3;

    [Header("UI References")]
    public GameEndUIManager gameEndUIManager;
    [Header("Life Sharing Settings")]
    public bool allowLifeSharing = true;
    public bool hasDiceBeenRolledThisTurn = false;
    private LifeSharingManager lifeSharingManager; 

    private AudioManager audioManager; 


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
        audioManager = AudioManager.Instance;
        
        // Check if it exists
        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in the scene. Make sure it's set up properly.");
        }
    }

    // Stocke tous les joueurs
    public List<GameObject> players = new List<GameObject>();

    public int currentPlayerIndex = 0;
    public Board gameBoard;
    public DiceManager diceManager;

    public GameObject selectedPlayer;

    public Player currentQuestionPlayer;
    public bool isEffectMovement = false;
    private bool gameWon = false;
    private bool gameLost = false;
    public GameEndManager gameEndManager;

    void Start()
    {
        Debug.Log("🎲 GameManager starting...");
        lifeSharingManager = FindObjectOfType<LifeSharingManager>();
        DetectGameModeBasedOnActivePlayers();
        InitializePlayers();
        Debug.Log($"📌 Nombre de joueurs détectés: {players?.Count ?? 0}");

        // Initialize database and set up profiles
        _ = InitializeDatabaseAndSetupProfiles();

        SetupPlayersInitialLives();

        // Special initialization for certain player types
        foreach (var player in players)
        {
            if (player == null) continue;

            GeoPlayer geoPlayer = player.GetComponent<GeoPlayer>();
            if (geoPlayer != null)
            {
                Debug.Log("🔄 Réinitialisation du bouclier de GeoPlayer après configuration des vies");
                geoPlayer.InitializeShield();
            }
        }
        StartGame();
    }

    // Create profiles and assign them directly from database
    private async Task InitializeDatabaseAndSetupProfiles()
    {
        try
        {
            // First initialize the database manager
            Debug.Log("🔄 Initializing DatabaseManager...");
            await DatabaseManager.Instance.Initialize();
            Debug.Log("✅ DatabaseManager initialized successfully");

            // Check for existing profiles first
            var existingProfiles = await DatabaseManager.Instance.GetAll<Profile>();
            Debug.Log($"🔍 Found {existingProfiles.Count} existing profiles in database");
            // Query all profiles from database again to confirm what we have
            var updatedProfiles = await DatabaseManager.Instance.GetAll<Profile>();
            Debug.Log($"📊 Database now contains {updatedProfiles.Count} profiles");

            foreach (var profile in updatedProfiles)
            {
                Debug.Log($"  - Profile: {profile.Username} (ID: {profile.Id}, ELO: {profile.Elo})");
            }

            // Now assign profiles directly from database to players
            await AssignProfilesFromDatabase();
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Database error: {ex.Message}\n{ex.StackTrace}");
            CreateEmergencyProfiles();
        }
    }

    // Separate method to assign profiles directly from database
    private async Task AssignProfilesFromDatabase()
    {
        Debug.Log("🔄 Assigning profiles from database to players...");

        try
        {
            // Get all profiles from database
            var allProfiles = await DatabaseManager.Instance.GetAll<Profile>();

            if (allProfiles.Count == 0)
            {
                Debug.LogError("❌ No profiles found in database!");
                CreateEmergencyProfiles();
                return;
            }

            // Match profiles to players based on element type in the name
            foreach (var player in players)
            {
                if (player == null) continue;

                string elementType = player.name.Replace("Player", "");

                // Find a profile with matching element type
                Profile matchingProfile = allProfiles.FirstOrDefault(p =>
                    p.Username.StartsWith(elementType, StringComparison.OrdinalIgnoreCase));

                if (matchingProfile != null)
                {
                    Player playerScript = player.GetComponent<Player>();
                    if (playerScript != null)
                    {
                        playerScript.playerProfile = matchingProfile;
                        playerScript.debugProfileName = matchingProfile.Username;

                        Debug.Log($"✅ Assigned database profile to {player.name}: {matchingProfile.Username} (ID: {matchingProfile.Id}, ELO: {matchingProfile.Elo})");

                        // Remove this profile from the list to avoid duplicate assignments
                        allProfiles.Remove(matchingProfile);
                    }
                    else
                    {
                        Debug.LogError($"❌ No Player component found on {player.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ No matching profile found for {player.name}");

                    // If no matching profile, assign any available profile
                    if (allProfiles.Count > 0)
                    {
                        Player playerScript = player.GetComponent<Player>();
                        if (playerScript != null)
                        {
                            Profile anyProfile = allProfiles[0];
                            playerScript.playerProfile = anyProfile;
                            playerScript.debugProfileName = anyProfile.Username;

                            Debug.Log($"⚠️ Assigned non-matching profile to {player.name}: {anyProfile.Username} (ID: {anyProfile.Id})");

                            // Remove this profile from the list
                            allProfiles.RemoveAt(0);
                        }
                    }
                }
            }

            // Check if any players are missing profiles
            foreach (var player in players)
            {
                if (player == null) continue;

                Player playerScript = player.GetComponent<Player>();
                if (playerScript != null && playerScript.playerProfile == null)
                {
                    Debug.LogWarning($"⚠️ Player {player.name} still has no profile after database assignment");

                    // If there are any remaining profiles, assign one
                    if (allProfiles.Count > 0)
                    {
                        Profile remainingProfile = allProfiles[0];
                        playerScript.playerProfile = remainingProfile;
                        playerScript.debugProfileName = remainingProfile.Username;

                        Debug.Log($"⚠️ Assigned remaining profile to {player.name}: {remainingProfile.Username}");
                        allProfiles.RemoveAt(0);
                    }
                    else
                    {
                        // Create an emergency profile directly
                        try
                        {
                            string username = $"{player.name.Replace("Player", "")}Emergency{UnityEngine.Random.Range(100, 999)}";
                            Profile emergencyProfile = new Profile(username);

                            // Insert emergency profile into database
                            await DatabaseManager.Instance.Insert(emergencyProfile);

                            // Get the profile with ID
                            var createdProfile = await DatabaseManager.Instance.QueryFirstOrDefaultAsync<Profile>(
                                "SELECT * FROM Profiles WHERE Username = ?", username);

                            if (createdProfile != null)
                            {
                                playerScript.playerProfile = createdProfile;
                                playerScript.debugProfileName = createdProfile.Username;
                                Debug.Log($"⚠️ Created and assigned emergency profile from database: {createdProfile.Username} (ID: {createdProfile.Id})");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"❌ Failed to create emergency profile in database: {ex.Message}");
                            CreateEmergencyProfileForPlayer(player);
                        }
                    }
                }
            }

            Debug.Log("✅ Profile assignment from database complete");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error assigning profiles from database: {ex.Message}");
            CreateEmergencyProfiles();
        }
    }

    // Fallback method to create profiles directly in memory (without database)
    private void CreateEmergencyProfiles()
    {
        Debug.Log("⚠️ Creating emergency profiles directly (not in database)...");

        foreach (var player in players)
        {
            if (player == null) continue;

            CreateEmergencyProfileForPlayer(player);
        }

        Debug.Log("⚠️ Emergency profile creation complete");
    }

    // Create an emergency profile for a single player
    private void CreateEmergencyProfileForPlayer(GameObject player)
    {
        if (player == null) return;

        Player playerScript = player.GetComponent<Player>();
        if (playerScript == null) return;

        try
        {
            string elementType = player.name.Replace("Player", "");
            string username = $"{elementType}Emergency{UnityEngine.Random.Range(100, 999)}";

            Profile emergencyProfile = new Profile(username);
            playerScript.playerProfile = emergencyProfile;
            playerScript.debugProfileName = username;

            Debug.Log($"⚠️ Created direct emergency profile for {player.name}: {username}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to create even direct emergency profile: {ex.Message}");
        }
    }

    private void DetectGameModeBasedOnActivePlayers()
    {
        int activePlayers = 0;
        foreach (var playerObj in new List<GameObject> {
            GameObject.Find("PyroPlayer"),
            GameObject.Find("AnemoPlayer"),
            GameObject.Find("GeoPlayer"),
            GameObject.Find("HydroPlayer")
        })
        {
            if (playerObj != null && playerObj.activeInHierarchy) activePlayers++;
        }

        switch (activePlayers)
        {
            case 2:
                currentGameMode = GameMode.TwoPlayers;
                maxLives = twoPlayersInitialLives;
                break;
            case 3:
                currentGameMode = GameMode.ThreePlayers;
                maxLives = threeOrFourPlayersInitialLives;
                break;
            case 4:
                currentGameMode = GameMode.FourPlayers;
                maxLives = threeOrFourPlayersInitialLives;
                break;
            default:
                Debug.LogError($"Unsupported number of players: {activePlayers}");
                break;
        }

        Debug.Log($"🎮 Mode: {currentGameMode} | Players: {activePlayers} | Max Lives: {maxLives}");
    }

    private void SetupPlayersInitialLives()
    {
        foreach (var player in players)
        {
            if (player == null) continue;

            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.lives = maxLives;
                
                Debug.Log($"❤️ {player.name} initialized with {maxLives} lives");
            }
        }
    }


public bool CanCurrentPlayerGiveLife()
{
    if (selectedPlayer == null)
        return false;
        
    // Check if current player has 3+ lives
    Player currentPlayer = selectedPlayer.GetComponent<Player>();
    if (currentPlayer == null || currentPlayer.lives < 3)
        return false;
        
    // Check if any player has exactly 1 life
    foreach (GameObject playerObj in players)
    {
        if (playerObj == null || playerObj == selectedPlayer)
            continue;
        
        Player otherPlayer = playerObj.GetComponent<Player>();
        if (otherPlayer != null && otherPlayer.lives == 1)
        {
            return true;
        }
    }
    
    return false;
}

public void GiveLifeToPlayer(GameObject targetPlayerObject)
{
    if (selectedPlayer == null || targetPlayerObject == null)
        return;
    
    Player currentPlayer = selectedPlayer.GetComponent<Player>();
    Player targetPlayer = targetPlayerObject.GetComponent<Player>();
    
    if (currentPlayer == null || targetPlayer == null)
        return;
    
    // Check requirements
    if (currentPlayer.lives < 3)
    {
        Debug.LogWarning($"⚠️ {currentPlayer.gameObject.name} doesn't have enough lives to give (has {currentPlayer.lives}, needs at least 3)");
        return;
    }
    
    if (targetPlayer.lives != 1)
    {
        Debug.LogWarning($"⚠️ {targetPlayer.gameObject.name} must have exactly 1 life to receive (has {targetPlayer.lives})");
        return;
    }
    
    // Execute life transfer
    currentPlayer.lives--;
    targetPlayer.lives++;
    
    Debug.Log($"❤️ {currentPlayer.gameObject.name} gave a life to {targetPlayer.gameObject.name}!");
    
    // Notify LifeSharingManager if it exists
    if (lifeSharingManager != null)
    {
        lifeSharingManager.UpdateGiveLifeButtonVisibility();
    }
}

    // Finds all Player objects and stores them in players
    private void InitializePlayers()
    {
        players.Clear();
        Debug.Log("Initializing players...");

        // First try to find all players
        GameObject pyroPlayer = GameObject.Find("PyroPlayer");
        GameObject hydroPlayer = GameObject.Find("HydroPlayer");
        GameObject anemoPlayer = GameObject.Find("AnemoPlayer");
        GameObject geoPlayer = GameObject.Find("GeoPlayer");

        Debug.Log($"Found players - Pyro: {pyroPlayer != null}, Hydro: {hydroPlayer != null}, Anemo: {anemoPlayer != null}, Geo: {geoPlayer != null}");

        // Add only non-null players to the list
        if (pyroPlayer != null && pyroPlayer.activeInHierarchy) players.Add(pyroPlayer);
        if (hydroPlayer != null && hydroPlayer.activeInHierarchy) players.Add(hydroPlayer);
        if (anemoPlayer != null && anemoPlayer.activeInHierarchy) players.Add(anemoPlayer);
        if (geoPlayer != null && geoPlayer.activeInHierarchy) players.Add(geoPlayer);

        if (players.Count == 0)
        {
            Debug.LogError("❌ No active players found in the scene!");
        }

        Debug.Log($"Total active players: {players.Count}");
    }

    public void StartGame()
    {
        if (players.Count == 0)
        {
            Debug.LogError("❌ No players found!");
            return;
        }

        // Sets the first player and starts their turn
        currentPlayerIndex = 0;
        selectedPlayer = players[currentPlayerIndex];

        // Ensure selectedPlayer is not null
        if (selectedPlayer == null && players.Count > 0)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null)
                {
                    currentPlayerIndex = i;
                    selectedPlayer = players[i];
                    Debug.Log($"⚠️ Selected first available player: {selectedPlayer.name}");
                    break;
                }
            }
        }

        Debug.Log("🎮 Game Started! First player: " + (selectedPlayer != null ? selectedPlayer.name : "None"));

        // Activer le bouton de dés au début du jeu
        if (diceManager != null)
        {
            diceManager.EnableRollButton();
        }
    }

    public void OnDiceRolled()
    {
        Debug.Log("🎲 Dice rolled!");
        hasDiceBeenRolledThisTurn = true;

    if (lifeSharingManager != null)
    {
        lifeSharingManager.OnDiceRolled();
    }

        // Désactiver le bouton de dés après le lancer
        if (diceManager != null)
        {
            diceManager.DisableRollButton();
        }

        // Make sure selectedPlayer is valid
        if (selectedPlayer == null)
        {
            Debug.LogError("❌ No player selected for dice roll!");

            // Try to select a valid player
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null)
                {
                    selectedPlayer = players[i];
                    currentPlayerIndex = i;
                    Debug.Log($"⚠️ Selected player {selectedPlayer.name} for dice roll");
                    break;
                }
            }

            if (selectedPlayer == null)
            {
                Debug.LogError("❌ Could not find any valid player for dice roll!");
                return;
            }
        }

        Debug.Log($"🎲 Moving player: {selectedPlayer.name}");
        MoveSelectedPlayer();
    }

    private void MoveSelectedPlayer()
    {
        if (selectedPlayer == null)
        {
            Debug.LogError("❌ No player selected for movement!");
            return;
        }

        int moveSteps = diceManager.LastRollSum;
        Player movementScript = selectedPlayer.GetComponent<Player>(); // Get the Player script

        if (movementScript != null)
        {
            // Ensure profile exists before movement
            if (movementScript.playerProfile == null)
            {
                Debug.LogWarning($"⚠️ Player {selectedPlayer.name} has no profile before movement!");

                // Create an emergency profile directly since this is during gameplay
                CreateEmergencyProfileForPlayer(selectedPlayer);
            }
            movementScript.MovePlayer(moveSteps);
            StartCoroutine(WaitForMovements(movementScript)); // Wait for movement to complete
        }
        else
        {
            Debug.LogError("❌ No Player script found on " + selectedPlayer.name);
        }
    }

    // Waits for the player to stop moving
    private IEnumerator WaitForMovements(Player movementScript)
    {
        yield return new WaitUntil(() => movementScript.HasFinishedMoving);
        NextTurn();
    }

    public void SetCurrentQuestionPlayer(Player player)
    {
        currentQuestionPlayer = player;
    }

    public Player GetCurrentPlayer()
    {
        return currentQuestionPlayer;
    }

    public void ApplyQuestionResult(Player player, bool isCorrect, string difficulty)
    {
                    if (isCorrect)
    {
        audioManager.PlayRightAnswer();
    }
    else
    {
        audioManager.PlayWrongAnswer();
    }
        switch (difficulty.ToUpper())
        {
            case "EASY":
                if (isCorrect)
                {
                    // CA MARCHE 
                    Debug.Log("✅ Bonne réponse ! Récompense : Avancer de 2 cases.");
                    isEffectMovement = true;
                    player.MovePlayer(2);
                }
                else
                {
                    Debug.Log("❌ Mauvaise réponse ! Pénalité : Reculer de 6 cases.");
                    isEffectMovement = true;
                    player.MovePlayerBack();
                }
                break;

            case "MEDIUM":
                if (isCorrect)
                {
                    Debug.Log("✅ Bonne réponse ! Récompense : Lancer les dés une nouvelle fois.");
                    isEffectMovement = true;
                    RollDiceAgain(player);
                    return;
                }
                else
                {
                    Debug.Log("❌ Mauvaise réponse ! Pénalité : Perdre 1 vie.");
                    player.LoseLife();
                }
                break;

            case "HARD":
                if (isCorrect)
                {
                    Debug.Log("✅ Bonne réponse ! Récompense : Gagner 1 vie.");
                    player.GainLife();
                }

                else
                {
                    int turnsSkipped = 1;
                    Debug.Log($"❌ Mauvaise réponse ! Pénalité : Passer {turnsSkipped} tours.");
                    player.SkipTurns(turnsSkipped);
                }
                break;
        }
    }

    private void NextTurn()
    {
        hasDiceBeenRolledThisTurn = false;
        isEffectMovement = false;
        SetCurrentQuestionPlayer(selectedPlayer.GetComponent<Player>());
    if (lifeSharingManager != null)
    {
        lifeSharingManager.OnNewTurn();
    }

        int attempts = 0;
        int maxAttempts = players.Count * 2;

        do
        {
            attempts++;
            if (attempts > maxAttempts)
            {
                Debug.LogError("❌ Too many attempts to find next player. Breaking loop.");
                break;
            }

            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            selectedPlayer = players[currentPlayerIndex];

            if (selectedPlayer == null)
            {
                Debug.LogWarning($"⚠️ Player at index {currentPlayerIndex} is null. Skipping...");
                continue;
            }

            Player p = selectedPlayer.GetComponent<Player>();
            if (p == null)
            {
                Debug.LogWarning($"⚠️ No Player component on {selectedPlayer.name}. Skipping...");
                continue;
            }

            if (p.ShouldSkipTurn())
            {
                p.DecrementSkipTurn(); // ❗on décrémente le compteur ici
                Debug.Log($"⏭️ {selectedPlayer.name} passe son tour. Reste : {p.turnsToSkip} tours à sauter.");
            }
            else
            {
                break; // ✅ joueur peut jouer
            }

        } while (attempts <= maxAttempts); // continue jusqu'à trouver un joueur qui peut jouer

        Debug.Log($"🔄 Prochain joueur : {selectedPlayer.name}");

        // Activer le bouton de dés pour le nouveau joueur
        if (diceManager != null)
        {
            diceManager.EnableRollButton();
        }
    }


    private bool isExtraTurn = false;

    public void RollDiceAgain(Player player)
    {
        Debug.Log($"🎲 {player.gameObject.name} peut relancer les dés comme récompense!");

        // Activer le flag pour indiquer un tour supplémentaire
        isExtraTurn = true;

        // Réinitialiser l'état du joueur 
        currentPlayerIndex = players.IndexOf(player.gameObject);
        selectedPlayer = player.gameObject;

        // Activer le bouton de dés pour permettre un nouveau lancer
        if (diceManager != null)
        {
            diceManager.EnableRollButton();
        }

        Debug.Log($"🔄 {player.gameObject.name} obtient un tour supplémentaire!");
    }

    public bool IsGameWon()
    {
        return gameWon;
    }
    public void WinGameOver(Player winningPlayer)
    {
        Debug.Log($"🏆 WinGameOver appelé pour le joueur: {(winningPlayer != null ? winningPlayer.gameObject.name : "null")}");

        if (gameWon)
        {
            Debug.Log("🏆 Le jeu est déjà gagné, ignoré");
            return; // Éviter d'appeler plusieurs fois
        }

        Debug.Log("🏆 DÉFINITION DE VICTOIRE DU JEU");
        gameWon = true;

        string playerName = winningPlayer != null ? winningPlayer.gameObject.name : "Un joueur";
        Debug.Log($"🏆 VICTOIRE ! {playerName} a atteint un waypoint final et répondu correctement ! Tous les joueurs ont gagné !");

        // Désactiver les contrôles
        if (diceManager != null)
        {
            Debug.Log("🎮 Désactivation du bouton de dé");
            diceManager.DisableRollButton();
        }

        // Afficher un effet visuel ou un message pour chaque joueur
        foreach (GameObject playerObj in players)
        {
            if (playerObj == null) continue;

            Player player = playerObj.GetComponent<Player>();
            if (player != null)
            {
                Debug.Log($"🎉 {player.gameObject.name} célèbre la victoire !");
            }
        }

        if (gameEndUIManager != null)
        {
            gameEndUIManager.ShowVictoryScreen(winningPlayer);
        }
        else
        {
            Debug.LogWarning("⚠️ Panneau de victoire non assigné dans GameManager!");
            
            // Fallback: try to find GameEndManager directly if UI manager is not set
            GameEndManager endManager = FindObjectOfType<GameEndManager>();
            if (endManager != null)
            {
                endManager.CleanupUIForGameEnd();
                Debug.Log("⚠️ Utilisé GameEndManager directement pour nettoyer l'UI.");
            }
        }
    }
    public void CheckPlayerLives()
    {
        if (gameLost) return; // Éviter d'appeler plusieurs fois

        foreach (GameObject playerObj in players)
        {
            Player player = playerObj.GetComponent<Player>();
            if (player != null && player.lives <= 0)
            {
                // Un joueur a perdu toutes ses vies, on appelle LoseGame
                LoseGame(player);
                return;
            }
        }
    }
    

    public void LoseGame(Player losingPlayer)
    {
        if (gameLost || gameWon) return; // Éviter d'appeler plusieurs fois

        gameLost = true;
        string playerName = losingPlayer != null ? losingPlayer.gameObject.name : "Un joueur";
        Debug.Log($"💀 DÉFAITE ! {playerName} a perdu toutes ses vies ! La partie est terminée !");

        // Désactiver les contrôles
        if (diceManager != null)
        {
            diceManager.DisableRollButton();
        }

        // Afficher l'état final dans les logs
        foreach (GameObject playerObj in players)
        {
            if (playerObj == null) continue;

            Player player = playerObj.GetComponent<Player>();
            if (player != null)
            {
                Debug.Log($"📊 État final : {player.gameObject.name} a terminé avec {player.lives} vies.");
            }
        }

        if (gameEndUIManager != null)
        {
            gameEndUIManager.ShowDefeatScreen(losingPlayer);
        }
        else
        {
            Debug.LogWarning("⚠️ gameEndUIManager non assigné dans GameManager. Impossible d'afficher l'écran de défaite!");
            
            // Fallback: try to find GameEndManager directly if UI manager is not set
            GameEndManager endManager = FindObjectOfType<GameEndManager>();
            if (endManager != null)
            {
                endManager.CleanupUIForGameEnd();
                Debug.Log("⚠️ Utilisé GameEndManager directement pour nettoyer l'UI.");
            }
        }
    }

}