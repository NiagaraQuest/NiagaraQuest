using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    private GameObject selectedPlayer;

    private Player currentQuestionPlayer;



    void Start()
    {
        InitializePlayers();

        Debug.Log($"📌 Nombre de joueurs détectés: {players?.Count ?? 0}");

        AssignProfiles();
        StartGame();
    }




    // Finds all Player objects and stores them in players
    private void InitializePlayers()
    {
        players.Clear();

        players.Add(GameObject.Find("PyroPlayer"));
        players.Add(GameObject.Find("AnemoPlayer"));
        players.Add(GameObject.Find("GeoPlayer"));
        players.Add(GameObject.Find("HydroPlayer"));

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
            StartCoroutine(WaitForMovement(movementScript)); // Wait for movement to complete
        }
        else
        {
            Debug.LogError("❌ No Player script found on " + selectedPlayer.name);
        }
    }

    // Waits for the player to stop moving
    private IEnumerator WaitForMovement(Player movementScript)
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


    public void ApplyQuestionResult(Player player, bool isCorrect, Tile.Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Tile.Difficulty.Easy:
                if (isCorrect)
                {
                    // CA MARCHE 

                    Debug.Log("✅ Bonne réponse ! Récompense : Avancer de 2 cases.");
                    player.MoveForward(2);
                }
                else
                {
                    Debug.Log("❌ Mauvaise réponse ! Pénalité : Reculer de 6 cases.");



                    // Move the player backward
                    player.MovePlayerBack();




                }
                break;

            case Tile.Difficulty.Medium:
                if (isCorrect)
                {
                    Debug.Log("✅ Bonne réponse ! Récompense : Lancer les dés une nouvelle fois.");
                    // RollDiceAgain(player);
                    return; // Don't switch turns yet, the player rolls again
                }
                else
                {

                    // CA MARCHE 

                    Debug.Log("❌ Mauvaise réponse ! Pénalité : Perdre 1 vie.");
                    player.LoseLife();
                }
                break;

            case Tile.Difficulty.Hard:
                if (isCorrect)
                {
                    // ca marche 

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



    /*
    public void RollDiceAgain(Player player)
    {
        Debug.Log($"🎲 {player.gameObject.name} peut relancer les dés !");
        // Appelle ici ta fonction qui gère le lancement de dés
    }

   
    */

}







