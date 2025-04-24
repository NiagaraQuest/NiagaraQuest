using System;
using SQLite;
using Newtonsoft.Json;

[Table("Questions")]
public abstract class Question
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Category { get; set; }
    public string Qst { get; set; }
    public string Difficulty { get; set; }
    
    // ELO rating for the question
    public int Elo { get; set; } = 1000;
}

[Table("QCMQuestions")]
public class QCMQuestion : Question
{
    public string ChoicesJson { get; set; }
    public int CorrectChoice { get; set; }

    [Ignore]
    public string[] Choices
    {
        get => JsonConvert.DeserializeObject<string[]>(ChoicesJson) ?? Array.Empty<string>();
        set => ChoicesJson = JsonConvert.SerializeObject(value);
    }
}

[Table("OpenQuestions")]
public class OpenQuestion : Question
{
    public string Answer { get; set; }
}

[Table("TrueFalseQuestions")]
public class TrueFalseQuestion : Question
{
    public bool IsTrue { get; set; }
}