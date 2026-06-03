using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalSpawner : ServiceScript
{
    [Header("Enemy Prefabs")]
    public List<GameObject> enemyPrefabs;   // solo enemigos voladores, por ahora uno

    [Header("Spawn Settings")]
    public float initialSpawnDelay = 2f;
    public float spawnRadius = 30f;         // radio alrededor del centro donde aparecen
    public Transform centerPoint;           // centro del mapa
    public Transform player;                // objetivo de los enemigos (auto: tag "Player" o Camera.main)

    [Header("Difficulty Progression")]
    public int baseEnemiesPerWave = 3;
    public int extraEnemiesPerWave = 2;     // más enemigos con cada oleada
    public float timeBetweenWaves = 10f;
    public float waveTimeReduction = 0.5f;  // las oleadas se aceleran
    public float enemySpeedIncrease = 0.2f; // incremento de velocidad por oleada
    public float enemyDamageIncrease = 0.1f;// incremento de daño por oleada

    private int currentWave = 0;
    private int enemiesRemaining = 0;
    private bool spawningWave = false;
    private float waveTimer;
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        waveTimer = initialSpawnDelay;

        // FIX: centro por defecto = este objeto (evita NullReference si no se asignó).
        if (centerPoint == null) centerPoint = transform;

        // FIX: resolver al jugador para que los enemigos lo persigan. Los prefabs no
        // pueden referenciarlo en el editor, así que lo buscamos en runtime:
        // 1) GameObject con tag "Player", 2) cámara principal (cabeza VR del jugador).
        if (player == null)
        {
            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null) player = tagged.transform;
            else if (Camera.main != null) player = Camera.main.transform;
        }
    }

    void Update()
    {
        // Limpiar enemigos destruidos
        activeEnemies.RemoveAll(e => e == null);

        enemiesRemaining = activeEnemies.Count;

        if (!spawningWave && enemiesRemaining <= 0)
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0f)
            {
                spawningWave = true;
                StartCoroutine(SpawnWave());
            }
        }
    }

    IEnumerator SpawnWave()
    {
        // FIX: sin prefabs no hay nada que spawnear (evita IndexOutOfRange y log claro).
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("[SurvivalSpawner] enemyPrefabs está vacío: no se puede spawnear.");
            spawningWave = false;
            waveTimer = timeBetweenWaves;
            yield break;
        }

        currentWave++;
        int enemiesToSpawn = baseEnemiesPerWave + (currentWave - 1) * extraEnemiesPerWave;

        Debug.Log($"[SurvivalSpawner] Wave {currentWave}: spawning {enemiesToSpawn} enemies");

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // Posición aleatoria alrededor del centro
            Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
            randomPos.y = 0f; // manténlos a nivel del suelo, ellos volarán a su altura
            Vector3 spawnPos = centerPoint.position + randomPos;

            // Elegir un prefab aleatorio (de momento solo tenemos uno)
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Ajustar estadísticas según la oleada
            FlyingRangedEnemy flyer = enemy.GetComponentInChildren<FlyingRangedEnemy>();
            if (flyer != null)
            {
                flyer.speed += currentWave * enemySpeedIncrease;
                flyer.damage += currentWave * enemyDamageIncrease;
                flyer.playerTarget = player;   // FIX: sin esto el enemigo no persigue ni dispara
            }

            // Suscribirnos a la muerte del enemigo para no depender solo de RemoveAll
            StructManager structMgr = enemy.GetComponentInChildren<StructManager>();
            if (structMgr != null)
                structMgr.OnEntityDestroyed.AddListener(() => activeEnemies.Remove(enemy));

            activeEnemies.Add(enemy);

            yield return new WaitForSeconds(0.5f); // pequeño intervalo entre spawns
        }

        spawningWave = false;
        waveTimer = Mathf.Max(2f, timeBetweenWaves - currentWave * waveTimeReduction);
    }
}
