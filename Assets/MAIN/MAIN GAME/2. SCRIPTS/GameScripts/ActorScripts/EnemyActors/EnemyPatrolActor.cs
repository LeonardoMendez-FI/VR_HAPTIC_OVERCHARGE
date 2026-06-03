using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolActor : EnemyActor
{
    [Header("Patrol Settings")]
    public float patrolRadius = 10f;
    public float waitTimeAtDestination = 2f;
    public float playerDetectionRange = 15f;

    private Vector3 patrolCenter;
    private bool waiting;
    private float waitTimer;

    void Start()
    {
        patrolCenter = transform.position;
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
                waiting = true;
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

    void GoToRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += patrolCenter;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    public override void StopExecution()
    {
        base.StopExecution();
        if (agent != null && agent.isOnNavMesh)
            agent.ResetPath();
    }
}