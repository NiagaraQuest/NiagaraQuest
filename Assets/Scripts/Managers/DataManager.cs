using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System;

public class DataManager : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("GameManager Awake called");
    }

    private async void Start()
    {
        Debug.Log("GameManager Start called");
        
        try
        {
            Debug.Log("Initializing Database...");
            try
            {
                await DatabaseManager.Instance.Initialize();
                Debug.Log("Database Initialized!");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to initialize database. Stopping game initialization.");
                Debug.LogError(e);
                return;
            }

            Debug.Log("Creating Test Profile...");
            bool created = await ProfileManager.Instance.CreateProfile("TestPlayer");
            if (!created)
            {
                Debug.LogError("Profile creation failed!");
                return;
            }
            Debug.Log("Profile Created Successfully!");

            Profile player = await ProfileManager.Instance.GetProfileById(1);
            if (player == null)
            {
                Debug.LogError("Failed to retrieve profile!");
                return;
            }
            Debug.Log($"Player Found: {player.Username}");

            Debug.Log("Generating Question...");
            Question question = await QuestionManager.Instance.GenerateQuestionForPlayer(player);
            if (question == null)
            {
                Debug.Log("No available questions!");
            }
            else
            {
                Debug.Log($"Generated Question: {question.Qst}");
                if (question is OpenQuestion openQuestion)
                {
                    Debug.Log($"This is an open question with answer: {openQuestion.Answer}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in GameManager: {e.Message}\n{e.StackTrace}");
        }
    }
}