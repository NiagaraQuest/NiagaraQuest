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
        
        // Initialize database and set up profiles
        _ = InitializeDatabaseAndSetupProfiles();
        
        // After profiles are assigned, detect game mode and setup lives
        DetectGameModeBasedOnActivePlayers();
        SetupPlayersInitialLives();

        // Find and initialize LifeSharingManager AFTER players are set up
        lifeSharingManager = FindObjectOfType<LifeSharingManager>();
        if (lifeSharingManager != null)
        {
            Debug.Log("✅ LifeSharingManager found. Initializing...");
            lifeSharingManager.InitializeWithGameManager(this);
        }
        else
        {
            Debug.LogWarning("⚠️ LifeSharingManager not found in scene. Life sharing feature won't be available.");
        }

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

    // Set up profiles from PlayerPrefs (selected in menu)
    private async Task InitializeDatabaseAndSetupProfiles()
    {
        try
        {
            // First initialize the database manager
            Debug.Log("🔄 Initializing DatabaseManager...");
            await DatabaseManager.Instance.Initialize();
            Debug.Log("✅ DatabaseManager initialized successfully");
            
            // Now load profiles from PlayerPrefs (selected in the menu)
            await AssignProfilesFromPlayerPrefs();
            
            // Make sure selectedPlayer is set after players are loaded
            if (players != null && players.Count > 0)
            {
                currentPlayerIndex = 0;
                selectedPlayer = players[0];
                Debug.Log($"✅ Selected initial player: {selectedPlayer.name}");
            }
            else
            {
                Debug.LogError("❌ No players available to select!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Database error: {ex.Message}\n{ex.StackTrace}");
        }
    }
    // Assign profiles based on selections from the menu
    private async Task AssignProfilesFromPlayerPrefs()
    {
        Debug.Log("🔄 Assigning profiles from PlayerPrefs to players...");
        
        try
        {
            // Prepare a list with proper positions for each player
            GameObject[] orderedPlayers = new GameObject[4]; // Use array to maintain positions
            
            // Check each player GameObject - Maintain fixed order: Pyro, Hydro, Anemo, Geo
            
            // Check PyroPlayer (position 0)
            GameObject pyroPlayer = GameObject.Find("PyroPlayer");
            if (pyroPlayer != null && PlayerPrefs.GetInt("PyroPlayer_Active", 0) == 1)
            {
                AssignProfileToPlayer(pyroPlayer, "PyroPlayer");
                orderedPlayers[0] = pyroPlayer;
            }
            
            // Check HydroPlayer (position 1)
            GameObject hydroPlayer = GameObject.Find("HydroPlayer");
            if (hydroPlayer != null && PlayerPrefs.GetInt("HydroPlayer_Active", 0) == 1)
            {
                AssignProfileToPlayer(hydroPlayer, "HydroPlayer");
                orderedPlayers[1] = hydroPlayer;
            }
            
            // Check AnemoPlayer (position 2)
            GameObject anemoPlayer = GameObject.Find("AnemoPlayer");
            if (anemoPlayer != null && PlayerPrefs.GetInt("AnemoPlayer_Active", 0) == 1)
            {
                AssignProfileToPlayer(anemoPlayer, "AnemoPlayer");
                orderedPlayers[2] = anemoPlayer;
            }
            
            // Check GeoPlayer (position 3)
            GameObject geoPlayer = GameObject.Find("GeoPlayer");
            if (geoPlayer != null && PlayerPrefs.GetInt("GeoPlayer_Active", 0) == 1)
            {
                AssignProfileToPlayer(geoPlayer, "GeoPlayer");
                orderedPlayers[3] = geoPlayer;
            }
            
            // Disable inactive players
            if (pyroPlayer != null && PlayerPrefs.GetInt("PyroPlayer_Active", 0) != 1)
            {
                pyroPlayer.SetActive(false);
                Debug.Log($"🚫 Disabling PyroPlayer - not selected in menu");
            }
            
            if (hydroPlayer != null && PlayerPrefs.GetInt("HydroPlayer_Active", 0) != 1)
            {
                hydroPlayer.SetActive(false);
                Debug.Log($"🚫 Disabling HydroPlayer - not selected in menu");
            }
            
            if (anemoPlayer != null && PlayerPrefs.GetInt("AnemoPlayer_Active", 0) != 1)
            {
                anemoPlayer.SetActive(false);
                Debug.Log($"🚫 Disabling AnemoPlayer - not selected in menu");
            }
            
            if (geoPlayer != null && PlayerPrefs.GetInt("GeoPlayer_Active", 0) != 1)
            {
                geoPlayer.SetActive(false);
                Debug.Log($"🚫 Disabling GeoPlayer - not selected in menu");
            }
            
            // Update the players list - keep only active players but maintain order
            players = new List<GameObject>();
            foreach (GameObject player in orderedPlayers)
            {
                if (player != null && player.activeInHierarchy)
                {
                    players.Add(player);
                }
            }
            
            Debug.Log($"✅ Successfully assigned profiles to {players.Count} active players");
            
            // Print player order for debugging
            string playerOrder = "";
            for (int i = 0; i < players.Count; i++)
            {
                playerOrder += players[i].name + (i < players.Count - 1 ? " -> " : "");
            }
            Debug.Log($"🔄 Player turn order: {playerOrder}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error assigning profiles from PlayerPrefs: {ex.Message}");
        }
    }

    // Helper method to assign a profile to a player
    private void AssignProfileToPlayer(GameObject playerObject, string playerKey)
    {
        if (playerObject == null) return;
        
        Player playerScript = playerObject.GetComponent<Player>();
        if (playerScript == null) return;
        
        int profileId = PlayerPrefs.GetInt($"{playerKey}_ProfileId", -1);
        string username = PlayerPrefs.GetString($"{playerKey}_ProfileName", "Unknown");
        int elo = PlayerPrefs.GetInt($"{playerKey}_ProfileElo", 1000);
        
        // Create and assign profile
        Profile profile = new Profile();
        profile.Id = profileId;
        profile.Username = username;
        profile.Elo = elo;
        
        playerScript.playerProfile = profile;
        playerScript.debugProfileName = username;
        
        Debug.Log($"✅ Assigned profile to {playerObject.name}: {username} (ID: {profileId}, ELO: {elo})");
    }

    private void DetectGameModeBasedOnActivePlayers()
    {
        int activePlayers = players.Count;

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
        
        // Check if any player has exactly 1 life
        bool anyPlayerHasOneLife = false;
        foreach (GameObject playerObj in players)
        {
            if (playerObj == null || playerObj == selectedPlayer)
                continue;
                
            Player otherPlayer = playerObj.GetComponent<Player>();
            if (otherPlayer != null && otherPlayer.lives == 1)
            {
                anyPlayerHasOneLife = true;
                Debug.Log($"⚠️ Player {otherPlayer.gameObject.name} has 1 life at game start");
                break;
            }
        }
        
        // If current player has enough lives and another player has 1 life
        if (selectedPlayer != null)
        {
            Player currentPlayer = selectedPlayer.GetComponent<Player>();
            if (currentPlayer != null && currentPlayer.lives >= 3 && anyPlayerHasOneLife)
            {
                Debug.Log($"✅ Conditions met for life sharing at game start: {currentPlayer.gameObject.name} has {currentPlayer.lives} lives");
            }
        }

        // Update life sharing button visibility at game start
        if (lifeSharingManager != null)
        {
            hasDiceBeenRolledThisTurn = false; // Reset this flag
            lifeSharingManager.OnNewTurn(); // Notify the LifeSharingManager
            lifeSharingManager.UpdateGiveLifeButtonVisibility();
        }

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