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

    [Header("ELO Display")]
    public bool showEloChanges = true;
    public float eloDisplayTime = 3f;
    public TextMeshProUGUI eloChangeText;

    private QuestionTile currentTile;
    private Question currentQuestion;
    public bool isProcessingQuestion = false;
    private bool isRetrying = false;
    private bool isSecondChance = false;
    private bool lastAnswerCorrect = false; // Store the last answer result
    private int lastPlayerEloChange = 0;
    private int lastQuestionEloChange = 0;

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
            
        // Hide ELO change text if it exists
        if (eloChangeText != null)
            eloChangeText.gameObject.SetActive(false);
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
        
        // Hide ELO change text if it exists
        if (eloChangeText != null)
            eloChangeText.gameObject.SetActive(false);
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
        
        // Process answer and update ELO
        ProcessPlayerAnswer(lastAnswerCorrect);
    }

    private void CheckQCMAnswer(QCMQuestion question, int choiceIndex)
    {
        // Hide question panel
        qcmQuestionPanel.SetActive(false);
        
        // Store result
        lastAnswerCorrect = choiceIndex == question.CorrectChoice;
        
        // Process answer and update ELO
        ProcessPlayerAnswer(lastAnswerCorrect);
    }
    
    private void CheckTrueFalseAnswer(bool userAnswer)
    {
        // Hide question panel
        tfQuestionPanel.SetActive(false);
        
        // Check if the user's answer matches the correct answer
        bool correctAnswer = ((TrueFalseQuestion)currentQuestion).IsTrue;
        
        // Store result
        lastAnswerCorrect = userAnswer == correctAnswer;
        
        // Process answer and update ELO
        ProcessPlayerAnswer(lastAnswerCorrect);
    }

    // Process the player's answer, update ELO and show result
    private async void ProcessPlayerAnswer(bool isCorrect)
    {
        Player currentPlayer = GameManager.Instance.GetCurrentPlayer();
        
        // Reset ELO change values
        lastPlayerEloChange = 0;
        lastQuestionEloChange = 0;
        
        // Check for second chance with PyroPlayer
        if (!isCorrect && currentPlayer is PyroPlayer pyroPlayer &&
            pyroPlayer.useSecondChance && !isSecondChance)
        {
            GameObject waypoint = pyroPlayer.GetCurrentWaypoint();
            Tile tile = waypoint?.GetComponent<Tile>();

            if (tile != null && tile.region == Tile.Region.Vulkan)
            {
                isSecondChance = true;
                resultPanel.SetActive(true);
                resultText.text = "❌ Wrong!\n🔥 Second chance!";
                Invoke("RetrySameQuestion", 1.5f);
                return;
            }
        }
        
        // Capture initial ELO values for display
        int initialPlayerElo = 0;
        int initialQuestionElo = 0;
        
        if (currentPlayer != null && currentPlayer.playerProfile != null && currentQuestion != null)
        {
            initialPlayerElo = currentPlayer.playerProfile.Elo;
            initialQuestionElo = currentQuestion.Elo;
            
            try
            {
                // Record the answer and update ELO values
                await QuestionManager.Instance.RecordPlayerAnswer(currentPlayer.playerProfile, currentQuestion, isCorrect);
                
                // Calculate the ELO changes
                lastPlayerEloChange = currentPlayer.playerProfile.Elo - initialPlayerElo;
                lastQuestionEloChange = currentQuestion.Elo - initialQuestionElo;
                
                Debug.Log($"⚖️ ELO Change - Player: {initialPlayerElo} → {currentPlayer.playerProfile.Elo} " +
                         $"({(lastPlayerEloChange >= 0 ? "+" : "")}{lastPlayerEloChange}), " +
                         $"Question: {initialQuestionElo} → {currentQuestion.Elo} " +
                         $"({(lastQuestionEloChange >= 0 ? "+" : "")}{lastQuestionEloChange})");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Error updating ELO: {ex.Message}");
            }
        }
        
        ShowResult(isCorrect);
    }

    private void ShowResult(bool isCorrect)
    {
        Player currentPlayer = GameManager.Instance.GetCurrentPlayer();

        // Si réponse correcte OU 2ème échec
        resultPanel.SetActive(true);
        string effectDescription = GetEffectDescription(currentQuestion.Difficulty, isCorrect);
        
        // Generate result text
        string resultBaseText = isCorrect ? 
            $"✅ Correct!\n\n<b>Reward:</b> {effectDescription}" : 
            $"❌ Wrong!\n\n<b>Penalty:</b> {effectDescription}";
            
        // Add ELO information if available and enabled
        if (showEloChanges && currentPlayer != null && currentPlayer.playerProfile != null)
        {
            int currentElo = currentPlayer.playerProfile.Elo;
            int previousElo = currentElo - lastPlayerEloChange;
            
            string eloColorStart = lastPlayerEloChange >= 0 ? "<color=#4CAF50>" : "<color=#F44336>";
            string eloColorEnd = "</color>";
            
            string eloChangeDisplay = $"\nELO: {previousElo} → {currentElo} ({eloColorStart}{(lastPlayerEloChange >= 0 ? "+" : "")}{lastPlayerEloChange}{eloColorEnd})";
            resultText.text = resultBaseText + eloChangeDisplay;
            
            // Show separate ELO change text if it exists
            if (eloChangeText != null)
            {
                eloChangeText.gameObject.SetActive(true);
                eloChangeText.text = $"ELO: {previousElo} → {currentElo} ({(lastPlayerEloChange >= 0 ? "+" : "")}{lastPlayerEloChange})";
                
                // Set color based on change
                if (lastPlayerEloChange > 0)
                    eloChangeText.color = new Color(0.2f, 0.8f, 0.2f); // Green
                else if (lastPlayerEloChange < 0)
                    eloChangeText.color = new Color(0.8f, 0.2f, 0.2f); // Red
                else
                    eloChangeText.color = Color.white;
                    
                // Hide ELO text after a few seconds
                StartCoroutine(HideEloTextAfterDelay(eloDisplayTime));
            }
        }
        else
        {
            resultText.text = resultBaseText;
        }
        
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

    private IEnumerator HideEloTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (eloChangeText != null)
            eloChangeText.gameObject.SetActive(false);
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

    private void RetrySameQuestion()
    {
        resultPanel.SetActive(false);
        if (currentQuestion is OpenQuestion)
            ShowOpenQuestion((OpenQuestion)currentQuestion);
        else if (currentQuestion is QCMQuestion)
            ShowQCMQuestion((QCMQuestion)currentQuestion);
        else if (currentQuestion is TrueFalseQuestion)
            ShowTrueFalseQuestion((TrueFalseQuestion)currentQuestion);
    }
}