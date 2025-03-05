using UnityEngine;
using System;

public class QuestionManager
{
    private static QuestionManager _instance;
    private static QuestionManager Instance
    {
         get {
            if (_instance == null){
                _instance = new QuestionManager();
            }

            return _instance;

             }
    }
    private readonly DatabaseManager dbManager ;

    private QuestionManager(){
      dbManager = DatabaseManager.Instance;
    }

    public async Task<Question> GenerateQuestionForPlayer(Profile player)
    {
        List<Question> allQuestions = await dbManager.GetAll<Question>();
        List<PlayerQuestionHistory> history = await dbManager.GetAll<PlayerQuestionHistory>();

        HashSet<int> answeredQuestionIds = new HashSet<int>();
        foreach (var entry in history)
        {
            if (entry.PlayerId == player.Id)
            {
                answeredQuestionIds.Add(entry.QuestionId);
            }
        }

        List<Question> availableQuestions = allQuestions.FindAll(q => !answeredQuestionIds.Contains(q.Id));

        if (availableQuestions.Count == 0)
        {
            Debug.Log("No more new questions for this player.");
            return null;
        }

        Question selectedQuestion = availableQuestions[UnityEngine.Random.Range(0, availableQuestions.Count)];

        await SaveQuestionAppearance(player, selectedQuestion);

        return selectedQuestion;
    }

}
