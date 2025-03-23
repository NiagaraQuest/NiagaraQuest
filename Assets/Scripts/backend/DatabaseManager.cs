using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

public class DatabaseManager
{
    private static DatabaseManager _instance;
    private SQLiteAsyncConnection _profileConnection;
    private SQLiteAsyncConnection _questionConnection;

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
        Debug.Log("Starting database initialization...");
        
        if (_profileConnection == null)
        {
            string profileDbPath = Path.Combine(Application.persistentDataPath, "profiles.db");
            Debug.Log($"Creating profiles database at: {profileDbPath}");
            _profileConnection = new SQLiteAsyncConnection(profileDbPath);
            await _profileConnection.CreateTableAsync<Profile>();
        }

        if (_questionConnection == null)
        {
            string questionsPath = Path.Combine(Application.streamingAssetsPath, "questions.db");
            Debug.Log($"Questions database path: {questionsPath}");

            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
                Debug.Log("Created StreamingAssets directory");
            }

            Debug.Log("Creating new questions database...");
            using (var connection = new SQLiteConnection(questionsPath))
            {
                connection.CreateTable<QCMQuestion>();
                connection.CreateTable<OpenQuestion>();
                connection.CreateTable<FillTheGapsQuestion>();
                connection.CreateTable<TrueFalseQuestion>();
                connection.CreateTable<RankingQuestion>();
                connection.CreateTable<PlayerQuestionHistory>();
                connection.Execute("CREATE INDEX IF NOT EXISTS idx_player_question ON PlayerQuestionHistory (PlayerId, QuestionId)");
            }
            Debug.Log("Questions database initialized with tables");

            _questionConnection = new SQLiteAsyncConnection(questionsPath);
            Debug.Log("Question database connection established");
        }

        Debug.Log("Database initialization completed successfully");
    }

    public async Task Insert<T>(T item) where T : new()
    {
        var connection = GetAppropriateConnection<T>();
        await connection.InsertAsync(item);
    }

    public async Task<List<T>> GetAll<T>() where T : new()
    {
        var connection = GetAppropriateConnection<T>();
        return await connection.Table<T>().ToListAsync();
    }

    public async Task<T> GetById<T>(int id) where T : new()
    {
        var connection = GetAppropriateConnection<T>();
        return await connection.FindAsync<T>(id);
    }

    public async Task Update<T>(T item) where T : new()
    {
        var connection = GetAppropriateConnection<T>();
        await connection.UpdateAsync(item);
    }

    public async Task Delete<T>(int id) where T : new()
    {
        var connection = GetAppropriateConnection<T>();
        await connection.DeleteAsync<T>(id);
    }

    public async Task<T> QueryFirstOrDefaultAsync<T>(string query, params object[] args) where T : new()
    {
        var connection = GetAppropriateConnection<T>();
        var result = await connection.QueryAsync<T>(query, args);
        return result.FirstOrDefault();
    }

    private SQLiteAsyncConnection GetAppropriateConnection<T>() where T : new()
    {
        // Profile-related tables go to profile database
        if (typeof(T) == typeof(Profile))
        {
            return _profileConnection;
        }
        // Question-related tables go to question database
        else if (typeof(T) == typeof(QCMQuestion) || 
                typeof(T) == typeof(OpenQuestion) || 
                typeof(T) == typeof(PlayerQuestionHistory))
        {
            return _questionConnection;
        }
        
        throw new ArgumentException($"Unknown type {typeof(T).Name} - cannot determine appropriate database");
    }

    public async Task<object> GetRandomQuestion()
{
    if (_questionConnection == null)
    {
        Debug.LogError("❌ Question database connection is not initialized!");
        return null;
    }

    // List of question table names
    string[] questionTables = new string[]
    {
        "QCMQuestion",
        "OpenQuestion",
        "FillTheGapsQuestion",
        "TrueFalseQuestion",
        "RankingQuestion"
    };

    // Choose a random table
    string selectedTable = questionTables[UnityEngine.Random.Range(0, questionTables.Length)];
    Debug.Log($"🎲 Selecting random question from {selectedTable}");

    // Fetch a random question
    string query = $"SELECT * FROM {selectedTable} ORDER BY RANDOM() LIMIT 1";

    var result = await _questionConnection.QueryAsync<object>(query); // Fix: Use generic <object>

    if (result != null && result.Count > 0)
    {
        return result[0]; // Return the first (random) result
    }
    else
    {
        Debug.LogWarning($"⚠️ No questions found in table {selectedTable}");
        return null;
    }
}

}
