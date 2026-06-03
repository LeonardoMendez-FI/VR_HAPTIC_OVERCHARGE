using UnityEngine;

public class FlyingRangedEnemy : EnemyActor
{
    [Header("Flight Settings")]
    public float flyHeight = 5f;           // altura a la que intenta mantenerse
    public float speed = 5f;
    public float rotationSpeed = 3f;
    public float attackRange = 15f;        // distancia a la que empieza a disparar
    public float preferredDistance = 10f;  // distancia que trata de mantener

    [Header("Attack")]
    public float damage = 10f;
    public float fireRate = 1f;
    public GameObject projectilePrefab;
    public Transform firePoint;            // desde donde salen los proyectiles

    private float fireCooldown = 0f;

    public override bool MeetsRequirements()
    {
        return playerTarget != null;       // siempre activo mientras haya jugador
    }

    public override void UpdateExecution()
    {
        if (playerTarget == null) return;

        Vector3 targetPos = playerTarget.position;
        Vector3 myPos = transform.position;

        // Calcular dirección horizontal hacia el jugador
        Vector3 toPlayer = targetPos - myPos;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        // Rotar hacia el jugador
        if (toPlayer != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Movimiento hacia la distancia preferida
        Vector3 moveDir = Vector3.zero;
        if (dist > preferredDistance + 1f)
        {
            moveDir = toPlayer.normalized;
        }
        else if (dist < preferredDistance - 1f)
        {
            moveDir = -toPlayer.normalized;
        }
        // Movimiento lateral opcional: podría esquivar, pero por simplicidad se queda quieto

        // Movimiento vertical para mantener la altura
        float heightError = flyHeight - myPos.y;
        Vector3 verticalMove = Vector3.up * heightError * 0.5f;

        Vector3 velocity = (moveDir * speed) + verticalMove;
        transform.position += velocity * Time.deltaTime;

        // Ataque a distancia
        fireCooldown -= Time.deltaTime;
        if (dist <= attackRange && fireCooldown <= 0f)
        {
            FireProjectile();
            fireCooldown = 1f / fireRate;
        }
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.damage = damage;
            p.target = playerTarget;   // el proyectil perseguirá al jugador (opcional)
        }
    }

    public override void StopExecution()
    {
        base.StopExecution();
    }
}