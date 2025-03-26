using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionUIManager : MonoBehaviour
{
    public static QuestionUIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }







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

        bool isCorrect = (playerAnswer == correctAnswer);

        // Show result panel
        resultPanel.SetActive(true);
        resultText.text = isCorrect ? "✅ Correct!" : "❌ Wrong!";

        questionPanel.SetActive(false); // Hide question panel

        // ✅ Apply the effect in GameManager (assuming difficulty is Easy)
        GameManager.Instance.ApplyQuestionResult(GameManager.Instance.GetCurrentPlayer(), isCorrect , Tile.Difficulty.Easy);
    }


    void CloseResultPanel()
    {
        resultPanel.SetActive(false);
        questionPanel.SetActive(false);  // Ensure it is closed
       // currentTile.penalityAndReward(resultText.text.Contains("✅")); // Apply the penalty/reward
        //currentTile.ContinueGame(); // Resume game
    }
}