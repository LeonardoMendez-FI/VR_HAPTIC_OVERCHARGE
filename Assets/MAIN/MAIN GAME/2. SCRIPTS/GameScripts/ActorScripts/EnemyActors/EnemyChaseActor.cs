using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseActor : EnemyActor
{
    [Header("Chase Settings")]
    public float detectionRange = PlayerParameters.ENEMY_DETECTION_RANGE;
    public float loseRange      = PlayerParameters.ENEMY_LOSE_RANGE;

    private bool hasDetectedPlayer;

    // ConfigureAgent runs in Awake so that EnemyEnergyScaledStatsActor.Start()
    // is guaranteed to read the correct initial values when it captures originals.
    private void Awake()
    {
        ConfigureAgent();
    }

    protected override void Start()
    {
        base.Start(); // populates playerTarget from EnemyReferences
    }

    private void ConfigureAgent()
    {
        if (agent == null) return;

        // MEDIUM_LINEAR_SPEED * ENEMY_SPEED_MULTIPLIER keeps enemy speed relative
        // to the player via a single global dial.
        agent.speed        = PlayerParameters.MEDIUM_LINEAR_SPEED * PlayerParameters.ENEMY_SPEED_MULTIPLIER;

        // NavMeshAgent.angularSpeed is in DEGREES/sec — do NOT use MEDIUM_ANGULAR_SPEED
        // (which is rad/s and would reduce rotation to ~0.75 deg/sec, nearly freezing the enemy).
        agent.angularSpeed = PlayerParameters.ENEMY_ANGULAR_SPEED_DEG;

        agent.acceleration    = agent.speed * 2f;
        agent.stoppingDistance = 0.5f;
    }

    public override bool MeetsRequirements()
    {
        if (agent == null || !agent.isOnNavMesh) return false;
        if (playerTarget == null) return false;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (!hasDetectedPlayer)
        {
            if (distance <= detectionRange)
            {
                hasDetectedPlayer = true;
                return true;
            }
            return false;
        }

        if (distance > loseRange)
        {
            hasDetectedPlayer = false;
            return false;
        }

        return true;
    }

    public override void UpdateExecution()
    {
        if (agent != null && agent.isOnNavMesh && playerTarget != null)
            agent.SetDestination(playerTarget.position);
    }

    public override void StopExecution()
    {
        base.StopExecution();
        if (agent != null && agent.isOnNavMesh)
            agent.ResetPath();
    }
}
