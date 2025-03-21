using UnityEngine;
using System.Text.RegularExpressions;

public class Profile
{
    private static int idCounter = 0;
    public int Id { get; set; }
    public string Username { get; set; }
    public int elo {get; set;}

    private bool IsValidUsername(string username)
    {
        string pattern = @"^[a-zA-Z][a-zA-Z0-9_]{2,15}$";
        return Regex.IsMatch(username, pattern);
    }
    public Profile() { }
    public Profile(string name)
    {
        if (idCounter >= 10)
        {
            Debug.Log("You can create only 10 profiles.");
            return;
        }
        
        if (!IsValidUsername(name))
        {
            Debug.Log("Username is not valid.");
            return;
        }

        Id = ++idCounter;
        Username = name;
        elo = 1000;
    }

    
    public void AddElo(string difficulty)
    {
        switch (difficulty.ToUpper())
        {
            case "HARD":
                elo += 30;
                break;
            case "MEDIUM":
                elo += 20;
                break;
            case "EASY":
                elo += 10;
                break;
            default:
                Debug.LogError("Invalid difficulty type.");
                return;
        }
        
        Debug.Log($"Elo updated for {Username}:New Elo: {elo}");
    }

    public void SubElo( string difficulty)
    {
        switch (difficulty.ToUpper())
        {
            case "HARD":
                elo -= 10;
                break;
            case "MEDIUM":
                elo -= 20;
                break;
            case "EASY":
                elo -= 30;
                break;
            default:
                Debug.LogError("Invalid difficulty type.");
                return;
        }
        
        Debug.Log($"Elo updated for {Username}: New Elo: {elo}");
    }
}
