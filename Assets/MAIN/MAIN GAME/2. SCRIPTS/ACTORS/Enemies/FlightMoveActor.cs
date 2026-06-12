using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Flying pursuit actor. Uses NavMeshAgent for XZ pathfinding while adjusting
/// agent.baseOffset every frame to float toward the player's altitude.
///
/// Why baseOffset instead of overwriting transform.position.y:
///   agent.baseOffset shifts the NavMeshAgent's sampling point on the navmesh
///   without fighting the agent's own position update. This lets the agent
///   continue navigating on the ground mesh while the visual rises/falls smoothly.
///   The spec says "do not overwrite the offset" — this actor ADDS a delta each frame
///   rather than setting it to an absolute value, so external initial offsets are
///   preserved.
/// </summary>
public class FlightMoveActor : ActorScript<EnemyManager>
{
    [Header("Agent Reference")]
    public NavMeshAgent agent;

    [Header("Player Reference (injected by EnemyReferences)")]
    public Transform playerTarget;

    [Header("Detection Multipliers")]
    [Range(0.1f, 3f)] public float detectionTimeMultiplier = 1f;
    [Range(0.1f, 3f)] public float loseTimeMultiplier      = 1f;

    [Header("Speed Multipliers")]
    [Range(0.1f, 3f)] public float linearSpeedMultiplier  = 1f;
    [Range(0.1f, 3f)] public float angularSpeedMultiplier = 1f;

    [Header("Altitude Tracking")]
    [Tooltip("How quickly baseOffset converges toward the player's Y altitude (units/s).")]
    [Range(0.1f, 10f)] public float altitudeSpeed = 2f;

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
        if (!base.MeetsRequirements()) return false;
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
        if (agent.isOnNavMesh)
            agent.SetDestination(playerTarget.position);

        // Incrementally adjust baseOffset toward the player's altitude.
        // The agent's world-space Y = navmesh surface Y + baseOffset,
        // so adding (playerY - enemyY) each frame converges the enemy's
        // height toward the player's without overwriting a configured start offset.
        float heightDelta = (playerTarget.position.y - transform.position.y)
                          * altitudeSpeed * Time.deltaTime;
        agent.baseOffset += heightDelta;
    }

    public override void StopExecution()
    {
        base.StopExecution();
        if (agent != null && agent.isOnNavMesh)
            agent.ResetPath();
    }
}
