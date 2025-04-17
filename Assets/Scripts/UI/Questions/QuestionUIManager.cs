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
    
    [Header("True/False Question UI")]
    public GameObject tfQuestionPanel;
    public TextMeshProUGUI tfQuestionText;
    public Button trueButton;
    public Button falseButton;

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
        
        // Setup True/False button listeners
        if (trueButton != null)
            trueButton.onClick.AddListener(() => CheckTrueFalseAnswer(true));
        if (falseButton != null)
            falseButton.onClick.AddListener(() => CheckTrueFalseAnswer(false));
    }

    void Update()
    {
        // Enter key handler - simulates clicking the active button
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Submit answer if open question panel is active
            if (openQuestionPanel.activeSelf)
            {
                submitButton.onClick.Invoke();
            }
            // Close result panel if it's active
            else if (resultPanel.activeSelf)
            {
                exitButton.onClick.Invoke();
            }
        }
    }

    private void HideAllPanels()
    {
        openQuestionPanel.SetActive(false);
        qcmQuestionPanel.SetActive(false);
        tfQuestionPanel.SetActive(false);
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
        else if (question is TrueFalseQuestion tfQuestion)
        {
            ShowTrueFalseQuestion(tfQuestion);
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
    
    private void ShowTrueFalseQuestion(TrueFalseQuestion question)
    {
        // Set question text
        tfQuestionText.text = question.Qst;
        
        // Make sure the True and False buttons are active
        if (trueButton != null)
            trueButton.gameObject.SetActive(true);
        if (falseButton != null)
            falseButton.gameObject.SetActive(true);
        
        // Show panel
        tfQuestionPanel.SetActive(true);
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
        
        // Update ELO ratings
        UpdatePlayerElo(lastAnswerCorrect);
    }

    private void CheckQCMAnswer(QCMQuestion question, int choiceIndex)
    {
        // Hide question panel
        qcmQuestionPanel.SetActive(false);
        
        // Store result
        lastAnswerCorrect = choiceIndex == question.CorrectChoice;
        
        // Show result
        ShowResult(lastAnswerCorrect);
        
        // Update ELO ratings
        UpdatePlayerElo(lastAnswerCorrect);
    }
    
    private void CheckTrueFalseAnswer(bool userAnswer)
    {
        // Hide question panel
        tfQuestionPanel.SetActive(false);
        
        // Check if the user's answer matches the correct answer
        bool correctAnswer = ((TrueFalseQuestion)currentQuestion).IsTrue;
        
        // Store result
        lastAnswerCorrect = userAnswer == correctAnswer;
        
        // Show result
        ShowResult(lastAnswerCorrect);
        
        // Update ELO ratings
        UpdatePlayerElo(lastAnswerCorrect);
    }

    private void ShowResult(bool isCorrect)
    {
        resultPanel.SetActive(true);
        resultText.text = isCorrect ? "✅ Correct!" : "❌ Wrong!";
        GameManager.Instance.ApplyQuestionResult(GameManager.Instance.GetCurrentPlayer(), isCorrect, currentQuestion.Difficulty);
        
        // Focus on the exit button so Enter key works right away
        StartCoroutine(FocusExitButton());
    }
    
    private IEnumerator FocusExitButton()
    {
        // Wait a frame to ensure the panel is active
        yield return null;
        if (exitButton != null)
        {
            exitButton.Select();
        }
    }
    
    private async void UpdatePlayerElo(bool isCorrect)
    {
        try
        {
            // Get the current player
            Player currentPlayer = GameManager.Instance.GetCurrentPlayer();
            if (currentPlayer != null && currentPlayer.playerProfile != null && currentQuestion != null)
            {
                // Record the answer using QuestionManager - this will update ELO ratings
                int initialElo = currentPlayer.playerProfile.Elo;
                await QuestionManager.Instance.RecordPlayerAnswer(currentPlayer.playerProfile, currentQuestion, isCorrect);
                
                // Log ELO change
                int newElo = currentPlayer.playerProfile.Elo;
                int eloChange = newElo - initialElo;
                
                Debug.Log($"⚖️ Player ELO updated: {initialElo} → {newElo} ({(eloChange >= 0 ? "+" : "")}{eloChange})");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error updating ELO ratings: {ex.Message}");
        }
    }

    // Method to get the question result
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