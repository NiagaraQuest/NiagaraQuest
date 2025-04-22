using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

    public Board gameBoard;
    public IntersectionUIManager uiManager;
    public string currentPath;
    public float speed = 2;
    public int currentWaypointIndex = 0;
    public int targetWaypointIndex = 0;
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
    private bool usingPlayerCamera = false;
    public int lives = 4; // ✅ Starts with 4 lives
    public Profile playerProfile; // Visible in the Inspector

    [Header("🔹 Player Profile Info")]
    [SerializeField] public string debugProfileName; // 🔥 Affiche dans l'Inspector
    public bool HasFinishedMoving => !isMoving && !reachedIntersection;


    protected string lastLandingPath;
    protected int lastLandingIndex;
    protected int lastLandingDirection;


    protected string previousLandingPath;
    protected int previousLandingIndex;
    protected int previousLandingDirection;

    // Structure pour stocker les données d'un waypoint avec sa direction
    protected struct WaypointData
    {
        public string pathName;
        public int waypointIndex;
        public int direction;  // Direction du mouvement pour ce waypoint
        public bool isLandingPoint;  // Indique si c'est un point d'atterrissage

        public WaypointData(string path, int index, int dir, bool isLanding = false)
        {
            pathName = path;
            waypointIndex = index;
            direction = dir;
            isLandingPoint = isLanding;
        }
    }

    protected virtual void Start()
    {
        if (gameBoard != null)
        {
            MoveToWaypoint(0);
            DisplayCurrentRegion();

            lastLandingPath = currentPath;
            lastLandingIndex = currentWaypointIndex;
            lastLandingDirection = movementDirection;

            previousLandingPath = lastLandingPath;
            previousLandingIndex = lastLandingIndex;
            previousLandingDirection = lastLandingDirection;

            Debug.Log($"🔰 Positions d'atterrissage initiales: current={currentPath}-{currentWaypointIndex}, last={lastLandingPath}-{lastLandingIndex}, previous={previousLandingPath}-{previousLandingIndex}");
        }


        if (playerProfile != null && !string.IsNullOrEmpty(playerProfile.Username))
        {
            Debug.Log($"👤 {gameObject.name} → Profil après assignation : {playerProfile.Username}");
        }
        else
        {
            Debug.LogWarning($"⚠️ {gameObject.name} → Profil encore vide. Assignation en attente...");
            StartCoroutine(WaitForProfileAssignment());
        }

        uiManager = FindObjectOfType<IntersectionUIManager>();
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
        GameManager.Instance.CheckPlayerLives();
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
                    Debug.Log($"📍 Waypoint Atteint: {currentWaypointIndex}, isMovingBack: {isMovingBack}");
                    Debug.Log($"📍 Waypoint Atteint: {currentWaypointIndex}");
                    if (!isMovingBack && !targetWaypoint.CompareTag("Intersection"))
                    {
                        StoreWaypointInHistory();
                    }

                    if (targetWaypoint.CompareTag("Intersection"))
                    {
                        reachedIntersection = true;
                        isMoving = false;
                        
                        // Switch back to main camera when reaching intersection
                        if (usingPlayerCamera && CameraManager.Instance != null)
                        {
                            CameraManager.Instance.SwitchToMainCamera();
                            usingPlayerCamera = false;
                        }
                        
                        uiManager.ShowUI(this);
                    }
                    else
                    {
                        lastWaypointBeforeIntersection = targetWaypoint;
                        remainingSteps--;

                        if (remainingSteps > 0)
                        {
                            targetWaypointIndex += movementDirection;
                            // ✅ Vérification encore si on dépasse -1
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
                            isMovingBack = false;
                            
                            // Switch back to main camera when player stops moving
                            if (usingPlayerCamera && CameraManager.Instance != null)
                            {
                                CameraManager.Instance.SwitchToMainCamera();
                                usingPlayerCamera = false;
                            }
                            
                            DisplayCurrentRegion();
                        }
                    }
                }
            }
        }
    }

    protected virtual void StoreWaypointInHistory(bool isLanding = false)
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

        // Ajouter le waypoint actuel dans la pile avec la direction courante et le statut d'atterrissage
        previousWaypoints.Push(new WaypointData(currentPath, currentWaypointIndex, movementDirection, isLanding));
        Debug.Log($"🔄 Waypoint stocké: {currentPath} - {currentWaypointIndex} (direction: {movementDirection}, atterrissage: {isLanding}). Total stocké: {previousWaypoints.Count}/{MAX_WAYPOINT_HISTORY}");
    }

        public virtual void RegisterLandingPosition()
    {
        // L'ancienne position devient la précédente
        previousLandingPath = lastLandingPath;
        previousLandingIndex = lastLandingIndex;
        previousLandingDirection = lastLandingDirection;

        // Mettre à jour la position d'atterrissage actuelle
        lastLandingPath = currentPath;
        lastLandingIndex = currentWaypointIndex;
        lastLandingDirection = movementDirection;

        Debug.Log($"📌 Nouveau point d'atterrissage: {lastLandingPath} - {lastLandingIndex}");
        Debug.Log($"📌 Point d'atterrissage précédent: {previousLandingPath} - {previousLandingIndex}");

        // On stocke aussi dans la pile pour la cohérence et le débogage
        StoreWaypointInHistory(true);
    }


    public virtual bool CanGiveLife()
    {
        return lives >= 3;
    }

    public virtual void GiveLifeTo(Player targetPlayer)
    {
        if (!CanGiveLife())
        {
            Debug.LogWarning($"⚠️ {gameObject.name} cannot give a life (only has {lives} left, needs at least 3)");
            return;
        }

        if (targetPlayer == null)
        {
            Debug.LogError("❌ Target player is null!");
            return;
        }
        
        if (targetPlayer.lives != 1)
        {
            Debug.LogWarning($"⚠️ Cannot give life to {targetPlayer.gameObject.name} - they must have exactly 1 life (current: {targetPlayer.lives})");
            return;
        }
        lives--;
        targetPlayer.GainLife();
        
        Debug.Log($"❤️ {gameObject.name} gave a life to {targetPlayer.gameObject.name}! " +
                $"{gameObject.name} now has {lives} lives, {targetPlayer.gameObject.name} has {targetPlayer.lives} lives.");
    }


    public virtual void StartTurn()
    {
        // Si c'est la première fois que ce joueur joue, stocker sa position initiale
        if (!hasStoredInitialWaypoint)
        {
            // Le point initial est aussi un point d'atterrissage valide
            lastLandingPath = currentPath;
            lastLandingIndex = currentWaypointIndex;
            lastLandingDirection = movementDirection;

            // Au début, l'avant-dernière position est identique à la dernière
            previousLandingPath = lastLandingPath;
            previousLandingIndex = lastLandingIndex;
            previousLandingDirection = lastLandingDirection;

            Debug.Log($"🔰 Positions d'atterrissage initiales dans StartTurn: current={currentPath}-{currentWaypointIndex}, last={lastLandingPath}-{lastLandingIndex}, previous={previousLandingPath}-{previousLandingIndex}");

            StoreWaypointInHistory(true);
            hasStoredInitialWaypoint = true;
            Debug.Log($"🏁 {gameObject.name} → Waypoint initial stocké: {currentPath} - {currentWaypointIndex} (direction: {movementDirection})");
        }
    }

    public virtual void MovePlayer(int steps)
    {
        Debug.Log($"DEBUG: MovePlayer called - isMovingBack before: {isMovingBack}");
        if (!hasStoredInitialWaypoint)
        {
            // Le point initial est aussi un point d'atterrissage valide
            lastLandingPath = currentPath;
            lastLandingIndex = currentWaypointIndex;
            lastLandingDirection = movementDirection;

            // Au début, l'avant-dernière position est identique à la dernière
            previousLandingPath = lastLandingPath;
            previousLandingIndex = lastLandingIndex;
            previousLandingDirection = lastLandingDirection;

            Debug.Log($"🔰 Positions d'atterrissage initiales dans MovePlayer: current={currentPath}-{currentWaypointIndex}, last={lastLandingPath}-{lastLandingIndex}, previous={previousLandingPath}-{previousLandingIndex}");

            StoreWaypointInHistory(true);
            hasStoredInitialWaypoint = true;
            Debug.Log($"🏁 {gameObject.name} → Waypoint initial stocké: {currentPath} - {currentWaypointIndex} (direction: {movementDirection})");
        }

        if (!reachedIntersection)
        {
            remainingSteps = steps;
            targetWaypointIndex = currentWaypointIndex + movementDirection;
            isMoving = true;

            // Switch to player camera when starting movement
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.SwitchToPlayerCamera(this);
                usingPlayerCamera = true;
            }
        }
    }

    public virtual void MovePlayerBack()
    {
        Debug.Log($"DEBUG: MovePlayerBack called - isMovingBack before: {isMovingBack}");
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
            return; // Ajouter cette ligne pour arrêter l'exécution actuelle
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

        // If continuing movement, switch to player camera
        if (isMoving && CameraManager.Instance != null)
        {
            CameraManager.Instance.SwitchToPlayerCamera(this);
            usingPlayerCamera = true;
        }
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
            CheckForWinCondition(currentWaypoint);
        }
    }

    [Header("")]
    public bool isOnFinalTile = false;

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
    /*
    public void SkipTurns(int turns)
    {
        TurnsToSkip += turns;
        Debug.Log($"⏳ {gameObject.name} doit sauter {turns} tour(s). Total à sauter : {TurnsToSkip}");
    }
    */
    public virtual void AnswerQuestion(bool isCorrect)
    {
        Debug.Log($"🎮 {gameObject.name} a répondu: {(isCorrect ? "Correctement ✓" : "Incorrectement ✗")}");

        // Vérifier si c'est une question finale (joueur en position >= 50)
        CheckAndTriggerWinCondition(isCorrect);
    }

    public void CheckAndTriggerWinCondition(bool answeredCorrectly)
    {
        Debug.Log($"🔍 Vérification de victoire pour {gameObject.name}: index={currentWaypointIndex}, isOnFinalTile={isOnFinalTile}, réponseCorrecte={answeredCorrectly}");

        // Vérifier la position finale (index >= 50) ET si la réponse est correcte
        if (currentWaypointIndex >= 50 && answeredCorrectly)
        {
            Debug.Log($"🏆 CONDITIONS DE VICTOIRE REMPLIES: {gameObject.name} a atteint l'index {currentWaypointIndex} et répondu correctement!");
            // Appeler la méthode de victoire du GameManager
            GameManager.Instance.WinGameOver(this);
        }
        else if (currentWaypointIndex >= 50 && !answeredCorrectly)
        {
            Debug.Log($"⛔ {gameObject.name} a atteint l'index {currentWaypointIndex} mais n'a pas répondu correctement. Attente d'une prochaine tentative.");
            MoveToPreviousPosition();
        }
    }


    public void DebugWaypointHistory()
    {
        Debug.Log($"📋 Historique des waypoints ({previousWaypoints.Count}/{MAX_WAYPOINT_HISTORY}) - Maximum {MAX_MOVE_BACK_STEPS} pas en arrière à la fois:");

        Stack<WaypointData> tempStack = new Stack<WaypointData>(previousWaypoints);
        int index = 1;

        while (tempStack.Count > 0)
        {
            WaypointData data = tempStack.Pop();
            string marker = index <= MAX_MOVE_BACK_STEPS ? "🔹" : "⚪";
            string landingIcon = data.isLandingPoint ? "🛬" : "  ";
            Debug.Log($"  {marker} {index++}. Path: {data.pathName}, Index: {data.waypointIndex}, Direction: {data.direction} {landingIcon}");
        }

        // Aussi afficher les points d'atterrissage stockés
        Debug.Log($"🛬 Points d'atterrissage actuels:");
        Debug.Log($"  ↪️ Dernier: {lastLandingPath}-{lastLandingIndex} (direction: {lastLandingDirection})");
        Debug.Log($"  ↪️ Avant-dernier: {previousLandingPath}-{previousLandingIndex} (direction: {previousLandingDirection})");
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

    protected virtual void CheckForWinCondition(GameObject waypoint)
    {
        Debug.Log($"🏁 DEBUG: CheckForWinCondition called for {gameObject.name} at index {currentWaypointIndex} on {currentPath}");

        // Print current isOnFinalTile value
        Debug.Log($"🏁 DEBUG: Before check: isOnFinalTile = {isOnFinalTile}");

        // Vérifier si le joueur a atteint l'index 50 ou plus
        if (currentWaypointIndex >= 50)
        {
            Debug.Log($"🏁 {gameObject.name} a atteint l'index {currentWaypointIndex} sur {currentPath} ! Question finale requise.");

            // Set isOnFinalTile flag
            Debug.Log($"🏁 DEBUG: Setting isOnFinalTile from {isOnFinalTile} to true");
            isOnFinalTile = true;
            Debug.Log($"🏁 DEBUG: After setting: isOnFinalTile = {isOnFinalTile}");
        }
        else
        {
            Debug.Log($"🏁 DEBUG: Player not at final position (index {currentWaypointIndex} < 50)");
        }
    }

        public virtual void MoveToPreviousPosition()
    {
        if (isMoving)
        {
            Debug.LogWarning("⚠️ Impossible de revenir à la position précédente pendant un mouvement");
            return;
        }

        // Utiliser l'avant-dernière position d'atterrissage
        if (string.IsNullOrEmpty(previousLandingPath))
        {
            Debug.LogWarning("⚠️ Aucune position d'atterrissage précédente enregistrée");
            return;
        }

        Debug.Log($"🔄 AVANT déplacement - Position actuelle: {currentPath}-{currentWaypointIndex}");
        Debug.Log($"🔄 AVANT déplacement - Dernière position d'atterrissage: {lastLandingPath}-{lastLandingIndex}");
        Debug.Log($"🔄 AVANT déplacement - Avant-dernière position d'atterrissage: {previousLandingPath}-{previousLandingIndex}");

        // Se déplacer vers le point d'atterrissage précédent
        currentPath = previousLandingPath;
        currentWaypointIndex = previousLandingIndex;
        targetWaypointIndex = currentWaypointIndex;
        movementDirection = previousLandingDirection;

        // Déplacer le joueur instantanément
        MoveToWaypoint(currentWaypointIndex);
        Debug.Log($"↩️ RETOUR à l'avant-dernière position d'atterrissage: {currentPath} - {currentWaypointIndex} (direction: {movementDirection})");

        // Réinitialiser les états
        isMoving = false;
        isMovingBack = false;
        reachedIntersection = false;


        DisplayCurrentRegion();
    }
}