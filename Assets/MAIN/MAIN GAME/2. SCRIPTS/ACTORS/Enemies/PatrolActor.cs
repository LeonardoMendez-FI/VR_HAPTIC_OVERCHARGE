using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Patrol actor. Roams to random NavMesh positions inside a configurable radius.
/// MeetsRequirements returns false the moment playerTarget enters detection range,
/// handing control back to EnemyManager so a pursuit actor can take over.
///
/// patrolCenter is captured in Start() so it persists after the enemy is spawned
/// at an arbitrary position, unlike Awake() which can run before the enemy is
/// placed in the world by SpawnService.
/// </summary>
public class PatrolActor : ActorScript<EnemyManager>
{
    [Header("Agent Reference")]
    public NavMeshAgent agent;

    [Header("Player Reference (injected by EnemyReferences)")]
    public Transform playerTarget;

    [Header("Detection Multiplier")]
    [Range(0.1f, 3f)] public float detectionTimeMultiplier = 1f;

    [Header("Patrol Settings")]
    public float patrolRadius          = 10f;
    public float waitTimeAtDestination = 2f;

    [Header("Speed Multipliers")]
    [Range(0.1f, 3f)] public float linearSpeedMultiplier  = 1f;
    [Range(0.1f, 3f)] public float angularSpeedMultiplier = 1f;

    private float   detectionRange;
    private Vector3 patrolCenter;
    private bool    waiting;
    private float   waitTimer;

    private void Awake()
    {
        detectionRange = PlayerParameters.MEDIUM_LINEAR_SPEED
                       * PlayerParameters.ENEMY_DETECTION_TIME
                       * detectionTimeMultiplier;

        if (agent == null) return;

        agent.speed          = PlayerParameters.MEDIUM_LINEAR_SPEED
                             * PlayerParameters.ENEMY_BASE_SPEED_MULTIPLIER
                             * linearSpeedMultiplier;
        agent.angularSpeed   = PlayerParameters.ENEMY_BASE_ANGULAR_SPEED_DEG
                             * angularSpeedMultiplier;
        agent.acceleration   = agent.speed * 2f;
        agent.stoppingDistance = 0.5f;
    }

    private void Start()
    {
        patrolCenter = transform.position;
    }

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (agent == null || !agent.isOnNavMesh) return false;

        // Yield to pursuit actors the moment the player enters detection range.
        if (playerTarget != null)
        {
            float dist = Vector3.Distance(transform.position, playerTarget.position);
            if (dist <= detectionRange) return false;
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
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius + patrolCenter;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    public override void StopExecution()
    {
        base.StopExecution();
        if (agent != null && agent.isOnNavMesh)
            agent.ResetPath();
    }
}
