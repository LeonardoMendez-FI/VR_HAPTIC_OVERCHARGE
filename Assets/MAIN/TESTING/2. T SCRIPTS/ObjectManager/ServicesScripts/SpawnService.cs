using System.Collections.Generic;
using UnityEngine;

public class SpawnService : ServiceScript
{
    [Header("Enemy Prefabs (up to 5)")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public int maxSpawnedEnemies = 3;

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

        Vector3 spawnPos = transform.position;
        GameObject instance = Instantiate(enemyPrefabs[index], spawnPos, Quaternion.identity);

        var structMgr = instance.GetComponentInChildren<StructManager>();
        if (structMgr != null)
            structMgr.OnEntityDestroyed.AddListener(() => OnEnemyDestroyed(instance));

        activeSpawned.Add(instance);
    }

    void OnEnemyDestroyed(GameObject enemy)
    {
        activeSpawned.Remove(enemy);
    }

    int PickRandomIndex()
    {
        float roll = Random.value;
        float cumulative = 0f;
        int lastIndex = probabilities.Length - 1;
        for (int i = 0; i < lastIndex; i++)
        {
            cumulative += probabilities[i];
            if (roll < cumulative) return i;
        }
        return lastIndex;
    }

    void NormalizeProbabilities()
    {
        float sum = 0f;
        foreach (float p in probabilities) sum += p;
        if (sum <= 0f) return;
        for (int i = 0; i < probabilities.Length; i++)
            probabilities[i] /= sum;
    }

    public void SetProbabilities(float[] probs)
    {
        probabilities = probs;
        NormalizeProbabilities();
    }
}