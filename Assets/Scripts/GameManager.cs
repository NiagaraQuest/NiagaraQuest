using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    //Stocke tous les joueurs
    public List<GameObject> players = new List<GameObject>();

    public int currentPlayerIndex = 0;
    public Board gameBoard;
    public DiceManager diceManager;

    private GameObject selectedPlayer;

    // Vérifie que les joueurs existent et sélectionne le premier joueur.

    void Start()
    {
        InitializePlayers();
        StartGame();
    }

    //Finds all Player objects and stores them in players
    private void InitializePlayers()
    {
        players.Clear();

        players.Add(GameObject.Find("PyroPlayer"));
        players.Add(GameObject.Find("AnemoPlayer"));
        players.Add(GameObject.Find("GeoPlayer"));
        players.Add(GameObject.Find("HydroPlayer"));

        if (players.Contains(null))
        {
            Debug.LogError(" Some players are missing from the scene!");
        }
    }


    public void StartGame()
    {
        if (players.Count == 0)
        {
            Debug.LogError(" No players found!");
            return;
        }

        //Sets the first player and starts their turn

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
            StartCoroutine(WaitForMovement(movementScript)); //  Wait for movement to complete
        }
        else
        {
            Debug.LogError("❌ No WaypointScript found on " + selectedPlayer.name);
        }
    }

    //Waits for the player to stop moving
    private IEnumerator WaitForMovement(Player movementScript)
    {
        yield return new WaitUntil(() => movementScript.HasFinishedMoving);
        NextTurn();
    }

    private void NextTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        selectedPlayer = players[currentPlayerIndex];
        Debug.Log($"🔄 Next turn: {selectedPlayer.name}");
    }
}

