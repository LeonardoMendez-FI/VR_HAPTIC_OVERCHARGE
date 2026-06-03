using System;
using System.IO;
using SQLite;
using UnityEngine;

public class DatabaseService : ServiceScript
{
    [Header("Database Settings")]
    public string databaseName = "runs.db";

    private string dbPath;
    private SQLiteConnection db;

    void Awake()
    {
        dbPath = Path.Combine(Application.persistentDataPath, databaseName);
        try
        {
            db = new SQLiteConnection(dbPath);
            db.CreateTable<RunRecord>();
            Debug.Log($"[DatabaseService] Base de datos lista en: {dbPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DatabaseService] Error al abrir/crear DB: {e.Message}");
        }
    }

    void OnDestroy()
    {
        try { db?.Close(); }
        catch (Exception e) { Debug.LogWarning($"[DatabaseService] Error al cerrar DB: {e.Message}"); }
    }

    public void SaveCurrentRun(RunRecord run)
    {
        run.runType = "Current";
        ReplaceRunByType("Current", run);
    }

    public void SaveBestRun(RunRecord run)
    {
        run.runType = "Best";
        ReplaceRunByType("Best", run);
    }

    void ReplaceRunByType(string type, RunRecord run)
    {
        if (db == null) return;
        try
        {
            // Borrar el registro anterior del mismo tipo
            db.Execute($"DELETE FROM RunRecord WHERE runType = ?", type);
            run.timestamp = DateTime.UtcNow.ToString("o");
            db.Insert(run);
            Debug.Log($"[DatabaseService] Run '{type}' guardada con score={run.score}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DatabaseService] Error guardando run '{type}': {e.Message}");
        }
    }

    public RunRecord GetCurrentRun()
    {
        return GetRunByType("Current");
    }

    public RunRecord GetBestRun()
    {
        return GetRunByType("Best");
    }

    RunRecord GetRunByType(string type)
    {
        if (db == null) return new RunRecord();
        try
        {
            var list = db.Query<RunRecord>($"SELECT * FROM RunRecord WHERE runType = ?", type);
            return list.Count > 0 ? list[0] : new RunRecord();
        }
        catch (Exception e)
        {
            Debug.LogError($"[DatabaseService] Error obteniendo run '{type}': {e.Message}");
            return new RunRecord();
        }
    }
}