using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Ground pursuit actor. Chases playerTarget using NavMeshAgent.
/// No detection logic, no patrol logic, no attack logic.
/// Detection/patrol gating is the responsibility of other actors in the priority list.
/// MeetsRequirements returns true whenever playerTarget is assigned and the
/// agent is on the NavMesh, so this actor should sit AFTER any patrol/detection
/// actors in EnemyManager's actor list.
/// </summary>
public class GroundMoveActor : ActorScript<EnemyManager>
{
    [Header("Agent Reference")]
    public NavMeshAgent agent;

    [Header("Player Reference (injected by EnemyReferences)")]
    public Transform playerTarget;

    [Header("Speed Multipliers")]
    [Range(0.1f, 3f)] public float linearSpeedMultiplier  = 1f;
    [Range(0.1f, 3f)] public float angularSpeedMultiplier = 1f;

    private void Awake()
    {
        if (agent == null) return;

        agent.speed        = PlayerParameters.MEDIUM_LINEAR_SPEED
                           * PlayerParameters.ENEMY_BASE_SPEED_MULTIPLIER
                           * linearSpeedMultiplier;
        agent.angularSpeed = PlayerParameters.ENEMY_BASE_ANGULAR_SPEED_DEG
                           * angularSpeedMultiplier;
        agent.acceleration = agent.speed * 2f;
        agent.stoppingDistance = 0.5f;
    }

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (agent == null || !agent.isOnNavMesh) return false;
        if (playerTarget == null) return false;
        return true;
    }

    public override void UpdateExecution()
    {
        if (agent.isOnNavMesh)
            agent.SetDestination(playerTarget.position);
    }

    public override void StopExecution()
    {
        base.StopExecution();
        if (agent != null && agent.isOnNavMesh)
            agent.ResetPath();
    }
}
