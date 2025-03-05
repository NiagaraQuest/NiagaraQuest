using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
public class DatabaseManager
{
    private static DatabaseManager _instance;
    private SQLiteAsyncConnecation _connection;

    public static DatabaseManager Instance
    {
        get {
            if(_instance == null)
            {
                _instance = new DatabaseManager();
            }
            return _instance;
        }
    }

    public async Task Initialize(){
        if (_connection == null){
            string dbPath = Path.Combine(Application.persistentDataPath, "game_database.db");
            _database = new SQLiteAsyncConnection(dbPath);

            await _database.CreateTableAsync<Profile>();
            await _database.CreateTableAsync<QCMQuestion>();
            await _database.CreateTableAsync<OpenQuestion>();

        }
    }

    public async Task Insert<T>(T item) where T : new(){
        await _database.InsertAsync(item);
    }

     public async Task<List<T>> GetAll<T>() where T : new()
    {
        return await _database.Table<T>().ToListAsync();
    }

    public async Task<T> GetById<T>(int id) where T : new()
    {
        return await _database.FindAsync<T>(id);
    }

    public async Task Update<T>(T item) where T : new()
    {
        await _database.UpdateAsync(item);
    }

    public async Task Delete<T>(int id) where T : new()
    {
        await _database.DeleteAsync<T>(id);
    }
}



}