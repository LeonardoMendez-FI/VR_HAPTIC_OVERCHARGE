using System;
using System.Collections.Generic;
using System.IO;
using SQLite;
using UnityEngine;

public class RunRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string timestamp { get; set; }
    public int robotsDestroyed { get; set; }
    public float damageDealt { get; set; }
    public float damageReceived { get; set; }
    public float energyAbsorbed { get; set; }
    public int levelReached { get; set; }
    public bool victory { get; set; }
}

public class DatabaseManager : ServiceScript
{
    [Header("Database Settings")]
    public string databaseName = "runs.db";

    private string dbPath;
    private SQLiteConnection db;

    void Awake()
    {
        dbPath = Path.Combine(Application.persistentDataPath, databaseName);
        db = new SQLiteConnection(dbPath);
        db.CreateTable<RunRecord>();
        Debug.Log($"[DatabaseManager] Base de datos lista en: {dbPath}");
    }

    void OnDestroy() => db?.Close();

    public int SaveRun(RunRecord run)
    {
        run.timestamp = DateTime.UtcNow.ToString("o");
        int id = db.Insert(run);
        Debug.Log($"[DatabaseManager] Run guardada con ID {id}");
        return id;
    }

    public RunRecord GetBestRun()
    {
        var list = db.Query<RunRecord>("SELECT * FROM RunRecord ORDER BY robotsDestroyed DESC, damageDealt DESC LIMIT 1");
        return list.Count > 0 ? list[0] : new RunRecord();
    }

    public List<RunRecord> GetAllRuns() => db.Table<RunRecord>().ToList();
}