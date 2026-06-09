using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnService : ServiceScript
{
    [Header("Enemy Prefabs (up to 5)")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public float spawnInterval = 5f;
    public int maxSpawnedEnemies = 3;

    [Header("Probabilities (normalized)")]
    [Range(0f, 1f)] public float[] probabilities = new float[5];

    private float timer;
    private List<GameObject> activeSpawned = new List<GameObject>();

    private Transform          _playerTarget;
    private GazeManager        _playerGaze;
    private EnergyManager      _playerEnergy;
    private AttackSequenceActor _attackSeq;
    private bool _referencesReady = false;

    void Start()
    {
        NormalizeProbabilities();
        if (spawnPoint == null) spawnPoint = transform;
    }

    public void SetPlayerReferences(Transform playerTarget, GazeManager playerGaze,
                                    EnergyManager playerEnergy, AttackSequenceActor attackSeq)
    {
        _playerTarget = playerTarget;
        _playerGaze   = playerGaze;
        _playerEnergy = playerEnergy;
        _attackSeq    = attackSeq;
        _referencesReady = (playerTarget != null && playerGaze != null && playerEnergy != null && attackSeq != null);
    }

    void Update()
    {
        if (!_referencesReady) return;
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

        Vector3 spawnPos = spawnPoint.position;
        GameObject instance = Instantiate(enemyPrefabs[index], spawnPos, Quaternion.identity);

        var refs = instance.GetComponentInChildren<EnemyReferences>();
        if (refs != null)
            refs.SetReferences(_playerTarget, _playerGaze, _playerEnergy, _attackSeq);

        var structMgr = instance.GetComponentInChildren<StructManager>();
        if (structMgr != null)
        {
            UnityAction destroyCallback = null;
            destroyCallback = () =>
            {
                structMgr.OnEntityDestroyed.RemoveListener(destroyCallback);
                activeSpawned.Remove(instance);
            };
            structMgr.OnEntityDestroyed.AddListener(destroyCallback);
        }

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