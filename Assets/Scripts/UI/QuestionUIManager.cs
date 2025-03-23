using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionUIManager : MonoBehaviour
{
    public GameObject questionPanel; 
    public TextMeshProUGUI questionText; 
    public TMP_InputField answerInput;
    public Button submitButton;
    public GameObject resultPanel; 
    public TextMeshProUGUI resultText; 
    public Button exitButton;

    private OpenQuestion currentQuestion;
    private QuestionTile currentTile;

    void Start()
    {
        // Hide panels
        questionPanel.SetActive(false);
        resultPanel.SetActive(false);

        // Assign button listeners
        submitButton.onClick.AddListener(CheckAnswer);
        exitButton.onClick.AddListener(CloseResultPanel);
    }

    public void ShowUI(OpenQuestion question, QuestionTile tile)
    {
        currentQuestion = question;
        currentTile = tile;

        questionText.text = question.Qst; // Display question
        answerInput.text = ""; 
        questionPanel.SetActive(true); // Show question panel
    }

    void CheckAnswer()
    {
        string playerAnswer = answerInput.text.Trim().ToLower();
        string correctAnswer = currentQuestion.Answer.Trim().ToLower();

        // Show result panel
        resultPanel.SetActive(true);
        resultText.text = (playerAnswer == correctAnswer) ? "✅ Correct!" : "❌ Wrong!";
        
        questionPanel.SetActive(false); // Hide question panel
    }

    void CloseResultPanel()
    {
        resultPanel.SetActive(false);
        currentTile.ContinueGame(); // Allow the player to resume
    }
}
