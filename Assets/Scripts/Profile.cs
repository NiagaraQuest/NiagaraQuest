using UnityEngine;
using System;
using System.Text.RegularExpressions;

public class Profile
{
    private static int idCounter = 0;
    private int[] elo;
    
    public int Id { get; private set; }
    public string Username { get; private set; }

    private bool IsValidUsername(string username)
    {
        string pattern = @"^[a-zA-Z][a-zA-Z0-9_]{2,15}$";
        return Regex.IsMatch(username, pattern);
    }

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
        elo = new int[5];

        for (int i = 0; i < 5; i++)
        {
            elo[i] = 1000;
        }
    }

    public int GetEloByCategory(int category)
    {
        if (category < 1 || category > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(category), "Category must be between 1 and 5.");
        }
        return elo[category - 1]; // Adjust for zero-based indexing
    }

    public void AddElo(int category, string difficulty)
    {
        if (category < 1 || category > 5)
        {
            Debug.LogError("Invalid category.");
            return;
        }

        int index = category - 1;
        switch (difficulty.ToUpper())
        {
            case "HARD":
                elo[index] += 30;
                break;
            case "MEDIUM":
                elo[index] += 20;
                break;
            case "EASY":
                elo[index] += 10;
                break;
            default:
                Debug.LogError("Invalid difficulty type.");
                return;
        }
        
        Debug.Log($"Elo updated for {Username}: Category {category}, New Elo: {elo[index]}");
    }

    public void SubElo(int category, string difficulty)
    {
        if (category < 1 || category > 5)
        {
            Debug.LogError("Invalid category.");
            return;
        }

        int index = category - 1;
        switch (difficulty.ToUpper())
        {
            case "HARD":
                elo[index] -= 10;
                break;
            case "MEDIUM":
                elo[index] -= 20;
                break;
            case "EASY":
                elo[index] -= 30;
                break;
            default:
                Debug.LogError("Invalid difficulty type.");
                return;
        }
        
        Debug.Log($"Elo updated for {Username}: Category {category}, New Elo: {elo[index]}");
    }
}
