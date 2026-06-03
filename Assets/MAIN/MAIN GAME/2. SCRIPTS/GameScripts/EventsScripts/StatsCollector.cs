using UnityEngine;

public class StatsCollector : MonoBehaviour
{
    [Header("References")]
    public DatabaseService databaseService;
    public AttackManager attackManager;
    public StructManager structManager;
    public GazeEnergyDrainActor drainActor;
    public LevelService levelService;
    public EndGameUI endGameUI;

    [Header("Score Multipliers")]
    public float pLevel     = 100f;   // puntos por nivel alcanzado
    public float pRobots    = 10f;    // puntos por robot destruido
    public float pMachines  = 50f;    // puntos por máquina destruida
    public float pEnergy    = 1f;     // puntos por unidad de energía absorbida
    public float pTime      = -1f;    // penalización por segundo (negativo = menos tiempo mejor)

    // Contadores internos
    private int robotsDestroyed;
    private int machinesDestroyed;
    private float damageDealt;
    private float damageReceived;
    private float energyAbsorbed;
    private int levelReached = 1;
    private float gameTime;
    private bool gameEnded;
    private float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    void OnEnable()
    {
        if (attackManager != null)
        {
            attackManager.OnEliminationCountChanged.AddListener(OnElimination);
            attackManager.OnDamageDealt.AddListener(OnDamageDealt);
        }
        if (structManager != null)
        {
            structManager.OnDamagedWithDirection.AddListener(OnDamaged);
            structManager.OnEntityDestroyed.AddListener(OnPlayerDeath);
        }
        if (drainActor != null)
            drainActor.onEnergyAbsorbed.AddListener(OnEnergyAbsorbed);
        if (levelService != null)
            levelService.OnMachinesRemainingChanged.AddListener(OnMachinesRemainingChanged);
    }

    void OnDisable()
    {
        if (attackManager != null)
        {
            attackManager.OnEliminationCountChanged.RemoveListener(OnElimination);
            attackManager.OnDamageDealt.RemoveListener(OnDamageDealt);
        }
        if (structManager != null)
        {
            structManager.OnDamagedWithDirection.RemoveListener(OnDamaged);
            structManager.OnEntityDestroyed.RemoveListener(OnPlayerDeath);
        }
        if (drainActor != null)
            drainActor.onEnergyAbsorbed.RemoveListener(OnEnergyAbsorbed);
        if (levelService != null)
            levelService.OnMachinesRemainingChanged.RemoveListener(OnMachinesRemainingChanged);
    }

    void OnElimination(int count) => robotsDestroyed = count;
    void OnDamageDealt(float dmg) => damageDealt += dmg;

    void OnDamaged(Vector3 attackerPos)
    {
        damageReceived += structManager.lastDamageReceived;
    }

    void OnPlayerDeath()
    {
        EndGame(false);
    }

    void OnEnergyAbsorbed(float amount) => energyAbsorbed += amount;

    // Contar máquinas destruidas a partir del cambio en la cantidad restante
    // (se asume que al inicio levelService conoce el total)
    private int lastMachinesRemaining = -1;
    void OnMachinesRemainingChanged(int remaining)
    {
        if (lastMachinesRemaining < 0) lastMachinesRemaining = remaining;
        int destroyed = lastMachinesRemaining - remaining;
        if (destroyed > 0) machinesDestroyed += destroyed;
        lastMachinesRemaining = remaining;
    }

    public void EndGame(bool victory)
    {
        if (gameEnded) return;
        gameEnded = true;

        gameTime = Time.time - startTime;
        levelReached = levelService != null ? levelService.currentLevel : 1;

        // Calcular puntuación
        float score = levelReached * pLevel
                    + robotsDestroyed * pRobots
                    + machinesDestroyed * pMachines
                    + energyAbsorbed * pEnergy
                    + gameTime * pTime;

        var currentRun = new RunRecord
        {
            robotsDestroyed = robotsDestroyed,
            machinesDestroyed = machinesDestroyed,
            damageDealt = damageDealt,
            damageReceived = damageReceived,
            energyAbsorbed = energyAbsorbed,
            levelReached = levelReached,
            gameTime = gameTime,
            score = score,
            victory = victory
        };

        // Guardar partida actual
        databaseService?.SaveCurrentRun(currentRun);

        // Comparar con la mejor
        var bestRun = databaseService?.GetBestRun() ?? new RunRecord();
        if (currentRun.score > bestRun.score)
        {
            databaseService?.SaveBestRun(currentRun);
            bestRun = currentRun; // para mostrarla en la UI
        }

        // Mostrar UI final
        endGameUI?.Show(currentRun, bestRun);
    }
}