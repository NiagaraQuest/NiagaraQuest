using System.Collections.Generic;
using UnityEngine;
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

    public int turnsToSkip = 0;

    // Pile pour stocker les derniers waypoints (maximum 20)
    protected Stack<WaypointData> previousWaypoints = new Stack<WaypointData>();
    protected const int MAX_WAYPOINT_HISTORY = 20;   // Maximum de waypoints à stocker
    protected const int MAX_MOVE_BACK_STEPS = 7;    // Maximum de pas en arrière à la fois
    protected bool isMovingBack = false; // Pour indiquer que le joueur est en train de reculer
    protected bool hasStoredInitialWaypoint = false; // Pour s'assurer que le waypoint initial est stocké une seule fois

    public int lives ; // 
    public Profile playerProfile; // Visible in the Inspector

    [Header("🔹 Player Profile Info")]
    [SerializeField] public string debugProfileName; // 🔥 Affiche dans l'Inspector
    public bool HasFinishedMoving => !isMoving && !reachedIntersection;

    // Structure pour stocker les données d'un waypoint avec sa direction
    protected struct WaypointData
    {
        public string pathName;
        public int waypointIndex;
        public int direction;  // Direction du mouvement pour ce waypoint

        public WaypointData(string path, int index, int dir)
        {
            pathName = path;
            waypointIndex = index;
            direction = dir;
        }
    }

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
    void OnValidate()
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

                    // Si on n'est pas en train de reculer, stocker ce waypoint dans la pile
                    if (!isMovingBack && !targetWaypoint.CompareTag("Intersection"))
                    {
                        StoreWaypointInHistory();
                    }

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
                            isMovingBack = false; // Réinitialiser l'état de recul
                            DisplayCurrentRegion();
                        }
                    }
                }
            }
        }
    }

    // Méthode pour stocker un waypoint dans l'historique avec sa direction
    protected virtual void StoreWaypointInHistory()
    {
        // Si la pile dépasse la limite, supprimer le waypoint le plus ancien
        if (previousWaypoints.Count >= MAX_WAYPOINT_HISTORY)
        {
            Stack<WaypointData> tempStack = new Stack<WaypointData>();

            // Retirer tous les éléments sauf le dernier
            while (previousWaypoints.Count > 1)
            {
                tempStack.Push(previousWaypoints.Pop());
            }

            // Supprimer le plus ancien
            previousWaypoints.Pop();

            // Remettre les autres dans la pile
            while (tempStack.Count > 0)
            {
                previousWaypoints.Push(tempStack.Pop());
            }
        }

        // Ajouter le waypoint actuel dans la pile avec la direction courante
        previousWaypoints.Push(new WaypointData(currentPath, currentWaypointIndex, movementDirection));
        Debug.Log($"🔄 Waypoint stocké: {currentPath} - {currentWaypointIndex} (direction: {movementDirection}). Total stocké: {previousWaypoints.Count}/{MAX_WAYPOINT_HISTORY}");
    }

    // Cette méthode est appelée lorsqu'un joueur commence son tour
    public virtual void StartTurn()
    {
        // Si c'est la première fois que ce joueur joue, stocker sa position initiale
        if (!hasStoredInitialWaypoint)
        {
            StoreWaypointInHistory();
            hasStoredInitialWaypoint = true;
            Debug.Log($"🏁 {gameObject.name} → Waypoint initial stocké: {currentPath} - {currentWaypointIndex} (direction: {movementDirection})");
        }
    }

    public virtual void MovePlayer(int steps)
    {
        // Stocker le waypoint initial si c'est le premier tour du joueur
        if (!hasStoredInitialWaypoint)
        {
            StoreWaypointInHistory();
            hasStoredInitialWaypoint = true;
            Debug.Log($"🏁 {gameObject.name} → Waypoint initial stocké: {currentPath} - {currentWaypointIndex} (direction: {movementDirection})");
        }

        if (!reachedIntersection)
        {
            remainingSteps = steps;
            targetWaypointIndex = currentWaypointIndex + movementDirection;
            isMoving = true;
        }
    }

    // Nouvelle méthode pour reculer en utilisant les waypoints stockés
    public virtual void MovePlayerBack()
    {
        if (isMoving || reachedIntersection)
        {
            Debug.LogWarning("⚠️ Impossible de reculer pendant un mouvement ou à une intersection");
            return;
        }

        if (previousWaypoints.Count == 0)
        {
            Debug.LogWarning("⚠️ Aucun waypoint stocké, impossible de reculer");
            return;
        }

        // Calculer combien de pas en arrière on peut faire (limité à MAX_MOVE_BACK_STEPS ou nombre disponible)
        int stepsToGoBack = Mathf.Min(previousWaypoints.Count, MAX_MOVE_BACK_STEPS);

        Debug.Log($"🔄 Recul de {stepsToGoBack} pas sur {previousWaypoints.Count} waypoints stockés (direction actuelle: {movementDirection})");

        isMovingBack = true;
        MoveBackToStoredWaypoints(0, stepsToGoBack);
    }

    // Méthode pour naviguer à travers les waypoints stockés
    protected virtual void MoveBackToStoredWaypoints(int currentStepCount, int maxSteps)
    {
        // Si on a atteint la limite de pas ou si la pile est vide, on s'arrête
        if (previousWaypoints.Count == 0 || currentStepCount >= maxSteps)
        {
            isMovingBack = false;
            isMoving = false;

            if (currentStepCount >= maxSteps)
            {
                Debug.Log($"🛑 Maximum de {maxSteps} pas atteint. Reste {previousWaypoints.Count} waypoints dans l'historique.");
            }
            else
            {
                Debug.Log($"🔙 Retour terminé après {currentStepCount} pas (direction finale: {movementDirection})");
            }

            DisplayCurrentRegion();
            return;
        }

        // Récupérer le dernier waypoint stocké avec sa direction
        WaypointData lastWaypoint = previousWaypoints.Pop();
        currentStepCount++; // Incrémenter le compteur de pas

        // Se déplacer vers ce waypoint
        currentPath = lastWaypoint.pathName;
        targetWaypointIndex = lastWaypoint.waypointIndex;
        currentWaypointIndex = targetWaypointIndex; // Pas de déplacement en plus

        // Restaurer la direction associée à ce waypoint
        movementDirection = lastWaypoint.direction;

        Debug.Log($"🔙 Retour vers: {currentPath} - {targetWaypointIndex} (direction: {movementDirection}). Pas #{currentStepCount}/{maxSteps}. Restants: {previousWaypoints.Count}");

        // Déplacer le joueur instantanément
        MoveToWaypoint(targetWaypointIndex);

        // S'il reste des waypoints et qu'on n'a pas atteint le maximum de pas, continuer à reculer
        if (previousWaypoints.Count > 0 && currentStepCount < maxSteps)
        {
            StartCoroutine(MoveBackWithDelay(currentStepCount, maxSteps));
        }
        else
        {
            isMovingBack = false;

            if (currentStepCount >= maxSteps && previousWaypoints.Count > 0)
            {
                Debug.Log($"🛑 Maximum de {maxSteps} pas atteint. Reste {previousWaypoints.Count} waypoints dans l'historique. Direction finale: {movementDirection}");
            }
            else
            {
                Debug.Log($"🔙 Retour terminé après {currentStepCount} pas. Direction finale: {movementDirection}");
            }

            DisplayCurrentRegion();
        }
    }

    // Coroutine pour ajouter un délai entre chaque mouvement en arrière
    protected virtual IEnumerator MoveBackWithDelay(int currentStepCount, int maxSteps)
    {
        yield return new WaitForSeconds(0.3f); // Délai entre chaque mouvement en arrière
        MoveBackToStoredWaypoints(currentStepCount, maxSteps);
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

    // Dans la classe Player, ajoute à la fin de DisplayCurrentRegion() :

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

            // Vérifier si c'est un waypoint final
            CheckForWinCondition(currentWaypoint);
        }
    }

    // Nouvelle méthode pour vérifier la condition de victoire
    protected virtual void CheckForWinCondition(GameObject waypoint)
    {
        // Vérifier si le waypoint actuel est un waypoint de victoire
        if (waypoint.name == "PyroWin" || waypoint.name == "HydroWin" ||
            waypoint.name == "GeoWin" || waypoint.name == "AnemoWin")
        {
            Debug.Log($"🏆 {gameObject.name} a atteint le waypoint de victoire {waypoint.name} !");

            // Appeler la méthode de victoire dans GameManager
            GameManager.Instance.WinGameOver(this);
        }

        // Alternative : vérifier par l'index si tous les waypoints de victoire sont à l'index 50
        if (currentWaypointIndex == 50)
        {
            string pathEndName = currentPath + " final";
            Debug.Log($"🏆 {gameObject.name} a atteint l'index 50 sur {currentPath} !");

            GameManager.Instance.WinGameOver(this);
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

    public virtual void AnswerQuestion(bool isCorrect)
    {
        //  later
    }

    // Méthode pour déboguer l'historique des waypoints
    public void DebugWaypointHistory()
    {
        Debug.Log($"📋 Historique des waypoints ({previousWaypoints.Count}/{MAX_WAYPOINT_HISTORY}) - Maximum {MAX_MOVE_BACK_STEPS} pas en arrière à la fois:");

        Stack<WaypointData> tempStack = new Stack<WaypointData>(previousWaypoints);
        int index = 1;

        while (tempStack.Count > 0)
        {
            WaypointData data = tempStack.Pop();
            string marker = index <= MAX_MOVE_BACK_STEPS ? "🔹" : "⚪";
            Debug.Log($"  {marker} {index++}. Path: {data.pathName}, Index: {data.waypointIndex}, Direction: {data.direction}");
        }
    }





    public void SkipTurns(int turns)
    {
        turnsToSkip = turns;
    }

    public bool ShouldSkipTurn()
    {
        return turnsToSkip > 0;
    }

    public void DecrementSkipTurn()
    {
        if (turnsToSkip > 0)
        {
            turnsToSkip--;
        }
    }



}