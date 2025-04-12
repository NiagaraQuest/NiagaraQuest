using System;
using UnityEngine;

public class QuestionTile : Tile
{

   
    public OpenQuestion question;
    private QuestionUIManager uiManager;

    public override void OnPlayerLands()
    {
        base.OnPlayerLands();
        AskQuestion();
       
    }

    private void AskQuestion()
    {


        // Vérifier si c'est un mouvement de récompense
        if (GameManager.Instance.isRewardMovement)
        {
            Debug.Log("🎁 Mouvement de récompense - pas de nouvelle question!");
            GameManager.Instance.isRewardMovement = false;  // Réinitialiser pour le prochain tour
            return;  // Ne pas poser de question
        }

        // 🔥 Always find the UI Manager before using it
        uiManager = FindFirstObjectByType<QuestionUIManager>();

        if (uiManager == null)
        {
            Debug.LogWarning("⚠️ QuestionUIManager introuvable ! Le jeu continue sans question.");
            return;
        }

        question = new OpenQuestion
        {
            Category = "General",
            Qst = "What is the capital of France?",
            Answer = "Paris",
            Difficulty = "Easy"

        };

        Debug.Log($"📢 Question posée : {question.Qst}");
        uiManager.ShowUI(question, this);
    }

    public void ContinueGame()
    {
        Debug.Log("✅ Player continues the game...");
    }

    

}


