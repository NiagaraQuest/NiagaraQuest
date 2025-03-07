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
    private SQLiteAsyncConnection _connection;

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
            string dbPath = Path.Combine(Application.persistentDataPath, "game_database.db");
            _connection = new SQLiteAsyncConnection(dbPath);

            await _connection.CreateTableAsync<Profile>();
            await _connection.CreateTableAsync<QCMQuestion>();
            await _connection.CreateTableAsync<OpenQuestion>();
            await _connection.CreateTableAsync<PlayerQuestionHistory>();
            
            await _connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_player_question ON PlayerQuestionHistory (PlayerId, QuestionId);");
        }
    }

    public async Task Insert<T>(T item) where T : new()
    {
        await _connection.InsertAsync(item);
    }

    public async Task<List<T>> GetAll<T>() where T : new()
    {
        return await _connection.Table<T>().ToListAsync();
    }

    public async Task<T> GetById<T>(int id) where T : new()
    {
        return await _connection.FindAsync<T>(id);
    }

    public async Task Update<T>(T item) where T : new()
    {
        await _connection.UpdateAsync(item);
    }

    public async Task Delete<T>(int id) where T : new()
    {
        await _connection.DeleteAsync<T>(id);
    }

    public async Task<T> QueryFirstOrDefaultAsync<T>(string query, params object[] args) where T : new()
    {
        var result = await _connection.QueryAsync<T>(query, args);
        return result.FirstOrDefault();
    }
}
