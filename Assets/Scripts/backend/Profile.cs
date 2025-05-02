using SQLite;
using System.Text.RegularExpressions;
using UnityEngine;

[Table("Profiles")]
public class Profile
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    // Remove the [Indexed, Unique] attributes here
    public string Username { get; set; }
    
    // Player's ELO rating (initially 1000)
    public int Elo { get; set; } = 1000;
    
    // Statistics
    public int TotalQuestionsAnswered { get; set; } = 0;
    public int CorrectAnswers { get; set; } = 0;
    public int QCMQuestionsAnswered { get; set; } = 0;
    public int OpenQuestionsAnswered { get; set; } = 0;
    public int TrueFalseQuestionsAnswered { get; set; } = 0;
    
    // Create an empty profile (required for SQLite)
    public Profile() { }
    
    // Create a new profile with a username
    public Profile(string username)
    {
        if (IsValidUsername(username))
        {
            Username = username;
        }
        else
        {
            Debug.LogError($"Invalid username: {username}. Username must start with a letter and be 3-16 characters using only letters, numbers, and underscores.");
            throw new System.ArgumentException("Invalid username format");
        }
    }
    
    // Validate username format
    public bool IsValidUsername(string username)
    {
        string pattern = @"^[a-zA-Z][a-zA-Z0-9_]{2,15}$";
        return Regex.IsMatch(username, pattern);
    }
    
    // Calculate ELO change based on question difficulty and result
    public int CalculateEloChange(Question question, bool correct)
    {
        // Expected score (probability of winning)
        double expectedScore = 1.0 / (1.0 + System.Math.Pow(10, (question.Elo - this.Elo) / 400.0));
        
        // Actual score (1 for win/correct, 0 for loss/incorrect)
        double actualScore = correct ? 1.0 : 0.0;
        
        // K-factor based on difficulty
        int kFactor;
        switch (question.Difficulty.ToUpper())
        {
            case "HARD":
                kFactor = 32;
                break;
            case "MEDIUM":
                kFactor = 24;
                break;
            case "EASY":
                kFactor = 16;
                break;
            default:
                kFactor = 20;
                break;
        }
        
        // Calculate ELO change
        int eloChange = (int)(kFactor * (actualScore - expectedScore));
        
        // Update player statistics
        UpdateStatistics(question, correct);
        
        return eloChange;
    }
    
    // Update player statistics
    private void UpdateStatistics(Question question, bool correct)
    {
        TotalQuestionsAnswered++;
        
        if (correct)
        {
            CorrectAnswers++;
        }
        
        if (question is QCMQuestion)
        {
            QCMQuestionsAnswered++;
        }
        else if (question is OpenQuestion)
        {
            OpenQuestionsAnswered++;
        }
        else if (question is TrueFalseQuestion)
        {
            TrueFalseQuestionsAnswered++;
        }
    }
    
    // Apply ELO change to the player
    public void ApplyEloChange(int eloChange)
    {
        Elo += eloChange;
        
        // Ensure ELO doesn't go below 100
        if (Elo < 100)
        {
            Elo = 100;
        }
        
        Debug.Log($"Elo updated for {Username}: {Elo - eloChange} -> {Elo} (change: {eloChange:+#;-#;0})");
    }
}