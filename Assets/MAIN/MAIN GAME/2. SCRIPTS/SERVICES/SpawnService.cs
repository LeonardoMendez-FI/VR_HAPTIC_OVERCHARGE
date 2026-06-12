using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnService : ServiceScript
{
    [Header("Enemy Prefabs (hasta 5)")]
    public GameObject[] enemyPrefabs;   // asigna aquí los 5 prefabs

    [Header("Spawn Probabilities (mismo orden que prefabs)")]
    [Range(0f, 1f)] public float[] probabilities = { 0.6f, 0.3f, 0.1f, 0f, 0f };

    [Header("Spawn Timing")]
    public float spawnInterval = 3f;          // cada cuántos segundos intenta spawnear
    public float initialDelay = 0f;           // espera antes de empezar

    [Header("Limits")]
    public int maxActiveEnemies = 5;          // máximo de enemigos simultáneos

    [Header("Spawn Points (opcional)")]
    public Transform[] spawnPoints;
    public Transform defaultSpawnPoint;

    // Referencias del jugador (inyectadas)
    private Transform _playerTarget;
    private GazeManager _playerGaze;
    private EnergyManager _playerEnergy;
    private AttackSequenceActor _attackSeq;
    private bool _referencesReady = false;

    // Estado interno
    private readonly List<GameObject> _activeEnemies = new();
    private bool _spawning = false;
    private Coroutine _spawnRoutine;

    void Start()
    {
        NormalizeProbabilities();
        if (defaultSpawnPoint == null) defaultSpawnPoint = transform;
    }

    public void SetPlayerReferences(Transform playerTarget, GazeManager playerGaze,
                                    EnergyManager playerEnergy, AttackSequenceActor attackSeq)
    {
        _playerTarget = playerTarget;
        _playerGaze = playerGaze;
        _playerEnergy = playerEnergy;
        _attackSeq = attackSeq;
        _referencesReady = (playerTarget != null && playerGaze != null && playerEnergy != null && attackSeq != null);
    }

    public void SetProbabilities(float[] probs)
    {
        probabilities = probs;
        NormalizeProbabilities();
    }

    /// <summary>
    /// Comienza el spawn continuo.
    /// </summary>
    public void StartSpawning()
    {
        if (_spawning) return;
        _spawning = true;
        _spawnRoutine = StartCoroutine(SpawnLoop());
        Debug.Log($"[SpawnService] Spawn continuo iniciado. Intervalo: {spawnInterval}s, máximo activos: {maxActiveEnemies}");
    }

    /// <summary>
    /// Detiene el spawn (por si se necesita).
    /// </summary>
    public void StopSpawning()
    {
        _spawning = false;
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (_spawning)
        {
            // Limpiar enemigos muertos (por si acaso)
            _activeEnemies.RemoveAll(e => e == null);

            if (_activeEnemies.Count < maxActiveEnemies)
            {
                SpawnRandomEnemy();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnRandomEnemy()
    {
        if (!_referencesReady)
        {
            Debug.LogWarning("[SpawnService] Referencias del jugador no listas.");
            return;
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[SpawnService] No hay prefabs asignados.");
            return;
        }

        // Elegir un prefab según probabilidades
        int index = PickRandomIndex();
        GameObject prefab = enemyPrefabs[index];
        if (prefab == null) return;

        // Elegir punto de spawn
        Transform spawnPoint = (spawnPoints != null && spawnPoints.Length > 0)
            ? spawnPoints[Random.Range(0, spawnPoints.Length)]
            : defaultSpawnPoint;

        if (spawnPoint == null) return;

        GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // Inyectar referencias del jugador
        EnemyReferences refs = enemy.GetComponent<EnemyReferences>()
                            ?? enemy.GetComponentInChildren<EnemyReferences>();
        if (refs != null)
            refs.SetReferences(_playerTarget, _playerGaze, _playerEnergy, _attackSeq);
        else
            Debug.LogError($"[SpawnService] El prefab {prefab.name} no tiene EnemyReferences.", prefab);

        // Registrar para control de activos
        _activeEnemies.Add(enemy);

        // Cuando el enemigo muera, quitarlo de la lista
        StructManager enemyStruct = enemy.GetComponent<StructManager>()
                                 ?? enemy.GetComponentInChildren<StructManager>();
        if (enemyStruct != null)
        {
            enemyStruct.OnEntityDestroyed.AddListener(() =>
            {
                _activeEnemies.Remove(enemy);
                Debug.Log($"[SpawnService] Enemigo destruido. Activos: {_activeEnemies.Count}");
            });
        }

        Debug.Log($"[SpawnService] Enemigo {prefab.name} generado. Activos: {_activeEnemies.Count}");
    }

    // Selecciona un índice basado en probabilidades normalizadas
    private int PickRandomIndex()
    {
        float roll = Random.value;
        float cumulative = 0f;
        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (roll < cumulative) return i;
        }
        return probabilities.Length - 1;
    }

    private void NormalizeProbabilities()
    {
        float sum = 0f;
        foreach (float p in probabilities) sum += p;
        if (sum <= 0f) return;
        for (int i = 0; i < probabilities.Length; i++)
            probabilities[i] /= sum;
    }

    void OnDestroy()
    {
        StopSpawning();
        foreach (var enemy in _activeEnemies)
            if (enemy != null) Destroy(enemy);
        _activeEnemies.Clear();
    }
}