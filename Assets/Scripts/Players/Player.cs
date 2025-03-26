using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Player : MonoBehaviour
{
    public Board gameBoard;
    public IntersectionUIManager uiManager;
    public string currentPath;
    public float speed = 2;
    protected int currentWaypointIndex = 0;
    protected int targetWaypointIndex = 0;
    public bool isMoving = false;
    protected bool reachedIntersection = false;
    protected GameObject lastWaypointBeforeIntersection;
    protected int remainingSteps = 0;
    public int movementDirection = 1;
    public int TurnsToSkip = 0; // Nombre de tours à sauter



    public int lives = 4; // ✅ Starts with 4 lives
    public Profile playerProfile; // Visible in the Inspector

    [Header("🔹 Player Profile Info")]
    [SerializeField] public string debugProfileName; // 🔥 Affiche dans l'Inspector
    public bool HasFinishedMoving => !isMoving && !reachedIntersection;

    protected virtual void Start()
    {
        if (gameBoard != null)
        {
            MoveToWaypoint(0);
            DisplayCurrentRegion();
        }

        // Vérifie si le profil a été assigné correctement

        if (playerProfile != null && !string.IsNullOrEmpty(playerProfile.Username))
        {
            Debug.Log($"👤 {gameObject.name} → Profil après assignation : {playerProfile.Username}");
        }
        else
        {
            Debug.LogWarning($"⚠️ {gameObject.name} → Profil encore vide. Assignation en attente...");
            StartCoroutine(WaitForProfileAssignment());
        }
    }



    // 🔹 Coroutine pour attendre que le profil soit bien assigné
    private IEnumerator WaitForProfileAssignment()
    {
        while (playerProfile == null || string.IsNullOrEmpty(playerProfile.Username))
        {
            yield return null; // Attend la frame suivante
        }

        // ✅ Maintenant le profil est bien assigné !
        Debug.Log($"🎉 {gameObject.name} → Profil final : {playerProfile.Username}");
    }
    // 🔥 Force la mise à jour dans l'Inspector
    void  OnValidate()
    {
        if (playerProfile != null)
        {
            debugProfileName = playerProfile.Username;
        }
    }
    protected virtual void Update()
    {
        if (isMoving && !reachedIntersection)
        {
            if (targetWaypointIndex < 0)
            {
                Debug.LogWarning($"⚠️ ATTENTION: Index -1 détecté à l'étape {remainingSteps}. Chemin: {currentPath}, Index actuel: {currentWaypointIndex}, Target: {targetWaypointIndex}");

                targetWaypointIndex = 0;
                movementDirection = 1;
                remainingSteps++;
                Debug.Log($"✅ Correction appliquée → Nouvel index : {targetWaypointIndex}, Pas restants : {remainingSteps}");
            }

            GameObject targetWaypoint = gameBoard.GetTile(currentPath, targetWaypointIndex);

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
                    currentWaypointIndex = targetWaypointIndex;
                    Debug.Log($"📍 Waypoint Atteint: {currentWaypointIndex}");

                    if (targetWaypoint.CompareTag("Intersection"))
                    {
                        reachedIntersection = true;
                        isMoving = false;
                        uiManager.ShowUI(this);
                    }
                    else
                    {
                        lastWaypointBeforeIntersection = targetWaypoint;
                        remainingSteps--;

                        if (remainingSteps > 0)
                        {
                            targetWaypointIndex += movementDirection;
                            if (targetWaypointIndex < 0)
                            {
                                Debug.LogWarning($"⚠️ ATTENTION: targetWaypointIndex est négatif ({targetWaypointIndex}) à l'étape {remainingSteps}. Chemin: {currentPath}, Index actuel: {currentWaypointIndex}");

                                targetWaypointIndex = 0;
                                movementDirection = 1;
                                remainingSteps++;
                            }
                        }
                        else
                        {
                            isMoving = false;
                            DisplayCurrentRegion();
                        }
                    }
                }
            }
        }
    }
   


    public virtual void MovePlayer(int steps)
    {
        if (!reachedIntersection)
        {
            remainingSteps = steps;
            targetWaypointIndex = currentWaypointIndex + movementDirection;
            isMoving = true;
        }
    }

    public virtual void ResumeMovement(GameObject newPath, bool stayOnSamePath)
    {
        if (newPath != null)
        {
            string newPathName = newPath.transform.parent?.name;
            int newWaypointIndex = gameBoard.GetWaypointIndex(newPathName, newPath);

            if (gameBoard.PathExists(newPathName))
            {
                if (newWaypointIndex < 0)
                {
                    Debug.LogWarning($"⚠️ Index négatif détecté pour {newPathName}, correction à 0 !");
                    newWaypointIndex = 0;
                }

                currentPath = newPathName;
                currentWaypointIndex = newWaypointIndex;
                targetWaypointIndex = currentWaypointIndex; // ✅ No extra movement
                MoveToWaypoint(newWaypointIndex);

                if (newPath.CompareTag("backward"))
                {
                    movementDirection = -1;
                }
                else if (newPath.CompareTag("forward"))
                {
                    movementDirection = 1;
                }
            }
        }
        else if (stayOnSamePath)
        {
            targetWaypointIndex = currentWaypointIndex + movementDirection;
            MoveToWaypoint(currentWaypointIndex);
        }

        reachedIntersection = false;
        isMoving = (remainingSteps > 0);
    }

    protected virtual void MoveToWaypoint(int index)
    {
        GameObject waypoint = gameBoard.GetTile(currentPath, index);
        if (waypoint != null)
        {
            transform.position = waypoint.transform.position;
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

    private void DisplayCurrentRegion()
    {
        GameObject currentWaypoint = GetCurrentWaypoint();
        if (currentWaypoint != null)
        {
            Tile tile = currentWaypoint.GetComponent<Tile>();
            if (tile != null)
            {
                tile.OnPlayerLands();
            }
        }
    }

    // ✅ Method to lose a life
    public virtual void LoseLife()
    {
        if (lives > 0)
        {
            lives--;
            Debug.Log($"❌ lost a life! Remaining lives: {lives}");
        }
        else
        {
            Debug.Log($"💀  has no more lives!");
        }
    }

    // ✅ Method to gain a life
    public virtual void GainLife()
    {
        lives++;
        Debug.Log($"❤️gained a life! Total lives: {lives}");
    }
    public void SkipTurns(int turns)
    {
        TurnsToSkip += turns;
        Debug.Log($"⏳ {gameObject.name} doit sauter {turns} tour(s). Total à sauter : {TurnsToSkip}");
    }
    public virtual void AnswerQuestion(bool isCorrect)
    {
        //  later
    }
}

