using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    private readonly DatabaseManager dbManager;

    private QuestionManager()
    {
        dbManager = DatabaseManager.Instance;
    }

   public async Task<Question> GenerateQuestionForPlayer(Profile player)
{
    string qcmQuery = @"
        SELECT * FROM QCMQuestion 
        WHERE NOT EXISTS 
            (SELECT 1 FROM PlayerQuestionHistory 
             WHERE PlayerQuestionHistory.QuestionId = QCMQuestion.Id 
             AND PlayerQuestionHistory.PlayerId = ?) 
        ORDER BY RANDOM() 
        LIMIT 1;";

    string openQuery = @"
        SELECT * FROM OpenQuestion 
        WHERE NOT EXISTS 
            (SELECT 1 FROM PlayerQuestionHistory 
             WHERE PlayerQuestionHistory.QuestionId = OpenQuestion.Id 
             AND PlayerQuestionHistory.PlayerId = ?) 
        ORDER BY RANDOM() 
        LIMIT 1;";

    // Get one question from each category
    QCMQuestion qcmQuestion = await dbManager.QueryFirstOrDefaultAsync<QCMQuestion>(qcmQuery, player.Id);
    OpenQuestion openQuestion = await dbManager.QueryFirstOrDefaultAsync<OpenQuestion>(openQuery, player.Id);

    // Combine results into a single list
    List<Question> availableQuestions = new List<Question>();
    if (qcmQuestion != null) availableQuestions.Add(qcmQuestion);
    if (openQuestion != null) availableQuestions.Add(openQuestion);

    if (availableQuestions.Count == 0)
    {
        Debug.Log("No more new questions for this player.");
        return null;
    }

    // Randomly select a question
    Question selectedQuestion = availableQuestions[UnityEngine.Random.Range(0, availableQuestions.Count)];

    await SaveQuestionAppearance(player, selectedQuestion);
    return selectedQuestion;
}

    private async Task SaveQuestionAppearance(Profile player, Question question)
    {
        var history = new PlayerQuestionHistory { PlayerId = player.Id, QuestionId = question.Id };
        await dbManager.Insert(history);
    }
}
