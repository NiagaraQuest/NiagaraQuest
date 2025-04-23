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
    public bool isProcessingQuestion = false;
    private bool isRetrying = false;
    private bool isSecondChance = false;
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

    //lyna
    public void SkipQuestion()
    {
        HideAllPanels();
        isProcessingQuestion = false;
        currentTile.SkipQuestion();
    }

    private string GetFormattedDifficulty(string difficulty)
    {
        // Normalize the difficulty to get a consistent format
        string normalizedDifficulty = difficulty.ToUpper();
        string colorCode = "";
        string difficultyText = "";
        
        switch (normalizedDifficulty)
        {
            case "EASY":
                colorCode = "#4CAF50"; // Green
                difficultyText = "Easy";
                break;
            case "MEDIUM":
                colorCode = "#FF9800"; // Orange
                difficultyText = "Medium";
                break;
            case "HARD":
                colorCode = "#F44336"; // Red
                difficultyText = "Hard";
                break;
            default:
                colorCode = "#2196F3"; // Blue
                difficultyText = difficulty; // Use as-is
                break;
        }
        
        return $"<color={colorCode}><b>{difficultyText}</b></color>";
    }



    private void ShowOpenQuestion(OpenQuestion question)
    {
        string difficultyDisplay = GetFormattedDifficulty(question.Difficulty);
        openQuestionText.text = $"{difficultyDisplay}\n{question.Qst}";
        
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
    string difficultyDisplay = GetFormattedDifficulty(question.Difficulty);
    qcmQuestionText.text = $"{difficultyDisplay}\n{question.Qst}";
        
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
    string difficultyDisplay = GetFormattedDifficulty(question.Difficulty);
    tfQuestionText.text = $"{difficultyDisplay}\n{question.Qst}";
        
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

    // Dans QuestionUIManager.cs, modifiez la méthode ShowResult:
    private void ShowResult(bool isCorrect)
    {
        Player currentPlayer = GameManager.Instance.GetCurrentPlayer();

        if (!isCorrect && currentPlayer is PyroPlayer pyroPlayer &&
            pyroPlayer.useSecondChance && !isSecondChance)
        {
            GameObject waypoint = pyroPlayer.GetCurrentWaypoint();
            Tile tile = waypoint?.GetComponent<Tile>();

            if (tile != null && tile.region == Tile.Region.Vulkan)
            {
                isSecondChance = true;
                resultPanel.SetActive(true);
                resultText.text = "❌ Wrong!\n🔥 Seconde chance!";
                Invoke("RetrySameQuestion", 1.5f);
                return;
            }
        }

        // Si réponse correcte OU 2ème échec
        resultPanel.SetActive(true);
        string effectDescription = GetEffectDescription(currentQuestion.Difficulty, isCorrect);
        resultText.text = isCorrect ? 
            $"✅ Correct!\n\n<b>Reward:</b> {effectDescription}" : 
            $"❌ Wrong!\n\n<b>Penalty:</b> {effectDescription}";
        isSecondChance = false;

        if (currentPlayer != null)
        {
            currentPlayer.AnswerQuestion(isCorrect); // Appeler la méthode AnswerQuestion du joueur
        }

        if (!isCorrect)
        {
            // Check if player has protection
            bool protectionUsed = CardManager.Instance.UseProtectionIfAvailable(currentPlayer);
            
            if (protectionUsed)
            {
                // Player was protected, show different result text
                resultText.text = "❌ Wrong!\n\n🛡️ Protection activated! No penalty applied.";
            }
            else
            {
                // Normal wrong answer handling
                GameManager.Instance.ApplyQuestionResult(currentPlayer, false, currentQuestion.Difficulty);
            }
        }
        else {
            GameManager.Instance.ApplyQuestionResult(currentPlayer, true, currentQuestion.Difficulty);
        }

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

    private string GetEffectDescription(string difficulty, bool isCorrect)
    {
    // Normalize the difficulty
    string normalizedDifficulty = difficulty.ToUpper();
    
    switch (normalizedDifficulty)
    {
        case "EASY":
            if (isCorrect)
                return "Move forward 2 spaces";
            else
                return "Move back 6 spaces";
        
        case "MEDIUM":
            if (isCorrect)
                return "Roll the dice again";
            else
                return "Lose 1 life";
        
        case "HARD":
            if (isCorrect)
                return "Gain 1 life";
            else
                return "Skip 1 turn";
        
        default:
            return "Unknown effect";
    }
    }


    private async void UpdatePlayerElo(bool isCorrect)
    {
        try
        {
            // Get the current playerd
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
        HideAllPanels();

        // Ne pas continuer le jeu si on donne une seconde chance
        if (!isRetrying && currentTile != null)
        {
            currentTile.ContinueGame();
        }

        isProcessingQuestion = false;
    }

    private IEnumerator ShowSecondChanceQuestion()
    {
        // Afficher le message de seconde chance
        resultText.text = "❌ Wrong!\n🔥 Seconde chance!";
        yield return new WaitForSeconds(1.5f);

        // Réafficher la même question
        resultPanel.SetActive(false);

        if (currentQuestion is OpenQuestion)
        {
            ShowOpenQuestion((OpenQuestion)currentQuestion);
        }
        else if (currentQuestion is QCMQuestion)
        {
            ShowQCMQuestion((QCMQuestion)currentQuestion);
        }
    }
    private void RetrySameQuestion()
    {
        resultPanel.SetActive(false);
        if (currentQuestion is OpenQuestion)
            ShowOpenQuestion((OpenQuestion)currentQuestion);
        else if (currentQuestion is QCMQuestion)
            ShowQCMQuestion((QCMQuestion)currentQuestion);
    }



}