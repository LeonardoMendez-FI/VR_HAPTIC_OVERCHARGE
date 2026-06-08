using UnityEngine;

public class FlyingPursuitActor : EnemyActor
{
    [Header("Flight Settings")]
    public float flyHeight = 5f;
    public float speed = 5f;
    public float rotationSpeed = 3f;
    public float preferredDistance = 8f;   // distancia que intenta mantener

    public override bool MeetsRequirements()
    {
        return playerTarget != null;
    }

    public override void UpdateExecution()
    {
        if (playerTarget == null) return;

        Vector3 targetPos = playerTarget.position;
        Vector3 myPos = transform.position;

        // Dirección horizontal hacia el jugador
        Vector3 toPlayer = targetPos - myPos;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        // Rotar hacia el jugador
        if (toPlayer != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Moverse hacia la distancia preferida
        Vector3 moveDir = Vector3.zero;
        if (dist > preferredDistance + 1f)
            moveDir = toPlayer.normalized;
        else if (dist < preferredDistance - 1f)
            moveDir = -toPlayer.normalized;

        // Movimiento vertical para mantener altura
        float heightError = flyHeight - myPos.y;
        Vector3 verticalMove = Vector3.up * heightError * 0.5f;

        Vector3 velocity = (moveDir * speed) + verticalMove;
        transform.position += velocity * Time.deltaTime;
    }

    public override void StopExecution() { }
}