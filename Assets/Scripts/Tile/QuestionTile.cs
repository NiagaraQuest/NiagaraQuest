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

    public override void OnPlayerLands()
    {
        base.OnPlayerLands();
        
        // Set current player in the GameManager
        Player currentPlayer = GameManager.Instance.selectedPlayer.GetComponent<Player>();
        GameManager.Instance.SetCurrentQuestionPlayer(currentPlayer);
        
        AskQuestion();
    }

    private void AskQuestion()
    {
        if (isProcessingQuestion)
        {
            Debug.LogWarning("⚠️ Déjà en train de traiter une question, ignoré");
            return;
        }
        
        isProcessingQuestion = true;

        if (GameManager.Instance.isEffectMovement)
        {
            Debug.Log("🎁 Mouvement de récompense - pas de nouvelle question!");
            GameManager.Instance.isEffectMovement = false;  // Réinitialiser pour le prochain tour
            isProcessingQuestion = false;
            return;  // Ne pas poser de question
        }
        
        // 🔥 Trouver le UI Manager avant de l'utiliser
        uiManager = FindFirstObjectByType<QuestionUIManager>();

        if (uiManager == null)
        {
            Debug.LogWarning("⚠️ QuestionUIManager introuvable ! Le jeu continue sans question.");
            isProcessingQuestion = false;
            ContinueGame();
            return;
        }

        // Get the current player
        Player currentPlayer = GameManager.Instance.GetCurrentPlayer();
        if (currentPlayer == null || currentPlayer.playerProfile == null)
        {
            Debug.LogError("❌ No player profile found! Using default question.");
            GenerateDefaultQuestion();
            ShowQuestion();
            return;
        }

        // Fetch a question from the database asynchronously
        FetchQuestionFromDatabase(currentPlayer);
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
            Debug.Log($"📢 Question posée : {question.Qst} (Difficulté: {question.Difficulty})");
            
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
            
            // Even if database update fails, still apply game effects
            ApplyGameEffects(currentPlayer, isCorrect);
            return;
        }
        
        // Apply game effects based on the answer
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

    }
}