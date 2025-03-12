using UnityEngine;
using System.Collections.Generic;

public class WaypointScript : MonoBehaviour
{
    public Board gameBoard;
    public IntersectionUIManager uiManager;
    public string currentPath = "GeoPath"; // Default path
    public float speed = 2;

    private int currentWaypointIndex = 0;
    private int targetWaypointIndex = 0;
    private bool isMoving = false;
    private bool reachedIntersection = false;
    private GameObject lastWaypointBeforeIntersection;
    private int remainingSteps = 0; // ✅ Stocke les pas restants après une intersection
    private int movementDirection = 1; // ✅ 1 pour forward, -1 pour backward

    // ✅ Vérifie si le mouvement est terminé
    public bool HasFinishedMoving => !isMoving && !reachedIntersection;

    void Start()
    {
        if (gameBoard != null)
        {
            MoveToWaypoint(0); // Commencer au premier waypoint
        }
    }

    void Update()
    {
        if (isMoving && !reachedIntersection)
        {
            GameObject targetWaypoint = gameBoard.GetTile(currentPath, currentWaypointIndex);

            if (targetWaypoint != null)
            {
                if (transform.position != targetWaypoint.transform.position)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        targetWaypoint.transform.position,
                        speed * Time.deltaTime
                    );
                }
                else
                {
                    if (targetWaypoint.CompareTag("Intersection"))
                    {
                        reachedIntersection = true;
                        isMoving = false;
                        uiManager.ShowUI(this); // Afficher le choix des chemins
                    }
                    else
                    {
                        lastWaypointBeforeIntersection = targetWaypoint;
                        currentWaypointIndex += movementDirection;
                        remainingSteps = Mathf.Max(0, remainingSteps - 1);

                        if (currentWaypointIndex == targetWaypointIndex || remainingSteps <= 0)
                        {
                            isMoving = false;
                        }
                    }
                }
            }
        }
    }

    // ✅ Déplace le joueur avec un vrai objectif
    public void MovePlayer(int steps)
    {
        if (!reachedIntersection)
        {
            remainingSteps = steps;
            targetWaypointIndex = currentWaypointIndex + (movementDirection * steps); // 🎯 Déterminer la cible
            isMoving = true;
        }
    }

    // ✅ Gère le changement de chemin et met à jour correctement le waypoint
    public void ResumeMovement(GameObject newPath, bool stayOnSamePath)
    {
        if (newPath != null)
        {
            string newPathName = newPath.transform.parent?.name; // 🔥 Récupérer le chemin parent
            int newWaypointIndex = gameBoard.GetWaypointIndex(newPathName, newPath); // 🔍 Obtenir l’index du waypoint

            if (gameBoard.PathExists(newPathName) && newWaypointIndex != -1) // ✅ Vérifier si c'est un chemin valide
            {
                currentPath = newPathName;
                currentWaypointIndex = newWaypointIndex;
                targetWaypointIndex = currentWaypointIndex + (movementDirection * remainingSteps);
                MoveToWaypoint(newWaypointIndex);
                Debug.Log($"✅ Changement vers le chemin {newPathName} au waypoint {newWaypointIndex}");

                // ✅ Déterminer la direction en fonction de l'étiquette du waypoint
                if (newPath.CompareTag("backward"))
                {
                    movementDirection = -1;
                }
                else if (newPath.CompareTag("forward"))
                {
                    movementDirection = 1;
                }
            }
            else
            {
                Debug.LogWarning($"❌ Impossible de changer vers {newPathName}, chemin inconnu !");
            }
        }
        else if (stayOnSamePath)
        {
            currentWaypointIndex += movementDirection;
            targetWaypointIndex = currentWaypointIndex + (movementDirection * remainingSteps);
            MoveToWaypoint(currentWaypointIndex);
        }

        reachedIntersection = false;
        isMoving = true;

        // ✅ Reprendre les pas restants après une intersection
        if (remainingSteps > 0)
        {
            MovePlayer(remainingSteps);
        }
    }

    // ✅ Déplacer directement le joueur à un waypoint
    private void MoveToWaypoint(int index)
    {
        GameObject waypoint = gameBoard.GetTile(currentPath, index);
        if (waypoint != null)
        {
            transform.position = waypoint.transform.position;
            currentWaypointIndex = index;
        }
    }

    public GameObject GetCurrentWaypoint()
    {
        return gameBoard.GetTile(currentPath, currentWaypointIndex);
    }

    public GameObject GetLastPath()
    {
        return lastWaypointBeforeIntersection;
    }
}