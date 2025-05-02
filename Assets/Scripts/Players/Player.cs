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

    [Header("Debug Information")]
    [SerializeField] private bool _debug_isEffectMovement = false;
    [SerializeField] private string _debug_movementSource = "None";
    [SerializeField] private int _debug_stepsToMove = 0;
    [SerializeField] private bool _debug_questionShown = false;
    [SerializeField] private bool _debug_landedOnFinalTile = false;
    [SerializeField] private string _debug_landedOnTileType = "None";
    private bool usingPlayerCamera = false;
    public int lives;
    public Profile playerProfile; // Visible in the Inspector

    [Header("🔹 Player Profile Info")]
    [SerializeField] public string debugProfileName; // 🔥 Affiche dans l'Inspector
    public bool HasFinishedMoving => !isMoving && !reachedIntersection;
    [Header("🔹 Landing History")]
    [SerializeField] protected string previousLandingPath; // Visible in Inspector
    [SerializeField] protected int previousLandingWaypointIndex; // Visible in Inspector
    protected int previousLandingDirection;
    [SerializeField] protected bool hasPreviousLanding = false; // Visible in Inspector


    protected virtual void Start()
    {
        if (gameBoard != null)
        {
            MoveToWaypoint(0);
            // Initialiser la position précédente à la position de départ (0)
            previousLandingPath = currentPath;
            previousLandingWaypointIndex = 0;
            previousLandingDirection = movementDirection;
            hasPreviousLanding = true;
            Debug.Log($"🏁 {gameObject.name} → Position initiale enregistrée comme atterrissage précédent: {previousLandingPath}:{previousLandingWaypointIndex}");
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
            GameObject currentWaypoint = GetCurrentWaypoint();
            if (currentWaypoint != null)
            {
                Tile tile = currentWaypoint.GetComponent<Tile>();
                if (tile != null && CameraManager.Instance != null)
                if (tile != null && CameraManager.Instance != null)
                {
                    Camera activeCamera = CameraManager.Instance.GetActiveCamera();
                    if(activeCamera != CameraManager.Instance.mainCamera)
                    {
                    // Immediately switch to the region camera based on current tile
                    CameraManager.Instance.OnPlayerLandedOnTile(this, tile.region);
                    Debug.Log($"🎥 Switching camera to {tile.region} region as player starts moving");
                    }
                }
            }
            CameraManager.Instance.DisableViewToggle();
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
                        // Si le joueur n'a plus qu'un seul pas restant, afficher l'UI d'intersection
                        if (remainingSteps == 1)
                        {
                            reachedIntersection = true;
                            isMoving = false;
                            uiManager.ShowUI(this);
                        }
                        // Si le joueur a encore 2 pas ou plus, continuer automatiquement sur le même chemin
                        else if (remainingSteps >= 2)
                        {
                            Debug.Log($"🔄 Intersection ignorée car remainingSteps = {remainingSteps} > 1. Continuation automatique sur le même chemin.");

                            lastWaypointBeforeIntersection = targetWaypoint;
                            ResumeMovement(null, true);
                        }
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
                            if(GameManager.Instance.isEffectMovement){
                                CameraManager.Instance.SwitchToMainCamera();
                            }
                            CameraManager.Instance.EnableViewToggle();

                            // Important: Update previous landing BEFORE displaying current region
                            Debug.Log($"🎲 {gameObject.name} → Fin du mouvement naturel: " +
                                      $"Actuel={currentPath}:{currentWaypointIndex}, " +
                                      $"Précédent={previousLandingPath}:{previousLandingWaypointIndex} → {previousLandingPath}:{previousLandingWaypointIndex}");

                            // Seule la position actuelle change, pas la précédente!
                            UpdatePreviousLanding();

                            // DEBUGGER: Check if we're at a final tile before calling DisplayCurrentRegion
                            if (currentWaypointIndex >= 50)
                            {
                                Debug.Log($"🔧 DEBUG: Player {gameObject.name} has completed movement and is at final tile (index: {currentWaypointIndex})");
                                _debug_landedOnFinalTile = true;
                                _debug_questionShown = false; // Will be set to true when a question is shown
                            }
                            else
                            {
                                _debug_landedOnFinalTile = false;
                            }

                            DisplayCurrentRegion();

                            // DEBUGGER: Log what happened after DisplayCurrentRegion
                            Debug.Log($"🔧 DEBUG: After DisplayCurrentRegion - Question shown: {_debug_questionShown}, isEffectMovement: {GameManager.Instance.isEffectMovement}");
                        }
                    }
                }
            }
        }
    }


    protected virtual void UpdatePreviousLanding()
    {
        // We're intentionally not changing the previous landing position here
        // We only update it if this is the first time we're recording a landing
        if (!hasPreviousLanding)
        {
            Debug.Log($"📌 {gameObject.name} → Premier atterrissage enregistré: Actuel={currentPath}:{currentWaypointIndex}, Précédent=aucun");
            previousLandingPath = currentPath;
            previousLandingWaypointIndex = currentWaypointIndex;
            previousLandingDirection = movementDirection;
            hasPreviousLanding = true;
        }
        else
        {
            // Log that we're keeping the previous landing the same
            Debug.Log($"📌 {gameObject.name} → Atterrissage actuel: {currentPath}:{currentWaypointIndex}, " +
                      $"Précédent maintenu: {previousLandingPath}:{previousLandingWaypointIndex}");
        }
    }

    // Method to move back to previous landing position
    public virtual void MoveToPreviousAtterrissage()
    {
        if (!hasPreviousLanding)
        {
            Debug.LogWarning($"⚠️ {gameObject.name} → Pas d'atterrissage précédent enregistré!");
            return;
        }

        if (isMoving || reachedIntersection)
        {
            Debug.LogWarning($"⚠️ {gameObject.name} → Impossible de retourner à l'atterrissage précédent pendant un mouvement!");
            return;
        }

        // Vérifier que la position précédente est différente de la position actuelle
        if (previousLandingPath == currentPath && previousLandingWaypointIndex == currentWaypointIndex)
        {
            Debug.LogWarning($"⚠️ {gameObject.name} → Déjà à la position d'atterrissage précédente! ({previousLandingPath}:{previousLandingWaypointIndex})");
            return;
        }

        Debug.Log($"🔙 {gameObject.name} → Téléportation vers l'atterrissage précédent: {previousLandingPath}:{previousLandingWaypointIndex} (position actuelle: {currentPath}:{currentWaypointIndex})");

        // Indiquer que c'est un mouvement de pénalité/effet pour éviter que des questions s'affichent
        GameManager.Instance.isEffectMovement = true;
        _debug_isEffectMovement = true;
        _debug_movementSource = "MoveToPreviousAtterrissage";

        // Swap previous and current positions
        string currentPositionPath = currentPath;
        int currentPositionIndex = currentWaypointIndex;
        int currentPositionDirection = movementDirection;

        // Move to previous landing
        currentPath = previousLandingPath;
        currentWaypointIndex = previousLandingWaypointIndex;
        targetWaypointIndex = currentWaypointIndex;
        movementDirection = previousLandingDirection;

        // Actually move the player
        MoveToWaypoint(currentWaypointIndex);

        // Update previous landing to now be the position we just left
        previousLandingPath = currentPositionPath;
        previousLandingWaypointIndex = currentPositionIndex;
        previousLandingDirection = currentPositionDirection;

        Debug.Log($"🔄 {gameObject.name} → Après téléportation: Actuel = {currentPath}:{currentWaypointIndex}, Précédent = {previousLandingPath}:{previousLandingWaypointIndex}");

        // Display region info for new position
        DisplayCurrentRegion();
    }


public virtual bool CanGiveLife()
{
    return lives >= 3;
}

public void PlayMovementSound(){
    while(isMoving){
        AudioManager.Instance.PlayMovement();
    }
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
        // If this is the first time this player is playing, record their initial position
        if (!hasPreviousLanding)
        {
            previousLandingPath = currentPath;
            previousLandingWaypointIndex = currentWaypointIndex;
            previousLandingDirection = movementDirection;
            hasPreviousLanding = true;
            Debug.Log($"🏁 {gameObject.name} → Position initiale enregistrée comme atterrissage précédent: {currentPath}:{currentWaypointIndex}");
        }
    }

    public virtual void MovePlayer(int steps)
    {
        // DEBUGGER: Track movement request
        _debug_stepsToMove = steps;
        _debug_isEffectMovement = GameManager.Instance.isEffectMovement;
        _debug_movementSource = GameManager.Instance.isEffectMovement ? "EffectMovement" : "NormalMovement";

        Debug.Log($"🔧 DEBUG: MovePlayer called - Player: {gameObject.name}, Steps: {steps}, isEffectMovement: {_debug_isEffectMovement}, currentIndex: {currentWaypointIndex}");
        Debug.Log($"DEBUG: MovePlayer called - Position actuelle: {currentPath}:{currentWaypointIndex}, Position précédente: {previousLandingPath}:{previousLandingWaypointIndex}");

        ///////////////////////////////////////////////////
        // Avant tout nouveau mouvement, enregistrer la position actuelle comme précédente
        if (hasPreviousLanding)
        {
            // SAVE CURRENT POSITION AS PREVIOUS
            previousLandingPath = currentPath;
            previousLandingWaypointIndex = currentWaypointIndex;
            previousLandingDirection = movementDirection;
            Debug.Log($"🔍 {gameObject.name} → Avant de bouger, position actuelle sauvegardée comme précédente: {previousLandingPath}:{previousLandingWaypointIndex}");
        }

        if (!reachedIntersection)
        {
            // DEBUGGER: Critical check for final tile movement
            if (currentWaypointIndex + steps >= 50 && !_debug_landedOnFinalTile)
            {
                Debug.Log($"🔧 CRITICAL DEBUG: Player {gameObject.name} WILL LAND ON FINAL TILE - Current index: {currentWaypointIndex}, Moving: {steps} steps, Final position will be: {currentWaypointIndex + steps}");
            }

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
                // DEBUGGER: Record tile info before processing
                _debug_landedOnTileType = tile.type.ToString();
                bool isFinalTile = (currentWaypointIndex >= 50);

                Debug.Log($"🔧 DEBUG: DisplayCurrentRegion - Player: {gameObject.name}, " +
                          $"Position: {currentWaypointIndex}, TileType: {tile.type}, " +
                          $"IsFinalTile: {isFinalTile}, IsEffectMovement: {GameManager.Instance.isEffectMovement}");

                // Notify the camera manager about the current region
                if (CameraManager.Instance != null)
                {
                    CameraManager.Instance.OnPlayerLandedOnTile(this, tile.region);
                }

                // CRITICAL DEBUGGING: Log whether the tile's OnPlayerLands will be called
                Debug.Log($"🔧 DEBUG: About to call tile.OnPlayerLands() - Effect Movement: {GameManager.Instance.isEffectMovement}, Final Tile: {isFinalTile}");

                tile.OnPlayerLands();

                // DEBUGGING: Check if the question was shown by updating our tracking variable
                // This would need to be updated in QuestionUIManager.ShowUI method 
                // To properly track this situation
                Debug.Log($"🔧 DEBUG: After tile.OnPlayerLands() - Question shown status: {_debug_questionShown}");

                Debug.Log($"🧩 {gameObject.name} se trouve maintenant sur la tuile {tile.pathName}:{tile.position} de type {tile.type} dans la région {tile.region}");
            }

            // Vérifier si c'est un waypoint final
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

    public virtual void AnswerQuestion(bool isCorrect)
    {
        Debug.Log($"🎮 {gameObject.name} a répondu: {(isCorrect ? "Correctement ✓" : "Incorrectement ✗")}");

        _debug_questionShown = true;
    }

    public void CheckAndTriggerWinCondition(bool answeredCorrectly)
    {
        Debug.Log($"🔍 Vérification de victoire pour {gameObject.name}: index={currentWaypointIndex}, isOnFinalTile={isOnFinalTile}, réponseCorrecte={answeredCorrectly}");

        // Vérifier la position finale (index >= 50) ET si la réponse est correcte
        if (currentWaypointIndex >= 50 && answeredCorrectly)
        {
            Debug.Log($"🏆 CONDITIONS DE VICTOIRE REMPLIES: {gameObject.name} a atteint l'index {currentWaypointIndex} et répondu correctement!");

            // On ne fait plus rien ici car la victoire est gérée directement dans GameManager.ApplyQuestionResult
            // Pour éviter les problèmes, on ne fait pas d'appel redondant à WinGameOver
        }
        else if (currentWaypointIndex >= 50 && !answeredCorrectly)
        {
            Debug.Log($"⛔ {gameObject.name} a atteint l'index {currentWaypointIndex} mais n'a pas répondu correctement.");
            // Les pénalités sont appliquées dans GameManager.ApplyQuestionResult
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
            _debug_landedOnFinalTile = true;
            Debug.Log($"🏁 DEBUG: After setting: isOnFinalTile = {isOnFinalTile}");

            // If this is a result of effect movement, also reset the effect flag in GameManager
            // to ensure the question will be shown
            if (GameManager.Instance.isEffectMovement)
            {
                Debug.Log($"🏁 CRITICAL DEBUG: {gameObject.name} a atteint la position finale via un effet de mouvement! isEffectMovement = {GameManager.Instance.isEffectMovement}");
                // We'll let the QuestionTile handle this flag now
            }
        }
        else
        {
            Debug.Log($"🏁 DEBUG: Player not at final position (index {currentWaypointIndex} < 50)");
        }
    }

    public string GetPreviousLandingInfo()
    {
        if (!hasPreviousLanding)
        {
            return "Aucun atterrissage précédent enregistré";
        }

        return $"Précédent atterrissage: {previousLandingPath}:{previousLandingWaypointIndex}";
    }

}