using UnityEngine;

public class StatsCollector : MonoBehaviour
{
    [Header("References")]
    public DatabaseService databaseService;
    public AttackManager attackManager;
    public StructManager structManager;
    public GazeEnergyDrainActor drainActor;
    public EndGameUI endGameUI;

    [Header("Score Multipliers")]
    public float pLevel     = 100f;
    public float pRobots    = 10f;
    public float pMachines  = 50f;
    public float pEnergy    = 1f;
    public float pTime      = -1f;

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

    public void SetLevel(int level)
    {
        levelReached = level;
    }

    public void RegisterMachineDestroyed()
    {
        machinesDestroyed++;
    }

    public void EndGame(bool victory)
    {
        if (gameEnded) return;
        gameEnded = true;

        gameTime = Time.time - startTime;

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

        databaseService?.SaveCurrentRun(currentRun);

        var bestRun = databaseService?.GetBestRun() ?? new RunRecord();
        if (currentRun.score > bestRun.score)
        {
            databaseService?.SaveBestRun(currentRun);
            bestRun = currentRun;
        }

        endGameUI?.Show(currentRun, bestRun);
    }
}