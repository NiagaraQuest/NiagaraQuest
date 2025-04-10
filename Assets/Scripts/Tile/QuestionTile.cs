using System;
using UnityEngine;

public class QuestionTile : Tile
{
    [Header("Question Settings")]
    public string category = "General";
    public Difficulty questionDifficulty = Difficulty.Medium;
    
    // 0 = Random, 1 = Open, 2 = QCM
    [Range(0, 2)]
    public int questionTypePreference = 0;
    
    private QuestionUIManager uiManager;
    private bool isProcessingQuestion = false;
    private bool questionAnswered = false;
    private bool isCorrect = false;

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
        
        // 🔥 Trouver le UI Manager avant de l'utiliser
        uiManager = FindFirstObjectByType<QuestionUIManager>();

        if (uiManager == null)
        {
            Debug.LogWarning("⚠️ QuestionUIManager introuvable ! Le jeu continue sans question.");
            isProcessingQuestion = false;
            ContinueGame();
            return;
        }

        Question question;
        
        // Choix du type de question en fonction de la préférence
        if (questionTypePreference == 1 || (questionTypePreference == 0 && UnityEngine.Random.Range(0, 2) == 0))
        {
            // Créer une question ouverte de test
            question = new OpenQuestion
            {
                Category = category,
                Qst = "What is the capital of France?",
                Answer = "Paris",
                Difficulty = questionDifficulty.ToString()
            };
        }
        else
        {
            // Créer une question QCM de test
            question = new QCMQuestion
            {
                Category = category,
                Qst = "Which is the capital of France?",
                Choices = new string[] { "Lyon", "Paris", "Marseille", "Lille" },
                CorrectChoice = 1,
                Difficulty = questionDifficulty.ToString()
            };
        }

        Debug.Log($"📢 Question posée : {question.Qst} (Difficulté: {questionDifficulty})");
        uiManager.ShowUI(question, this);
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
        
        // Appliquer les récompenses/pénalités en fonction de la réponse
        if (questionAnswered)
        {
            GameManager.Instance.ApplyQuestionResult(currentPlayer, isCorrect, questionDifficulty);
            questionAnswered = false;
        }
        
        // Vérifier si le joueur a encore des vies
        if (currentPlayer.lives <= 0)
        {
            Debug.Log($"💀 {currentPlayer.gameObject.name} n'a plus de vies !");
            // Le GameManager gère déjà l'élimination des joueurs dans ApplyQuestionResult
        }
    }
    
    // Cette méthode est appelée par le QuestionUIManager lorsque le joueur a répondu
    public void OnQuestionAnswered(bool correct)
    {
        questionAnswered = true;
        isCorrect = correct;
        
        // La méthode ContinueGame sera appelée par le QuestionUIManager
    }
}