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
            Debug.LogError($"[DatabaseService] Error al abrir/crear la base de datos: {e.Message}");
        }
    }

    void OnDestroy()
    {
        try
        {
            db?.Close();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DatabaseService] Error al cerrar la base de datos: {e.Message}");
        }
    }

    public int SaveRun(RunRecord run)
    {
        if (db == null)
        {
            Debug.LogWarning("[DatabaseService] No se puede guardar: base de datos no disponible.");
            return -1;
        }

        try
        {
            run.timestamp = DateTime.UtcNow.ToString("o");
            int id = db.Insert(run);
            Debug.Log($"[DatabaseService] Run guardada con ID {id}");
            return id;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DatabaseService] Error al guardar run: {e.Message}");
            return -1;
        }
    }

    public void UpdateRun(RunRecord run)
    {
        if (db == null) return;
        try
        {
            db.Update(run);
            Debug.Log($"[DatabaseService] Run ID {run.Id} actualizada.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DatabaseService] Error al actualizar run: {e.Message}");
        }
    }

    public RunRecord GetBestRun()
    {
        if (db == null) return new RunRecord();
        try
        {
            var list = db.Query<RunRecord>("SELECT * FROM RunRecord ORDER BY robotsDestroyed DESC, damageDealt DESC LIMIT 1");
            return list.Count > 0 ? list[0] : new RunRecord();
        }
        catch (Exception e)
        {
            Debug.LogError($"[DatabaseService] Error al obtener mejor run: {e.Message}");
            return new RunRecord();
        }
    }

    public List<RunRecord> GetAllRuns()
    {
        if (db == null) return new List<RunRecord>();
        try
        {
            return db.Table<RunRecord>().ToList();
        }
        catch (Exception e)
        {
            Debug.LogError($"[DatabaseService] Error al obtener todas las runs: {e.Message}");
            return new List<RunRecord>();
        }
    }

    public void DeleteAllRuns()
    {
        if (db == null) return;
        try
        {
            db.DeleteAll<RunRecord>();
            Debug.Log("[DatabaseService] Todas las runs han sido eliminadas.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DatabaseService] Error al borrar runs: {e.Message}");
        }
    }
}