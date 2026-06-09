using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseActor : ActorScript<EnemyManager>
{
    [Header("Agent Reference")]
    public NavMeshAgent agent;

    [Header("Player Reference (injected by EnemyReferences)")]
    public Transform playerTarget;

    [Header("Multipliers")]
    [Range(0.1f, 3f)] public float detectionTimeMultiplier = 1f;
    [Range(0.1f, 3f)] public float loseTimeMultiplier      = 1f;
    [Range(0.1f, 3f)] public float linearSpeedMultiplier   = 1f;
    [Range(0.1f, 3f)] public float angularSpeedMultiplier  = 1f;

    private float detectionRange;
    private float loseRange;
    private bool  hasDetectedPlayer;

    private void Awake()
    {
        detectionRange = PlayerParameters.MEDIUM_LINEAR_SPEED
                       * PlayerParameters.ENEMY_DETECTION_TIME
                       * detectionTimeMultiplier;
        loseRange      = PlayerParameters.MEDIUM_LINEAR_SPEED
                       * PlayerParameters.ENEMY_LOSE_TIME
                       * loseTimeMultiplier;
        ConfigureAgent();
    }

    private void ConfigureAgent()
    {
        if (agent == null) return;
        agent.speed          = PlayerParameters.MEDIUM_LINEAR_SPEED
                             * PlayerParameters.ENEMY_BASE_SPEED_MULTIPLIER
                             * linearSpeedMultiplier;
        agent.angularSpeed   = PlayerParameters.ENEMY_BASE_ANGULAR_SPEED_DEG
                             * angularSpeedMultiplier;
        agent.acceleration   = agent.speed * 2f;
        agent.stoppingDistance = 0.5f;
    }

    public override bool MeetsRequirements()
    {
        if (agent == null || !agent.isOnNavMesh) return false;
        if (playerTarget == null) return false;

        float dist = Vector3.Distance(transform.position, playerTarget.position);

        if (!hasDetectedPlayer)
        {
            if (dist <= detectionRange) { hasDetectedPlayer = true; return true; }
            return false;
        }

        if (dist > loseRange) { hasDetectedPlayer = false; return false; }
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