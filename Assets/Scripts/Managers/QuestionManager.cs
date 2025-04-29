using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

public class QuestionManager
{
    private static QuestionManager _instance;
    public static QuestionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new QuestionManager();
            }
            return _instance;
        }
    }

    private readonly DatabaseManager _dbManager;
    private System.Random _random;

    private QuestionManager()
    {
        _dbManager = DatabaseManager.Instance;
        _random = new System.Random();
    }
    
    public async Task Initialize()
    {
        Debug.Log("Initializing QuestionManager...");
    
        // Use the new CountAsync method instead of CountQuestions
        int qcmCount = await _dbManager.CountAsync<QCMQuestion>();
        int openCount = await _dbManager.CountAsync<OpenQuestion>();
        int tfCount = await _dbManager.CountAsync<TrueFalseQuestion>();
        
        Debug.Log($"Database contains {qcmCount + openCount + tfCount} questions " +
                 $"({qcmCount} QCM, {openCount} Open, {tfCount} True/False)");
    }
    
    // Generate a question for a player based on their ELO
    public async Task<Question> GenerateQuestionForPlayer(Profile player)
    {
        Debug.Log($"Generating question for player {player.Username} (ELO: {player.Elo})");
        
        string difficulty = SelectDifficultyForPlayer(player);
        
        Question question = await GetUnseenQuestion(player.Id, difficulty, player.Elo);
        
        // If no unseen questions with the selected difficulty, try any difficulty
        if (question == null)
        {
            Debug.Log($"No unseen questions found for {player.Username} with difficulty {difficulty}. Trying any difficulty.");
            question = await GetUnseenQuestion(player.Id, null, player.Elo);
        }
        
        // If all questions have been seen, get the least recently seen question
        if (question == null)
        {
            Debug.Log($"No unseen questions found for {player.Username}. Getting least recently seen question.");
            question = await GetLeastRecentlySeenQuestion(player.Id);
        }
        
        // If still no question (rare case), get any random question
        if (question == null)
        {
            Debug.Log($"No question history found for {player.Username}. Getting random question.");
            question = await GetRandomQuestion();
        }
        
        if (question != null)
        {
            Debug.Log($"Selected question ID: {question.Id}, Type: {question.GetType().Name}, ELO: {question.Elo}");
        }
        else
        {
            Debug.LogError("Failed to find any question. Database may be empty.");
        }
        
        return question;
    }
    
    // Select difficulty based on player's ELO
    private string SelectDifficultyForPlayer(Profile player)
    {
        if (player.Elo < 1000)
        {
            return "EASY";
        }
        else if (player.Elo < 1500)
        {
            return "MEDIUM";
        }
        else
        {
            return "HARD";
        }
    }
    
    // Get a question that the player hasn't seen before
    private async Task<Question> GetUnseenQuestion(int playerId, string difficulty, int playerElo)
    {
        // We'll try all three question types in random order
        List<string> questionTypes = new List<string> { 
            nameof(QCMQuestion), 
            nameof(OpenQuestion), 
            nameof(TrueFalseQuestion) 
        };
        
        // Shuffle question types for variety
        questionTypes = questionTypes.OrderBy(x => _random.Next()).ToList();
        
        foreach (string questionType in questionTypes)
        {
            string tableName = $"{questionType}s";
            string difficultyClause = difficulty != null ? $"AND Q.Difficulty = '{difficulty}'" : "";
            
            // Query to find a question that doesn't exist in the player's history
            string query = $@"
                SELECT Q.* FROM {tableName} Q
                WHERE NOT EXISTS (
                    SELECT 1 FROM PlayerQuestionHistory H
                    WHERE H.PlayerId = ? AND H.QuestionId = Q.Id AND H.QuestionType = ?
                )
                {difficultyClause}
                ORDER BY ABS(Q.Elo - ?) 
                LIMIT 10";
            
            // Parameters: playerId, questionType, playerElo
            List<Question> candidates;
            switch (questionType)
            {
                case nameof(QCMQuestion):
                    candidates = (await _dbManager.QueryAsync<QCMQuestion>(query, playerId, questionType, playerElo)).Cast<Question>().ToList();
                    break;
                case nameof(OpenQuestion):
                    candidates = (await _dbManager.QueryAsync<OpenQuestion>(query, playerId, questionType, playerElo)).Cast<Question>().ToList();
                    break;
                case nameof(TrueFalseQuestion):
                    candidates = (await _dbManager.QueryAsync<TrueFalseQuestion>(query, playerId, questionType, playerElo)).Cast<Question>().ToList();
                    break;
                default:
                    candidates = new List<Question>();
                    break;
            }
            
            if (candidates.Count > 0)
            {
                // Return a random question from the 10 closest ELO matches
                return candidates[_random.Next(candidates.Count)];
            }
        }
        
        return null;
    }
    
    // Get a question that the player has seen least recently
    private async Task<Question> GetLeastRecentlySeenQuestion(int playerId)
    {
        // We'll try all three question types in random order
        List<string> questionTypes = new List<string> { 
            nameof(QCMQuestion), 
            nameof(OpenQuestion), 
            nameof(TrueFalseQuestion) 
        };
        
        // Shuffle question types for variety
        questionTypes = questionTypes.OrderBy(x => _random.Next()).ToList();
        
        foreach (string questionType in questionTypes)
        {
            string tableName = $"{questionType}s";
            
            // First, find the minimum appearance count using direct SQL query
            string minAppearanceQuery = $@"
                SELECT MIN(AppearanceCount) FROM PlayerQuestionHistory
                WHERE PlayerId = ? AND QuestionType = ?";
            
            int minCount = await _dbManager.ExecuteScalarAsync(minAppearanceQuery, playerId, questionType);
            
            if (minCount <= 0)
            {
                continue; // Try next question type
            }
            
            // Get the least recently seen questions with minimum appearance count
            string query = $@"
                SELECT Q.* FROM {tableName} Q
                JOIN PlayerQuestionHistory H ON Q.Id = H.QuestionId
                WHERE H.PlayerId = ? AND H.QuestionType = ? AND H.AppearanceCount = ?
                ORDER BY H.TimestampUtc ASC
                LIMIT 10";
            
            List<Question> candidates;
            switch (questionType)
            {
                case nameof(QCMQuestion):
                    candidates = (await _dbManager.QueryAsync<QCMQuestion>(query, playerId, questionType, minCount)).Cast<Question>().ToList();
                    break;
                case nameof(OpenQuestion):
                    candidates = (await _dbManager.QueryAsync<OpenQuestion>(query, playerId, questionType, minCount)).Cast<Question>().ToList();
                    break;
                case nameof(TrueFalseQuestion):
                    candidates = (await _dbManager.QueryAsync<TrueFalseQuestion>(query, playerId, questionType, minCount)).Cast<Question>().ToList();
                    break;
                default:
                    candidates = new List<Question>();
                    break;
            }
            
            if (candidates.Count > 0)
            {
                // Return a random question from the 10 oldest seen questions
                return candidates[_random.Next(Math.Min(candidates.Count, 10))];
            }
        }
        
        return null;
    }
    
    // Get any random question (fallback method)
    private async Task<Question> GetRandomQuestion()
    {
        // We'll try all three question types in random order
        List<string> questionTypes = new List<string> { 
            nameof(QCMQuestion), 
            nameof(OpenQuestion), 
            nameof(TrueFalseQuestion) 
        };
        
        // Shuffle question types for variety
        questionTypes = questionTypes.OrderBy(x => _random.Next()).ToList();
        
        foreach (string questionType in questionTypes)
        {
            string tableName = $"{questionType}s";
            
            // Get a random question
            string query = $"SELECT * FROM {tableName} ORDER BY RANDOM() LIMIT 1";
            
            switch (questionType)
            {
                case nameof(QCMQuestion):
                    var qcm = await _dbManager.QueryFirstOrDefaultAsync<QCMQuestion>(query);
                    if (qcm != null) return qcm;
                    break;
                case nameof(OpenQuestion):
                    var open = await _dbManager.QueryFirstOrDefaultAsync<OpenQuestion>(query);
                    if (open != null) return open;
                    break;
                case nameof(TrueFalseQuestion):
                    var tf = await _dbManager.QueryFirstOrDefaultAsync<TrueFalseQuestion>(query);
                    if (tf != null) return tf;
                    break;
            }
        }
        
        return null;
    }
    
    // Record player's answer to a question and update ELOs
    public async Task RecordPlayerAnswer(Profile player, Question question, bool isCorrect)
    {
        Debug.Log($"Recording answer from {player.Username} for question {question.Id}: {(isCorrect ? "Correct" : "Incorrect")}");
        
        // Calculate ELO changes
        int playerEloChange = player.CalculateEloChange(question, isCorrect);
        int questionEloChange = CalculateQuestionEloChange(question, player, isCorrect);
        
        // Apply ELO changes
        player.ApplyEloChange(playerEloChange);
        question.Elo += questionEloChange;
        
        // Check if this question has been seen before
        var existingHistory = await _dbManager.QueryFirstOrDefaultAsync<PlayerQuestionHistory>(
            "SELECT * FROM PlayerQuestionHistory WHERE PlayerId = ? AND QuestionId = ? AND QuestionType = ?",
            player.Id, question.Id, question.GetType().Name);
        
        if (existingHistory != null)
        {
            // Update existing history
            existingHistory.AppearanceCount++;
            existingHistory.CorrectAnswer = isCorrect;
            existingHistory.TimestampUtc = DateTime.UtcNow;
            existingHistory.PlayerEloChange = playerEloChange;
            existingHistory.QuestionEloChange = questionEloChange;
            
            await _dbManager.Update(existingHistory);
        }
        else
        {
            // Create new history entry
            var history = new PlayerQuestionHistory
            {
                PlayerId = player.Id,
                QuestionId = question.Id,
                QuestionType = question.GetType().Name,
                CorrectAnswer = isCorrect,
                TimestampUtc = DateTime.UtcNow,
                AppearanceCount = 1,
                PlayerEloChange = playerEloChange,
                QuestionEloChange = questionEloChange
            };
            
            await _dbManager.Insert(history);
        }
        
        // Update question in database
        if (question is QCMQuestion qcmQuestion)
        {
            await _dbManager.Update(qcmQuestion);
        }
        else if (question is OpenQuestion openQuestion)
        {
            await _dbManager.Update(openQuestion);
        }
        else if (question is TrueFalseQuestion tfQuestion)
        {
            await _dbManager.Update(tfQuestion);
        }
        
        // Update player in database
        await _dbManager.Update(player);
    }
    
    // Calculate ELO change for a question
    private int CalculateQuestionEloChange(Question question, Profile player, bool correctAnswer)
    {
        // Expected score (probability of question "winning")
        double expectedScore = 1.0 / (1.0 + Math.Pow(10, (player.Elo - question.Elo) / 400.0));
        
        // Actual score (1 for question "loss" when player answers correctly, 0 for question "win" when player is wrong)
        double actualScore = correctAnswer ? 0.0 : 1.0;
        
        // K-factor based on question difficulty
        int kFactor;
        switch (question.Difficulty.ToUpper())
        {
            case "HARD":
                kFactor = 16;
                break;
            case "MEDIUM":
                kFactor = 12;
                break;
            case "EASY":
                kFactor = 8;
                break;
            default:
                kFactor = 10;
                break;
        }
        
        // Calculate ELO change
        return (int)(kFactor * (actualScore - expectedScore));
    }
    
    // Add a new question to the database
    public async Task<Question> AddQuestion(Question question)
    {
        // Set default ELO if not set
        if (question.Elo <= 0)
        {
            question.Elo = 1000;
        }
        
        // Insert based on question type
        if (question is QCMQuestion qcmQuestion)
        {
            await _dbManager.Insert(qcmQuestion);
        }
        else if (question is OpenQuestion openQuestion)
        {
            await _dbManager.Insert(openQuestion);
        }
        else if (question is TrueFalseQuestion tfQuestion)
        {
            await _dbManager.Insert(tfQuestion);
        }
        
        return question;
    }
    
    // Get a question by ID
    public async Task<Question> GetQuestionById(int id, string questionType)
    {
        Question question = null;
        
        // Query based on question type
        switch (questionType)
        {
            case nameof(QCMQuestion):
                question = await _dbManager.GetById<QCMQuestion>(id);
                break;
                
            case nameof(OpenQuestion):
                question = await _dbManager.GetById<OpenQuestion>(id);
                break;
                
            case nameof(TrueFalseQuestion):
                question = await _dbManager.GetById<TrueFalseQuestion>(id);
                break;
                
            default:
                Debug.LogError($"Unknown question type: {questionType}");
                break;
        }
        
        return question;
    }

    public async Task<List<PlayerQuestionHistory>> GetPlayerQuestionHistory(int playerId, int limit = 50)
    {
        return await _dbManager.QueryAsync<PlayerQuestionHistory>(
            "SELECT * FROM PlayerQuestionHistory WHERE PlayerId = ? ORDER BY TimestampUtc DESC LIMIT ?", 
            playerId, limit);
    }
    
    public async Task<Dictionary<string, object>> GetPlayerPerformanceStats(int playerId)
    {
        var stats = new Dictionary<string, object>();
        
        // Get count of total questions answered
        int totalAnswered = await _dbManager.ExecuteScalarAsync(
            "SELECT COUNT(*) FROM PlayerQuestionHistory WHERE PlayerId = ?", 
            playerId);
        
        stats["TotalQuestionsAnswered"] = totalAnswered;
        
        // Get count of correct answers
        int correctAnswers = await _dbManager.ExecuteScalarAsync(
            "SELECT COUNT(*) FROM PlayerQuestionHistory WHERE PlayerId = ? AND CorrectAnswer = 1", 
            playerId);
        
        stats["CorrectAnswers"] = correctAnswers;
        stats["IncorrectAnswers"] = totalAnswered - correctAnswers;
        
        if (totalAnswered > 0)
        {
            stats["CorrectPercentage"] = (double)correctAnswers / totalAnswered * 100;
        }
        else
        {
            stats["CorrectPercentage"] = 0.0;
        }
        
        // Stats by question type
        int qcmAnswered = await _dbManager.ExecuteScalarAsync(
            "SELECT COUNT(*) FROM PlayerQuestionHistory WHERE PlayerId = ? AND QuestionType = ?", 
            playerId, nameof(QCMQuestion));
        
        int openAnswered = await _dbManager.ExecuteScalarAsync(
            "SELECT COUNT(*) FROM PlayerQuestionHistory WHERE PlayerId = ? AND QuestionType = ?", 
            playerId, nameof(OpenQuestion));
        
        int tfAnswered = await _dbManager.ExecuteScalarAsync(
            "SELECT COUNT(*) FROM PlayerQuestionHistory WHERE PlayerId = ? AND QuestionType = ?", 
            playerId, nameof(TrueFalseQuestion));
        
        stats["QCMQuestionsAnswered"] = qcmAnswered;
        stats["OpenQuestionsAnswered"] = openAnswered;
        stats["TrueFalseQuestionsAnswered"] = tfAnswered;
        
        // Get difficulty stats with direct SQL queries
        var difficultyStats = new Dictionary<string, int>
        {
            ["EASY"] = 0,
            ["MEDIUM"] = 0,
            ["HARD"] = 0
        };
        
        // For each question type, join with the history table and count by difficulty
        foreach (var pair in new[] { 
            (Type: nameof(QCMQuestion), Table: "QCMQuestions"),
            (Type: nameof(OpenQuestion), Table: "OpenQuestions"),
            (Type: nameof(TrueFalseQuestion), Table: "TrueFalseQuestions")
        })
        {
            foreach (var difficulty in difficultyStats.Keys.ToList())
            {
                string query = $@"
                    SELECT COUNT(*) FROM PlayerQuestionHistory H
                    JOIN {pair.Table} Q ON H.QuestionId = Q.Id
                    WHERE H.PlayerId = ? AND H.QuestionType = ? AND Q.Difficulty = ?";
                
                int count = await _dbManager.ExecuteScalarAsync(query, playerId, pair.Type, difficulty);
                difficultyStats[difficulty] += count;
            }
        }
        
        stats["EasyQuestionsAnswered"] = difficultyStats["EASY"];
        stats["MediumQuestionsAnswered"] = difficultyStats["MEDIUM"];
        stats["HardQuestionsAnswered"] = difficultyStats["HARD"];
        
        return stats;
    }
    
    // Delete a question
    public async Task DeleteQuestion(int id, string questionType)
    {
        // Delete question based on type
        switch (questionType)
        {
            case nameof(QCMQuestion):
                await _dbManager.Delete<QCMQuestion>(id);
                break;
                
            case nameof(OpenQuestion):
                await _dbManager.Delete<OpenQuestion>(id);
                break;
                
            case nameof(TrueFalseQuestion):
                await _dbManager.Delete<TrueFalseQuestion>(id);
                break;
                
            default:
                Debug.LogError($"Unknown question type: {questionType}");
                break;
        }
        
        // Also delete any history entries for this question
        await _dbManager.ExecuteAsync(
            "DELETE FROM PlayerQuestionHistory WHERE QuestionId = ? AND QuestionType = ?", 
            id, questionType);
    }
}