using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolActor : EnemyActor
{
    [Header("Patrol Settings")]
    public float patrolRadius            = 10f;
    public float waitTimeAtDestination   = 2f;
    public float playerDetectionRange    = PlayerParameters.ENEMY_DETECTION_RANGE;

    private Vector3 patrolCenter;
    private bool    waiting;
    private float   waitTimer;

    // ConfigureAgent runs in Awake so that EnemyEnergyScaledStatsActor.Start()
    // is guaranteed to read the correct initial values when it captures originals.
    private void Awake()
    {
        ConfigureAgent();
    }

    protected override void Start()
    {
        base.Start(); // populates playerTarget from EnemyReferences
        patrolCenter = transform.position;
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

        agent.acceleration     = agent.speed * 2f;
        agent.stoppingDistance = 0.5f;
    }

    public override bool MeetsRequirements()
    {
        if (agent == null || !agent.isOnNavMesh) return false;

        if (playerTarget != null)
        {
            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance <= playerDetectionRange)
                return false;
        }

        return true;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        waiting = false;
        GoToRandomDestination();
    }

    public override void UpdateExecution()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!waiting)
            {
                waiting   = true;
                waitTimer = waitTimeAtDestination;
            }

            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                GoToRandomDestination();
            }
        }
    }

    private void GoToRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + patrolCenter;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    public override void StopExecution()
    {
        base.StopExecution();
        if (agent != null && agent.isOnNavMesh)
            agent.ResetPath();
    }
}
