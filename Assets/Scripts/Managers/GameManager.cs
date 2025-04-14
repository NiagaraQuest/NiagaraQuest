using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

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




    void Start()
    {
        DetectGameModeBasedOnActivePlayers();
        InitializePlayers();
        Debug.Log($"📌 Nombre de joueurs détectés: {players?.Count ?? 0}");
        AssignProfiles();
        SetupPlayersInitialLives();

        // Vérifier si certains joueurs ont besoin d'une initialisation spéciale
        foreach (var player in players)
        {
            // Spécifiquement identifier GeoPlayer pour son initialisation spéciale
            GeoPlayer geoPlayer = player.GetComponent<GeoPlayer>();
            if (geoPlayer != null)
            {
                // Pour GeoPlayer, réactiver le bouclier après l'initialisation des vies
                Debug.Log("🔄 Réinitialisation du bouclier de GeoPlayer après configuration des vies");
                geoPlayer.InitializeShield();
            }
        }

        StartGame();
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
                //playerScript.maxLives = maxLives;
                
                Debug.Log($"❤️ {player.name} initialized with {maxLives} lives");
            }
        }
    }





    // Finds all Player objects and stores them in players
    private void InitializePlayers()
    {
        players.Clear();

        players.Add(GameObject.Find("PyroPlayer"));
        players.Add(GameObject.Find("HydroPlayer"));
        players.Add(GameObject.Find("AnemoPlayer"));
        players.Add(GameObject.Find("GeoPlayer"));
        

        if (players.Contains(null))
        {
            Debug.LogError("❌ Some players are missing from the scene!");
        }
    }

    // Assigns a Profile to each player
    private void AssignProfiles()
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogError("❌ Aucun joueur trouvé !");
            return;
        }

        foreach (GameObject playerObject in players)
        {
            Player playerScript = playerObject.GetComponent<Player>();

            if (playerScript != null)
            {
                if (playerScript.playerProfile == null)
                {
                    playerScript.playerProfile = new Profile();
                }

                if (string.IsNullOrEmpty(playerScript.playerProfile.Username))
                {
                    playerScript.playerProfile.Username = "Joueur_" + UnityEngine.Random.Range(100, 999);
                }

                // ✅ Debug log for profile
                Debug.Log($"✅ {playerObject.name} → Profil assigné : {playerScript.playerProfile.Username}");
            }
            else
            {
                Debug.LogError($"❌ Pas de script Player sur {playerObject.name} !");
            }
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
        selectedPlayer = players[currentPlayerIndex];
        Debug.Log("🎮 Game Started! First player: " + selectedPlayer.name);
    }

    public void OnDiceRolled()
    {
        Debug.Log("🎲 Dice rolled! Moving player...");
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

  


    public void ApplyQuestionResult(Player player, bool isCorrect, Tile.Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Tile.Difficulty.Easy:
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

            case Tile.Difficulty.Medium:
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

            case Tile.Difficulty.Hard:
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
        // Réinitialiser le flag au cas où
        isEffectMovement = false;
        SetCurrentQuestionPlayer(selectedPlayer.GetComponent<Player>());

        do
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            selectedPlayer = players[currentPlayerIndex];

            Player p = selectedPlayer.GetComponent<Player>();

            if (p.ShouldSkipTurn())
            {
                p.DecrementSkipTurn(); // ❗on décrémente le compteur ici
                Debug.Log($"⏭️ {selectedPlayer.name} passe son tour. Reste : {p.turnsToSkip} tours à sauter.");
            }
            else
            {
                break; // ✅ joueur peut jouer
            }

        } while (true); // continue jusqu'à trouver un joueur qui peut jouer

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

 
    // Dans la classe GameManager
    public void WinGameOver(Player winningPlayer)
    {
        if (gameWon) return; // Éviter d'appeler plusieurs fois

        gameWon = true;

        string playerName = winningPlayer != null ? winningPlayer.gameObject.name : "Un joueur";
        Debug.Log($"🏆 VICTOIRE ! {playerName} a atteint un waypoint final ! Tous les joueurs ont gagné !");

        // Désactiver les contrôles
        if (diceManager != null)
        {
            diceManager.DisableRollButton();
        }

        // Afficher un effet visuel ou un message pour chaque joueur
        foreach (GameObject playerObj in players)
        {
            Player player = playerObj.GetComponent<Player>();
            if (player != null)
            {
                // Tu pourrais ajouter un effet visuel ici
                Debug.Log($"🎉 {player.gameObject.name} célèbre la victoire !");
            }
        }

        // Tu peux appeler ici une méthode pour afficher l'écran de victoire
        // ShowVictoryScreen();
    }


    
 // Modifions aussi le WaitForMovement pour gérer le cas d'un tour supplémentaire
 private IEnumerator WaitForMovement(Player movementScript)
 {
     yield return new WaitUntil(() => movementScript.HasFinishedMoving);

     if (isExtraTurn)
     {
         // Réinitialiser le flag pour le prochain tour
         isExtraTurn = false;
         Debug.Log($"✅ Tour supplémentaire terminé pour {selectedPlayer.name}");
     }
     else
     {
        isEffectMovement = false;
         NextTurn();
     }
 }


  private IEnumerator RestoreDirectionWhenStopped(Player player, int originalDirection)
    {
        // Wait until the player finishes moving
        while (player.isMoving)
        {
            yield return null; // Wait for the next frame
        }

        // Restore the original movement direction
        player.movementDirection = originalDirection;
        Debug.Log("✅ Direction restored after movement.");
    }



}