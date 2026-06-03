using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseActor : EnemyActor
{
    [Header("Chase Settings")]
    public float detectionRange = 15f;
    public float loseRange = 20f;

    private bool hasDetectedPlayer;

    void Start()
    {
        ConfigureAgent();
    }

    void ConfigureAgent()
    {
        if (agent == null) return;
        agent.speed = PlayerParameters.MEDIUM_LINEAR_SPEED;
        agent.angularSpeed = PlayerParameters.MEDIUM_ANGULAR_SPEED;
        agent.acceleration = agent.speed * 2f;
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
        {
            agent.SetDestination(playerTarget.position);
        }
    }

    public override void StopExecution()
    {
        base.StopExecution();
        if (agent != null && agent.isOnNavMesh)
            agent.ResetPath();
    }
}