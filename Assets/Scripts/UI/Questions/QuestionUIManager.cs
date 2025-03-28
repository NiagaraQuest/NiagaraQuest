using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionUIManager : MonoBehaviour
{
    public GameObject openQuestionPanel;
    public TMP_InputField answerInput;
    public Button submitButton;

    public GameObject qcmQuestionPanel;
    public TextMeshProUGUI qcmQuestionText;
    public Button[] choiceButtons;

    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button exitButton;

    private QuestionTile currentTile;
    private Question currentQuestion;

    void Start()
    {
        openQuestionPanel.SetActive(false);
        qcmQuestionPanel.SetActive(false);
        resultPanel.SetActive(false);

        submitButton.onClick.AddListener(CheckOpenAnswer);
        exitButton.onClick.AddListener(CloseResultPanel);
    }

    public void ShowUI(Question question, QuestionTile tile)
    {
        currentTile = tile;
        currentQuestion = question;
        if (question is OpenQuestion openQuestion)
        {
            ShowOpenQuestion(openQuestion);
        }
        else if (question is QCMQuestion qcmQuestion)
        {
            ShowQCMQuestion(qcmQuestion);
        }

    }

    private void ShowOpenQuestion(OpenQuestion question)
    {
        openQuestionPanel.SetActive(true);
        answerInput.text = "";
    }

    private void ShowQCMQuestion(QCMQuestion question)
    {
        qcmQuestionText.text = question.Qst;
        qcmQuestionPanel.SetActive(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(true);
            choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.Choices[i];

            int choiceIndex = i;
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => CheckQCMAnswer(question, choiceIndex));
        }
    }

    private void CheckOpenAnswer()
    {
        string playerAnswer = answerInput.text.Trim().ToLower();
        string correctAnswer = ((OpenQuestion)currentQuestion).Answer.Trim().ToLower();
        openQuestionPanel.SetActive(false);
        ShowResult(playerAnswer == correctAnswer);
    }

    private void CheckQCMAnswer(QCMQuestion question, int choiceIndex)
    {
        qcmQuestionPanel.SetActive(false);
        ShowResult(choiceIndex == question.CorrectChoice);
    }

    private void ShowResult(bool isCorrect)
    {
        resultPanel.SetActive(true);
        resultText.text = isCorrect ? "✅ Correct!" : "❌ Wrong!";
    }

    private void CloseResultPanel()
    {
        resultPanel.SetActive(false);
        openQuestionPanel.SetActive(false);
        qcmQuestionPanel.SetActive(false);

        currentTile.ContinueGame();
    }
}
