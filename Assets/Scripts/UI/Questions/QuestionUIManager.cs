using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuestionUIManager : MonoBehaviour
{
    [Header("Open Question UI")]
    public GameObject openQuestionPanel;
    public TextMeshProUGUI openQuestionText;
    public TMP_InputField answerInput;
    public Button submitButton;

    [Header("QCM Question UI")]
    public GameObject qcmQuestionPanel;
    public TextMeshProUGUI qcmQuestionText;
    public Button[] choiceButtons;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button exitButton;

    private QuestionTile currentTile;
    private Question currentQuestion;
    private bool isProcessingQuestion = false;
    private bool lastAnswerCorrect = false; // Store the last answer result

    void Start()
    {
        // Hide all panels at start
        HideAllPanels();

        // Setup button listeners
        submitButton.onClick.AddListener(CheckOpenAnswer);
        exitButton.onClick.AddListener(CloseResultPanel);
    }

    void Update()
    {
        // Allow submitting open question answers with Enter key
        if (openQuestionPanel.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            CheckOpenAnswer();
        }
    }

    private void HideAllPanels()
    {
        openQuestionPanel.SetActive(false);
        qcmQuestionPanel.SetActive(false);
        resultPanel.SetActive(false);
    }

    public void ShowUI(Question question, QuestionTile tile)
    {
        if (isProcessingQuestion)
        {
            Debug.LogWarning("⚠️ Already processing a question, ignoring new request");
            return;
        }

        isProcessingQuestion = true;
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
        else
        {
            Debug.LogError($"⚠️ Unsupported question type: {question.GetType().Name}");
            isProcessingQuestion = false;
        }
    }

    private void ShowOpenQuestion(OpenQuestion question)
    {
        // Set question text
        openQuestionText.text = question.Qst;
        
        // Clear previous answer
        answerInput.text = "";
        
        // Show panel
        openQuestionPanel.SetActive(true);
        
        // Focus the input field
        StartCoroutine(FocusInputField());
    }
    
    private IEnumerator FocusInputField()
    {
        // Wait a frame to ensure the UI is active
        yield return null;
        answerInput.Select();
        answerInput.ActivateInputField();
    }

    private void ShowQCMQuestion(QCMQuestion question)
    {
        // Set question text
        qcmQuestionText.text = question.Qst;
        
        // Setup choice buttons
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < question.Choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.Choices[i];

                int choiceIndex = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => CheckQCMAnswer(question, choiceIndex));
            }
            else
            {
                // Hide unused buttons
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
        
        // Show panel
        qcmQuestionPanel.SetActive(true);
    }

    private void CheckOpenAnswer()
    {
        // Get player's answer and correct answer
        string playerAnswer = answerInput.text.Trim().ToLower();
        string correctAnswer = ((OpenQuestion)currentQuestion).Answer.Trim().ToLower();
        
        // Hide question panel
        openQuestionPanel.SetActive(false);
        
        // Store result
        lastAnswerCorrect = playerAnswer == correctAnswer;
        
        // Show result
        ShowResult(lastAnswerCorrect);
    }

    private void CheckQCMAnswer(QCMQuestion question, int choiceIndex)
    {
        // Hide question panel
        qcmQuestionPanel.SetActive(false);
        
        // Store result
        lastAnswerCorrect = choiceIndex == question.CorrectChoice;
        
        // Show result
        ShowResult(lastAnswerCorrect);
    }

    private void ShowResult(bool isCorrect)
    {
        // Show result panel
        resultPanel.SetActive(true);
        
        // Set result text
        resultText.text = isCorrect ? "✅ Correct!" : "❌ Wrong!";
    }

    // New method to get the question result
    public bool GetQuestionResult()
    {
        return lastAnswerCorrect;
    }

    private void CloseResultPanel()
    {
        // Hide all panels
        HideAllPanels();

        // Continue game flow
        if (currentTile != null)
        {
            currentTile.ContinueGame();
        }
        
        // Reset processing flag to allow new questions
        isProcessingQuestion = false;
    }
}