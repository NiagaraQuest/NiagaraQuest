using System;
using UnityEngine;
using System.Threading.Tasks;

public class QuestionTile : Tile
{
    [Header("Question")]
    public Question question;

    // 0 = Random, 1 = Open, 2 = QCM, 3 = True/False
    [Range(0, 3)]
    public int questionTypePreference = 0;

    private QuestionUIManager uiManager;
    private bool isProcessingQuestion = false;

    // DEBUGGER VARIABLES
    [Header("Debug Information")]
    [SerializeField] private bool _debug_questionAsked = false;
    [SerializeField] private bool _debug_questionSkipped = false;
    [SerializeField] private bool _debug_wasFinalTile = false;
    [SerializeField] private string _debug_effectOrigin = "None";
    [SerializeField] private int _debug_playerPosition = -1;

    public override void OnPlayerLands()
    {
        // Sauvegarder l'état original du flag isEffectMovement AVANT d'appeler la méthode de base
        bool originalEffectMovement = GameManager.Instance.isEffectMovement;

        // IMPORTANT: Appeler la méthode de base qui va détecter le joueur et définir currentQuestionPlayer
        base.OnPlayerLands();

        // Maintenant récupérer le joueur que OnPlayerLands a identifié
        Player player = GameManager.Instance.GetCurrentPlayer();

        // Si aucun joueur n'est détecté, on ne peut pas poser de question
        if (player == null)
        {
            Debug.LogError("❌ Aucun joueur détecté sur la tuile question!");
            return;
        }

        // Vérifier si c'est une case finale
        bool isFinalTile = (player.currentWaypointIndex >= 50);

        // GESTION SPÉCIALE DES CASES FINALES
        if (isFinalTile && originalEffectMovement)
        {
            Debug.Log($"🔧 CRITIQUE: {player.gameObject.name} est sur une case finale via un mouvement d'effet - FORÇAGE de la question!");

            // Désactiver temporairement le flag pour s'assurer que la question s'affiche
            GameManager.Instance.isEffectMovement = false;

            // Poser la question
            AskQuestion();

            // Restaurer le flag original
            GameManager.Instance.isEffectMovement = originalEffectMovement;
        }
        else
        {
            // Sinon, procéder normalement
            AskQuestion();
        }
    }


    // Amélioration de AskQuestion pour mieux gérer les mouvements par effet
    private void AskQuestion()
    {
        if (isProcessingQuestion)
        {
            Debug.LogWarning("⚠️ Déjà en train de traiter une question, ignoré");
            return;
        }

        isProcessingQuestion = true;

        // Récupérer le joueur actuel (défini par OnPlayerLands)
        Player player = GameManager.Instance.GetCurrentPlayer();
        if (player == null)
        {
            Debug.LogError("❌ Aucun joueur trouvé dans GetCurrentPlayer()!");
            isProcessingQuestion = false;
            return;
        }

        bool isFinalTile = (player.currentWaypointIndex >= 50);

        // Pour les cases normales (non finales) avec mouvement d'effet, ne pas poser de question
        if (GameManager.Instance.isEffectMovement && !isFinalTile)
        {
            Debug.Log($"🎁 {player.gameObject.name}: Mouvement de récompense sur case normale - pas de question.");
            GameManager.Instance.isEffectMovement = false;  // Réinitialiser pour le prochain tour
            isProcessingQuestion = false;
            return;
        }

        // Pour toutes les autres situations (incluant les cases finales), poser une question

        // Trouver le UI Manager
        uiManager = FindFirstObjectByType<QuestionUIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("⚠️ QuestionUIManager introuvable! Impossible de poser une question.");
            isProcessingQuestion = false;
            ContinueGame();
            return;
        }

        // Vérifier le profil du joueur
        if (player.playerProfile == null)
        {
            Debug.LogError($"❌ Pas de profil trouvé pour {player.gameObject.name}! Question par défaut.");
            GenerateDefaultQuestion();
            ShowQuestion();
            return;
        }

        // Récupérer une question
        FetchQuestionFromDatabase(player);
    }


    private async void FetchQuestionFromDatabase(Player currentPlayer)
    {
        try
        {
            Debug.Log($"🔄 Fetching question from database for player: {currentPlayer.playerProfile.Username} (ID: {currentPlayer.playerProfile.Id})");

            // Ensure QuestionManager is initialized
            await QuestionManager.Instance.Initialize();

            // Check if player profile has valid ID
            if (currentPlayer.playerProfile.Id <= 0)
            {
                Debug.LogWarning("⚠️ Player profile ID is invalid (ID: 0). The profile may not be saved in the database.");
                GenerateDefaultQuestion();
                ShowQuestion();
                return;
            }

            // Use the QuestionManager to generate a question for the player
            question = await QuestionManager.Instance.GenerateQuestionForPlayer(currentPlayer.playerProfile);

            if (question == null)
            {
                Debug.LogWarning("⚠️ Failed to fetch question from database. Using default question.");
                // Check if database has any questions at all
                var qcmCount = await DatabaseManager.Instance.CountAsync<QCMQuestion>();
                var openCount = await DatabaseManager.Instance.CountAsync<OpenQuestion>();
                var tfCount = await DatabaseManager.Instance.CountAsync<TrueFalseQuestion>();
                Debug.LogWarning($"⚠️ Database question counts: QCM={qcmCount}, Open={openCount}, TF={tfCount}");

                GenerateDefaultQuestion();
            }
            else
            {
                Debug.Log($"✅ Fetched question from database: {question.Id} - {question.GetType().Name}");
            }

            // Show the question UI
            ShowQuestion();
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error fetching question: {ex.Message}\nStack trace: {ex.StackTrace}");
            GenerateDefaultQuestion();
            ShowQuestion();
        }
    }

    private void ShowQuestion()
    {
        if (question != null)
        {
            // DEBUGGING: Track that question is shown
            _debug_questionAsked = true;
            _debug_questionSkipped = false;

            Debug.Log($"📢 Question posée : {question.Qst} (Difficulté: {question.Difficulty})");

            // CRITICAL DEBUGGING: Log this important event
            Player player = GameManager.Instance.GetCurrentPlayer();
            bool isFinalTile = (player != null && player.currentWaypointIndex >= 50);
            Debug.Log($"🔧 DEBUG ShowQuestion: Player: {(player != null ? player.gameObject.name : "null")}, " +
                      $"Position: {(player != null ? player.currentWaypointIndex : -1)}, IsFinalTile: {isFinalTile}, " +
                      $"IsEffectMovement: {GameManager.Instance.isEffectMovement}, QuestionType: {question.GetType().Name}");

            // Pass this QuestionTile instance to the UI manager so it can call back when answered
            uiManager.ShowUI(question, this);
        }
        else
        {
            Debug.LogError("❌ No question available to show!");
            isProcessingQuestion = false;
            ContinueGame();
        }
    }

    private void GenerateDefaultQuestion()
    {
        Debug.Log("⚠️ Using default question as fallback");

        // Choix du type de question en fonction de la préférence
        if (questionTypePreference == 1 || (questionTypePreference == 0 && UnityEngine.Random.Range(0, 2) == 0))
        {
            // Créer une question ouverte de test
            question = new OpenQuestion
            {
                Category = "General",
                Qst = "What is the capital of France?",
                Answer = "Paris",
                Difficulty = "EASY"
            };
        }
        else if (questionTypePreference == 3)
        {
            // Create True/False question
            question = new TrueFalseQuestion
            {
                Category = "General",
                Qst = "Paris is the capital of France.",
                IsTrue = true,
                Difficulty = "EASY"
            };
        }
        else
        {
            // Créer une question QCM de test
            question = new QCMQuestion
            {
                Category = "General",
                Qst = "Which is the capital of France?",
                Choices = new string[] { "Lyon", "Paris", "Marseille", "Lille" },
                CorrectChoice = 1,
                Difficulty = "EASY"
            };
        }
    }

    public void ContinueGame()
    {
        Debug.Log("✅ Player continues the game...");
        isProcessingQuestion = false;

        // Récupérer le joueur actuel
        Player currentPlayer = GameManager.Instance.GetCurrentPlayer();

        if (currentPlayer == null)
        {
            Debug.LogError("❌ No current player found in GameManager!");
            return;
        }
    }

    // This method should be called by the QuestionUIManager when a player answers
    public async void OnQuestionAnswered(bool isCorrect)
    {
        Debug.Log($"🎮 Player answered: {(isCorrect ? "Correctly ✅" : "Incorrectly ❌")}");

        // Get the current player
        Player currentPlayer = GameManager.Instance.GetCurrentPlayer();
        if (currentPlayer == null || currentPlayer.playerProfile == null || question == null)
        {
            Debug.LogError("❌ Cannot record answer: Missing player profile or question");
            ApplyGameEffects(currentPlayer, isCorrect);
            return;
        }

        // Vérifier si c'est une case finale
        bool isFinalTile = (currentPlayer != null && currentPlayer.currentWaypointIndex >= 50);

        // DEBUGGING: Log answer event
        Debug.Log($"🔧 DEBUG OnQuestionAnswered: Player: {currentPlayer.gameObject.name}, " +
                  $"Position: {currentPlayer.currentWaypointIndex}, IsFinalTile: {isFinalTile}, " +
                  $"IsCorrect: {isCorrect}, Difficulty: {question.Difficulty}");

        try
        {
            // Log initial ELO values
            int initialPlayerElo = currentPlayer.playerProfile.Elo;
            int initialQuestionElo = question.Elo;

            Debug.Log($"⚖️ Before ELO update - Player: {initialPlayerElo}, Question: {initialQuestionElo}");

            // Record the answer using QuestionManager - this will update ELO ratings
            await QuestionManager.Instance.RecordPlayerAnswer(currentPlayer.playerProfile, question, isCorrect);

            // Log the ELO changes
            int playerEloChange = currentPlayer.playerProfile.Elo - initialPlayerElo;
            int questionEloChange = question.Elo - initialQuestionElo;

            Debug.Log($"⚖️ After ELO update - Player: {currentPlayer.playerProfile.Elo} ({(playerEloChange >= 0 ? "+" : "")}{playerEloChange}), " +
                     $"Question: {question.Elo} ({(questionEloChange >= 0 ? "+" : "")}{questionEloChange})");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error recording answer in database: {ex.Message}");
        }


        // Cas spécial: Réponse correcte sur case finale - traiter directement par Player.CheckAndTriggerWinCondition
        if (isCorrect && isFinalTile)
        {
            Debug.Log("🏆 Réponse correcte sur case finale - déclencher la victoire directement!");
            currentPlayer.CheckAndTriggerWinCondition(true);
            return; // Ne pas appliquer les effets normaux de la question
        }

        // Pour tous les autres cas, appliquer les effets normaux
        ApplyGameEffects(currentPlayer, isCorrect);
    }

    // Apply game effects based on the answer
    private void ApplyGameEffects(Player player, bool isCorrect)
    {
        if (player == null) return;

        // Apply game effects based on question difficulty and correctness
        GameManager.Instance.ApplyQuestionResult(player, isCorrect, question.Difficulty);
    }

    //lyna
    public void SkipQuestion()
    {
        Debug.Log("🔀 Le joueur a choisi de passer la question.");
        _debug_questionSkipped = true;
    }
}