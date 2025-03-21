using UnityEngine;

public class QuestionTile : Tile
{
    public string question;
    public string answer;

    public QuestionType questionType;
    public Difficulty difficulty;

    public override void OnPlayerLands()
    {
        base.OnPlayerLands(); // ✅ Appelle l'affichage de la région depuis Tile

        Debug.Log($"📝 Question: {question} (Catégorie: {questionType}, Difficulté: {difficulty})");

        // Affichage UI
        ShowQuestionUI();
    }

    private void ShowQuestionUI()
    {
        Debug.Log($"📢 Affichage UI: {question}");
    }
}
