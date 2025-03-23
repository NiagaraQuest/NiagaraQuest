using UnityEngine;


public class QuestionTile : Tile
{
    public OpenQuestion question;
    private QuestionUIManager uiManager;

    void Start()
    {
        uiManager = FindFirstObjectByType<QuestionUIManager>(); // Find UI manager in scene
    }

    public override void OnPlayerLands()
    {
        base.OnPlayerLands(); 
        AskQuestion();
    }

    private void AskQuestion()
    {
        question = new OpenQuestion{
            Category = "General",
            Qst = "What is the capital of France?",
            Answer = "Paris",
        };
        uiManager.ShowUI(question, this);
    }

    public void ContinueGame()
    {
        Debug.Log("✅ Player continues the game...");
        
    }
}
