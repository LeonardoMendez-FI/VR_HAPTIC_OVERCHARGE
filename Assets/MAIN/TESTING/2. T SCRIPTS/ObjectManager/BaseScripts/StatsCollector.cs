using UnityEngine;

public class StatsCollector : MonoBehaviour
{
    [Header("References")]
    public DatabaseManager databaseManager;
    public AttackManager attackManager;
    public StructManager structManager;
    public GazeEnergyDrainActor drainActor;
    public EndGameUI endGameUI;

    private int robotsDestroyed;
    private float damageDealt;
    private float damageReceived;
    private float energyAbsorbed;
    private int levelReached = 1;
    private bool gameEnded;

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
            structManager.OnEntityDestroyed.AddListener(OnPlayerDeath);  // ← nuevo
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

    public void EndGame(bool victory)
    {
        if (gameEnded) return;
        gameEnded = true;

        var run = new RunRecord
        {
            robotsDestroyed = robotsDestroyed,
            damageDealt = damageDealt,
            damageReceived = damageReceived,
            energyAbsorbed = energyAbsorbed,
            levelReached = levelReached,
            victory = victory
        };
        databaseManager?.SaveRun(run);

        endGameUI?.Show(run, databaseManager?.GetBestRun());
    }
}