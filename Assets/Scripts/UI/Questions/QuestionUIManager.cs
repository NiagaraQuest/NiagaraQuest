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
    public TextMeshProUGUI correctAnswerText;
    public Button exitButton;
    public float autoCloseResultTime = 2f;

    [Header("Effects")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    private QuestionTile currentTile;
    private Question currentQuestion;
    private Coroutine autoCloseCoroutine;
    private Player currentPlayer;

    void Start()
    {
        // Initialize all panels to hidden
        HideAllPanels();

        // Setup button listeners
        submitButton.onClick.AddListener(CheckOpenAnswer);
        exitButton.onClick.AddListener(CloseResultPanel);
        
        // Setup audio if needed
        if (audioSource == null && (correctSound != null || wrongSound != null))
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Allow Enter key to submit open answers
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
        // Cancel any existing auto-close routine
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        // Get the current player from the game manager
        currentPlayer = GameManager.Instance?.GetCurrentPlayer();
        
        // Store reference to tile and question
        currentTile = tile;
        currentQuestion = question;
        
        // Show the appropriate panel based on question type
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
            Debug.LogError($"Unsupported question type: {question.GetType().Name}");
        }
    }

    private void ShowOpenQuestion(OpenQuestion question)
    {
        // Set the question text
        openQuestionText.text = question.Qst;
        
        // Reset input field
        answerInput.text = "";
        
        // Show the panel
        openQuestionPanel.SetActive(true);
        
        // Focus the input field
        StartCoroutine(FocusInputField());
    }
    
    private IEnumerator FocusInputField()
    {
        // Wait for end of frame to ensure UI is initialized
        yield return new WaitForEndOfFrame();
        answerInput.Select();
        answerInput.ActivateInputField();
    }

    private void ShowQCMQuestion(QCMQuestion question)
    {
        // Set the question text
        qcmQuestionText.text = question.Qst;
        
        // Setup choice buttons
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < question.Choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.Choices[i];
                
                // Setup click handler
                int choiceIndex = i; // Local copy for closure
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => CheckQCMAnswer(question, choiceIndex));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
        
        // Show the panel
        qcmQuestionPanel.SetActive(true);
    }

    private void CheckOpenAnswer()
    {
        // Get player's answer and correct answer
        string playerAnswer = answerInput.text.Trim().ToLower();
        string correctAnswer = ((OpenQuestion)currentQuestion).Answer.Trim().ToLower();
        
        // Hide question panel
        openQuestionPanel.SetActive(false);
        
        // Check if answer is correct
        bool isCorrect = playerAnswer == correctAnswer;
        
        // Show result
        ShowResult(isCorrect, ((OpenQuestion)currentQuestion).Answer);
    }

    private void CheckQCMAnswer(QCMQuestion question, int choiceIndex)
    {
        // Hide question panel
        qcmQuestionPanel.SetActive(false);
        
        // Check if answer is correct
        bool isCorrect = choiceIndex == question.CorrectChoice;
        
        // Get correct answer text
        string correctAnswer = question.Choices[question.CorrectChoice];
        
        // Show result
        ShowResult(isCorrect, correctAnswer);
    }

    private void ShowResult(bool isCorrect, string correctAnswer)
    {
        // Set result text
        resultText.text = isCorrect ? "✅ Correct!" : "❌ Wrong!";
        
        // Show correct answer if wrong
        if (correctAnswerText != null)
        {
            if (isCorrect)
            {
                correctAnswerText.gameObject.SetActive(false);
            }
            else
            {
                correctAnswerText.gameObject.SetActive(true);
                correctAnswerText.text = $"The correct answer was: {correctAnswer}";
            }
        }
        
        // Apply game consequences
        if (currentPlayer != null)
        {
            // Using the Player.AnswerQuestion method
            currentPlayer.AnswerQuestion(isCorrect);
            
            // Also apply life consequences based on answer
            if (!isCorrect)
            {
                currentPlayer.LoseLife();
                Debug.Log($"Player lost a life. Remaining lives: {currentPlayer.lives}");
                
                // Play wrong sound
                if (audioSource != null && wrongSound != null)
                    audioSource.PlayOneShot(wrongSound);
            }
            else
            {
                // Play correct sound
                if (audioSource != null && correctSound != null)
                    audioSource.PlayOneShot(correctSound);
            }
        }
        
        // Show result panel
        resultPanel.SetActive(true);
        
        // Auto-close result after delay if set
        if (autoCloseResultTime > 0)
        {
            autoCloseCoroutine = StartCoroutine(AutoCloseResult(autoCloseResultTime));
        }
    }

    private IEnumerator AutoCloseResult(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseResultPanel();
    }

    private void CloseResultPanel()
    {
        // Cancel auto-close if running
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        
        // Hide all panels
        HideAllPanels();
        
        // Continue gameplay
        if (currentTile != null)
            currentTile.ContinueGame();
    }
    
    private void OnDisable()
    {
        // Clean up coroutines
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }
}