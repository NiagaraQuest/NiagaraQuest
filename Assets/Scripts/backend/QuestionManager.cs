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
        string query = @"
            SELECT * FROM Question 
            WHERE NOT EXISTS 
                (SELECT 1 FROM PlayerQuestionHistory 
                 WHERE PlayerQuestionHistory.QuestionId = Question.Id 
                 AND PlayerQuestionHistory.PlayerId = ?) 
            ORDER BY RANDOM() 
            LIMIT 1;";

        var unseenQuestion = await dbManager.QueryFirstOrDefaultAsync<Question>(query, player.Id);

        if (unseenQuestion == null)
        {
            Debug.Log("No more new questions for this player.");
            return null;
        }

        await SaveQuestionAppearance(player, unseenQuestion);
        
        return unseenQuestion;
    }

    private async Task SaveQuestionAppearance(Profile player, Question question)
    {
        var history = new PlayerQuestionHistory { PlayerId = player.Id, QuestionId = question.Id };
        await dbManager.Insert(history);
    }
}
