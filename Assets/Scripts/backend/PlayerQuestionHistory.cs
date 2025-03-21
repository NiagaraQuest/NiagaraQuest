using SQLite;

[Table("PlayerQuestionHistory")]
public class PlayerQuestionHistory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int PlayerId { get; set; }
    
    [Indexed]
    public int QuestionId { get; set; }
    
    // You might want to add more fields like:
    public bool Correct { get; set; }
    
    public DateTime AttemptedAt { get; set; }
}