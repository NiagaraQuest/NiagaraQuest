using System;
using UnityEngine;

public class QuestionTile : Tile
{
    [Header("Question")]
    public Question question;
    
    // 0 = Random, 1 = Open, 2 = QCM
    [Range(0, 2)]
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

        // Generate a question if one isn't already assigned
        if (question == null)
        {
            GenerateDefaultQuestion();
        }

        Debug.Log($"📢 Question posée : {question.Qst} (Difficulté: {question.Difficulty})");
        uiManager.ShowUI(question, this);
    }
    
    private void GenerateDefaultQuestion()
    {
        // Choix du type de question en fonction de la préférence
        if (questionTypePreference == 1 || (questionTypePreference == 0 && UnityEngine.Random.Range(0, 2) == 0))
        {
            // Créer une question ouverte de test
            question = new OpenQuestion
            {
                Category = "General",
                Qst = "What is the capital of France?",
                Answer = "Paris",
                Difficulty = Tile.Difficulty.Hard
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
                Difficulty = Tile.Difficulty.Hard
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
}