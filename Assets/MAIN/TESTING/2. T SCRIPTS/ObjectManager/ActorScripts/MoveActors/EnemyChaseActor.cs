using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseActor : MoveActor
{
    [Header("Chase Settings")]
    public float detectionRange = 15f;
    public float loseRange = 20f;

    public NavMeshAgent agent;

    private bool playerDetected;

    protected override void Start()
    {
        base.Start();
        ConfigureAgent();
    }

    void ConfigureAgent()
    {
        if (agent == null) return;
        agent.speed = PlayerParameters.MEDIUM_LINEAR_SPEED * linearSpeedMultiplier;
        agent.angularSpeed = PlayerParameters.MEDIUM_ANGULAR_SPEED * angularSpeedMultiplier;
        agent.acceleration = agent.speed * 2f;
        agent.stoppingDistance = 0.5f;
    }

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (agent == null || !agent.isOnNavMesh) return false;
        if (playerTransform == null) return false;

        // Solo consulta, no muta playerDetected aquí
        return playerDetected;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        playerDetected = true;   // mutación segura aquí
    }

    public override void UpdateExecution()
    {
        if (agent != null && agent.isOnNavMesh && playerTransform != null)
        {
            agent.SetDestination(playerTransform.position);

            // Actualizar detección para saber cuándo dejar de perseguir
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance > loseRange)
                playerDetected = false;
        }
    }

    public override void StopExecution()
    {
        base.StopExecution();
        playerDetected = false;
        if (agent != null && agent.isOnNavMesh)
            agent.ResetPath();
    }
}