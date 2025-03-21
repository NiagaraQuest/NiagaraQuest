using SQLite;

public class PlayerQuestionHistory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public int QuestionId { get; set; }
    
}
