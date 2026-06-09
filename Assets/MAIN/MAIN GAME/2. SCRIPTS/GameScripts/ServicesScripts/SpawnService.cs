using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnService : ServiceScript
{
    [Header("Enemy Prefabs (up to 5)")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public float     spawnInterval      = 5f;
    public int       maxSpawnedEnemies  = 3;

    [Header("Probabilities (normalized)")]
    [Range(0f, 1f)] public float[] probabilities = new float[5];

    private float              timer;
    private List<GameObject>   activeSpawned = new List<GameObject>();

    private Transform      _playerTarget;
    private GazeManager    _playerGaze;
    private EnergyManager  _playerEnergy;
    private bool           _referencesReady = false;

    // Tracks per-enemy destroy callbacks so they can be explicitly removed,
    // preventing the listener leak caused by anonymous lambdas (which cannot
    // be unregistered from UnityEvent).
    private Dictionary<GameObject, UnityAction> _destroyCallbacks
        = new Dictionary<GameObject, UnityAction>();

    private void Start()
    {
        NormalizeProbabilities();
        if (spawnPoint == null)
            spawnPoint = transform;
    }

    public void SetPlayerReferences(Transform playerTarget, GazeManager playerGaze, EnergyManager playerEnergy)
    {
        _playerTarget    = playerTarget;
        _playerGaze      = playerGaze;
        _playerEnergy    = playerEnergy;
        _referencesReady = (playerTarget != null && playerGaze != null && playerEnergy != null);
    }

    private void Update()
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

    private void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0) return;

        int index = PickRandomIndex();
        if (index < 0 || index >= enemyPrefabs.Count) return;

        GameObject instance = Instantiate(enemyPrefabs[index], spawnPoint.position, Quaternion.identity);

        var refs = instance.GetComponentInChildren<EnemyReferences>();
        if (refs != null)
            refs.SetReferences(_playerTarget, _playerGaze, _playerEnergy);

        var structMgr = instance.GetComponentInChildren<StructManager>();
        if (structMgr != null)
        {
            // Store the callback as a named UnityAction so it can be removed later.
            // Anonymous lambdas cannot be removed from UnityEvent — they create a
            // permanent subscription that leaks the closure even after the enemy
            // GameObject is destroyed.
            UnityAction callback = null;
            callback = () => OnEnemyDestroyed(instance, structMgr, callback);
            _destroyCallbacks[instance] = callback;
            structMgr.OnEntityDestroyed.AddListener(callback);
        }

        activeSpawned.Add(instance);
    }

    private void OnEnemyDestroyed(GameObject enemy, StructManager structMgr, UnityAction callback)
    {
        // Remove the listener immediately so the UnityEvent holds no further
        // reference to this callback or the captured variables.
        if (structMgr != null)
            structMgr.OnEntityDestroyed.RemoveListener(callback);

        _destroyCallbacks.Remove(enemy);
        activeSpawned.Remove(enemy);
    }

    private void OnDestroy()
    {
        // Clean up any callbacks for enemies still alive when this service is torn down.
        foreach (var kvp in _destroyCallbacks)
        {
            if (kvp.Key == null) continue;
            var structMgr = kvp.Key.GetComponentInChildren<StructManager>();
            if (structMgr != null)
                structMgr.OnEntityDestroyed.RemoveListener(kvp.Value);
        }
        _destroyCallbacks.Clear();
    }

    private int PickRandomIndex()
    {
        float roll       = Random.value;
        float cumulative = 0f;
        int   lastIndex  = probabilities.Length - 1;

        for (int i = 0; i < lastIndex; i++)
        {
            cumulative += probabilities[i];
            if (roll < cumulative) return i;
        }
        return lastIndex;
    }

    private void NormalizeProbabilities()
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
