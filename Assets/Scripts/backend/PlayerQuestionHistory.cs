using SQLite;
using System;

[Table("PlayerQuestionHistory")]
public class PlayerQuestionHistory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Remove individual indexes from these properties
    public int PlayerId { get; set; }
    public int QuestionId { get; set; }
    
    // Type of the question (QCM, Open, TrueFalse)
    public string QuestionType { get; set; }
    
    // Was the question answered correctly?
    public bool CorrectAnswer { get; set; }
    
    // When was this question presented to the player
    public DateTime TimestampUtc { get; set; }
    
    // How many times this question has been presented to the player
    public int AppearanceCount { get; set; } = 1;
    
    // ELO change for player from this question
    public int PlayerEloChange { get; set; }
    
    // ELO change for question from this player
    public int QuestionEloChange { get; set; }
}