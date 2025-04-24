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
            
            // Create synchronous connection first to setup tables and indexes
            await Task.Run(() => {
                using (var connection = new SQLiteConnection(profileDbPath))
                {
                    connection.CreateTable<Profile>();
                    // Create username index with unique constraint
                    connection.Execute("CREATE UNIQUE INDEX IF NOT EXISTS idx_username ON Profiles (Username)");
                }
            });
            
            _profileConnection = new SQLiteAsyncConnection(profileDbPath);
            // Perform a simple async operation to ensure connection is established
            await _profileConnection.ExecuteScalarAsync<int>("SELECT 1");
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
            await Task.Run(() => {
                using (var connection = new SQLiteConnection(questionsPath))
                {
                    connection.CreateTable<QCMQuestion>();
                    connection.CreateTable<OpenQuestion>();
                    connection.CreateTable<TrueFalseQuestion>();
                    connection.CreateTable<PlayerQuestionHistory>();
                    
                    // Create indexes separately with all columns having same properties
                    connection.Execute("CREATE INDEX IF NOT EXISTS idx_player_id ON PlayerQuestionHistory (PlayerId)");
                    connection.Execute("CREATE INDEX IF NOT EXISTS idx_question_id ON PlayerQuestionHistory (QuestionId)");
                    connection.Execute("CREATE INDEX IF NOT EXISTS idx_question_type ON PlayerQuestionHistory (QuestionType)");
                    connection.Execute("CREATE INDEX IF NOT EXISTS idx_player_question ON PlayerQuestionHistory (PlayerId, QuestionId, QuestionType)");
                }
            });
            
            Debug.Log("Questions database initialized with tables");

            _questionConnection = new SQLiteAsyncConnection(questionsPath);
            // Perform a simple async operation to ensure connection is established
            await _questionConnection.ExecuteScalarAsync<int>("SELECT 1");
            Debug.Log("Question database connection established");
        }

        Debug.Log("Database initialization completed successfully");
    }

    public async Task Insert<T>(T item) where T : new()
    {
        var connection = GetAppropriateConnection<T>();
        await connection.InsertAsync(item);
    }

    public async Task InsertAll<T>(IEnumerable<T> items) where T : new()
    {
        var connection = GetAppropriateConnection<T>();
        await connection.InsertAllAsync(items);
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

    public async Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : new()
    {
        var connection = GetAppropriateConnection<T>();
        return await connection.QueryAsync<T>(query, args);
    }

    public async Task<T> QueryFirstOrDefaultAsync<T>(string query, params object[] args) where T : new()
    {
        var connection = GetAppropriateConnection<T>();
        var result = await connection.QueryAsync<T>(query, args);
        return result.FirstOrDefault();
    }
    
    // New method for counting records
    public async Task<int> CountAsync<T>(string whereClause = null, params object[] args) where T : new()
    {
        var connection = GetAppropriateConnection<T>();
        string query;
        
        if (string.IsNullOrEmpty(whereClause))
            query = $"SELECT COUNT(*) FROM {typeof(T).Name}s";
        else
            query = $"SELECT COUNT(*) FROM {typeof(T).Name}s WHERE {whereClause}";
            
        return await connection.ExecuteScalarAsync<int>(query, args);
    }

    public async Task<int> ExecuteAsync(string query, params object[] args)
    {
        // Execute on both connections for simplicity
        // In a real-world scenario, you might want to be more specific
        int profileResult = await _profileConnection.ExecuteAsync(query, args);
        int questionResult = await _questionConnection.ExecuteAsync(query, args);
        
        // Return the sum of affected rows
        return profileResult + questionResult;
    }
    
    // Special method for utility queries that shouldn't use GetAppropriateConnection
    public async Task<int> ExecuteScalarAsync(string query, params object[] args)
    {
        // Choose the question connection by default for utility queries
        return await _questionConnection.ExecuteScalarAsync<int>(query, args);
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
                 typeof(T) == typeof(TrueFalseQuestion) || 
                 typeof(T) == typeof(PlayerQuestionHistory))
        {
            return _questionConnection;
        }
        
        throw new ArgumentException($"Unknown type {typeof(T).Name} - cannot determine appropriate database");
    }
}