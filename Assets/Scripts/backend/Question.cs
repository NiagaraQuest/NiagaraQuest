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
    public Tile.Difficulty Difficulty { get; set; }
}

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

public class OpenQuestion : Question
{
    public string Answer { get; set; }
}

public class FillTheGapsQuestion : Question
{
    public string SuggestionsJson { get; set; }
    public string Response { get; set; }

    [Ignore]
    public string[] Suggestions
    {
        get => JsonConvert.DeserializeObject<string[]>(SuggestionsJson) ?? Array.Empty<string>();
        set => SuggestionsJson = JsonConvert.SerializeObject(value);
    }
}

public class TrueFalseQuestion : Question
{
    public bool IsTrue { get; set; }
}

public class RankingQuestion : Question
{
    public string ListJson { get; set; }
    public string CorrectOrderJson { get; set; }

    [Ignore]
    public string[] List
    {
        get => JsonConvert.DeserializeObject<string[]>(ListJson) ?? Array.Empty<string>();
        set => ListJson = JsonConvert.SerializeObject(value);
    }

    [Ignore]
    public string[] CorrectOrder
    {
        get => JsonConvert.DeserializeObject<string[]>(CorrectOrderJson) ?? Array.Empty<string>();
        set => CorrectOrderJson = JsonConvert.SerializeObject(value);
    }
}
