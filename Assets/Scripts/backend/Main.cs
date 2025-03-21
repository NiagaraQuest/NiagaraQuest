using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

// Base Question class with SQLite attributes
[Table("Question")]
public class Question 
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string Category { get; set; }
    public string Qst { get; set; }
    public string Difficulty { get; set; }
}

// QCM Question class with SQLite attributes
[Table("QCMQuestion")]
public class QCMQuestion : Question
{
    // Property to handle string[] conversion for SQLite
    [Ignore]
    public string[] Choices { 
        get { return ChoicesText?.Split('|'); }
        set { ChoicesText = string.Join("|", value); }
    }
    
    // The actual column to be stored in the database
    public string ChoicesText { get; set; }
    
    public int CorrectChoice { get; set; }
}

// Open Question class with SQLite attributes
[Table("OpenQuestion")]
public class OpenQuestion : Question 
{
    public string Answer { get; set; }
}

// Profile class with SQLite attributes
[Table("Profile")]
public class Profile
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Indexed]
    public string Username { get; set; }
    
    public int Elo { get; set; }
    
    // Empty constructor required by SQLite-net
    public Profile() 
    {
        Elo = 1000;
    }
    
    public Profile(string name)
    {
        if (!IsValidUsername(name))
        {
            Debug.LogError($"Username '{name}' is not valid. Must start with a letter and be 3-16 alphanumeric or underscore characters.");
            return;
        }

        Username = name;
        Elo = 1000;
    }
    
    private bool IsValidUsername(string username)
    {
        if (string.IsNullOrEmpty(username)) return false;
        return System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z][a-zA-Z0-9_]{2,15}$");
    }

    public void AddElo(string difficulty)
    {
        switch (difficulty.ToUpper())
        {
            case "HARD":
                Elo += 30;
                break;
            case "MEDIUM":
                Elo += 20;
                break;
            case "EASY":
                Elo += 10;
                break;
            default:
                Debug.LogError("Invalid difficulty type.");
                return;
        }
        
        Debug.Log($"Elo increased for {Username}: New Elo: {Elo}");
    }

    public void SubElo(string difficulty)
    {
        switch (difficulty.ToUpper())
        {
            case "HARD":
                Elo -= 10;
                break;
            case "MEDIUM":
                Elo -= 20;
                break;
            case "EASY":
                Elo -= 30;
                break;
            default:
                Debug.LogError("Invalid difficulty type.");
                return;
        }
        
        Debug.Log($"Elo decreased for {Username}: New Elo: {Elo}");
    }
}

// Player Question History class with SQLite attributes
[Table("PlayerQuestionHistory")]
public class PlayerQuestionHistory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int PlayerId { get; set; }
    
    [Indexed]
    public int QuestionId { get; set; }
    
    public bool Correct { get; set; }
    
    public DateTime AttemptedAt { get; set; }
}

// Database Manager (singleton)
public class DatabaseManager
{
    private static DatabaseManager _instance;
    private SQLiteAsyncConnection _connection;
    
    // Singleton accessor
    public static DatabaseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DatabaseManager();
            }
            return _instance;
        }
    }

    private DatabaseManager() { }

    public async Task Initialize()
    {
        if (_connection == null)
        {
            // Get the path for the database file
            string dbPath = Path.Combine(Application.persistentDataPath, "game_database.db");
            Debug.Log($"Database path: {dbPath}");
            
            // Create the connection
            _connection = new SQLiteAsyncConnection(dbPath);
            
            // Create tables for all our models
            await _connection.CreateTableAsync<Profile>();
            await _connection.CreateTableAsync<OpenQuestion>();
            await _connection.CreateTableAsync<QCMQuestion>();
            await _connection.CreateTableAsync<PlayerQuestionHistory>();
            
            // Create index for faster lookups
            await _connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_player_question ON PlayerQuestionHistory (PlayerId, QuestionId);");
            
            Debug.Log("Database initialized successfully");
        }
    }

    // Generic CRUD operations
    public async Task<int> Insert<T>(T item) where T : new()
    {
        return await _connection.InsertAsync(item);
    }

    public async Task<List<T>> GetAll<T>() where T : new()
    {
        return await _connection.Table<T>().ToListAsync();
    }

    public async Task<T> GetById<T>(int id) where T : new()
    {
        return await _connection.FindAsync<T>(id);
    }

    public async Task<int> Update<T>(T item) where T : new()
    {
        return await _connection.UpdateAsync(item);
    }

    public async Task<int> Delete<T>(int id) where T : new()
    {
        return await _connection.DeleteAsync<T>(id);
    }

    public async Task<T> QueryFirstOrDefaultAsync<T>(string query, params object[] args) where T : new()
    {
        var result = await _connection.QueryAsync<T>(query, args);
        return result.FirstOrDefault();
    }
}

// Main MonoBehaviour class that initializes everything
public class DatabaseInitializer : MonoBehaviour
{
    // Flag to prevent multiple initializations
    private bool _initialized = false;
    
    async void Start()
    {
        if (!_initialized)
        {
            Debug.Log("Starting database initialization...");
            
            // Initialize the database
            await DatabaseManager.Instance.Initialize();
            
            // Create and save sample data
            await CreateSampleData();
            
            // Test retrieving data
            await TestDataRetrieval();
            
            _initialized = true;
            Debug.Log("Database setup complete!");
        }
    }
    
    private async Task CreateSampleData()
    {
        Debug.Log("Creating sample data...");
        
        // Create and save a profile
        Profile profile = new Profile("GameMaster42");
        await DatabaseManager.Instance.Insert(profile);
        Debug.Log($"Created profile: {profile.Username} with ID: {profile.Id}");
        
        // Create and save 5 open questions
        List<OpenQuestion> questions = new List<OpenQuestion>
        {
            new OpenQuestion 
            { 
                Category = "Science", 
                Qst = "What is the chemical symbol for gold?", 
                Difficulty = "EASY",
                Answer = "Au"
            },
            new OpenQuestion 
            { 
                Category = "History", 
                Qst = "Who was the first President of the United States?", 
                Difficulty = "EASY",
                Answer = "George Washington"
            },
            new OpenQuestion 
            { 
                Category = "Mathematics", 
                Qst = "What is the value of π (pi) to two decimal places?", 
                Difficulty = "MEDIUM",
                Answer = "3.14"
            },
            new OpenQuestion 
            { 
                Category = "Geography", 
                Qst = "What is the capital of Japan?", 
                Difficulty = "MEDIUM",
                Answer = "Tokyo"
            },
            new OpenQuestion 
            { 
                Category = "Computer Science", 
                Qst = "What does SQL stand for?", 
                Difficulty = "HARD",
                Answer = "Structured Query Language"
            }
        };
        
        foreach (var question in questions)
        {
            await DatabaseManager.Instance.Insert(question);
            Debug.Log($"Created question: {question.Category} - {question.Qst} (ID: {question.Id})");
        }
        
        // Create a sample question history entry
        PlayerQuestionHistory history = new PlayerQuestionHistory
        {
            PlayerId = profile.Id,
            QuestionId = questions[0].Id,
            Correct = true,
            AttemptedAt = DateTime.Now
        };
        
        await DatabaseManager.Instance.Insert(history);
        Debug.Log("Created sample question history entry");
    }
    
    private async Task TestDataRetrieval()
    {
        Debug.Log("Testing data retrieval...");
        
        // Get all profiles
        var profiles = await DatabaseManager.Instance.GetAll<Profile>();
        Debug.Log($"Found {profiles.Count} profiles");
        
        // Get all questions
        var questions = await DatabaseManager.Instance.GetAll<OpenQuestion>();
        Debug.Log($"Found {questions.Count} open questions");
        
        // Get history entries
        var histories = await DatabaseManager.Instance.GetAll<PlayerQuestionHistory>();
        Debug.Log($"Found {histories.Count} history entries");
        
        // Display the first profile and its associated question history
        if (profiles.Count > 0)
        {
            var profile = profiles[0];
            Debug.Log($"Profile details - Username: {profile.Username}, Elo: {profile.Elo}");
            
            // Find all questions attempted by this profile
            string query = "SELECT * FROM PlayerQuestionHistory WHERE PlayerId = ?";
            var results = await DatabaseManager.Instance._connection.QueryAsync<PlayerQuestionHistory>(query, profile.Id);
            
            Debug.Log($"Profile '{profile.Username}' has attempted {results.Count} questions");
        }
    }
}