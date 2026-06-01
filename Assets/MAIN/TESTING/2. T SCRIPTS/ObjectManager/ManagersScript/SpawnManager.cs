using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : ServiceScript
{
    [Header("Enemy Prefabs (up to 5)")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public int maxSpawnedEnemies = 3;           // límite de enemigos activos generados por esta máquina

    [Header("Probabilities (normalized)")]
    [Range(0f, 1f)] public float[] probabilities = new float[5];

    private float timer;
    private List<GameObject> activeSpawned = new List<GameObject>();

    void Start()
    {
        NormalizeProbabilities();
    }

    void Update()
    {
        // Limpiar enemigos muertos de la lista
        activeSpawned.RemoveAll(e => e == null);

        if (activeSpawned.Count >= maxSpawnedEnemies) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0) return;

        int index = PickRandomIndex();
        if (index < 0 || index >= enemyPrefabs.Count) return;

        // Spawn exactamente en la posición del SpawnManager
        Vector3 spawnPos = transform.position;
        GameObject instance = Instantiate(enemyPrefabs[index], spawnPos, Quaternion.identity);
        activeSpawned.Add(instance);
    }

    int PickRandomIndex()
    {
        float roll = Random.value;
        float cumulative = 0f;
        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (roll <= cumulative) return i;
        }
        return 0; // fallback
    }

    void NormalizeProbabilities()
    {
        float sum = 0f;
        foreach (float p in probabilities) sum += p;
        if (sum <= 0f) return;
        for (int i = 0; i < probabilities.Length; i++)
            probabilities[i] /= sum;
    }

    /// <summary>Permite al LevelManager asignar probabilidades externamente.</summary>
    public void SetProbabilities(float[] probs)
    {
        probabilities = probs;
        NormalizeProbabilities();
    }
}