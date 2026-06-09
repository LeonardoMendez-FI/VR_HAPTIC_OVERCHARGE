using UnityEngine;

public class FlyingRangedEnemy : EnemyActor
{
    [Header("Flight Settings")]
    public float flyHeight        = 5f;
    public float speed            = PlayerParameters.MEDIUM_LINEAR_SPEED * 0.5f;
    public float rotationSpeed    = 3f;
    public float attackRange      = 15f;
    public float preferredDistance = 10f;

    [Header("Detection")]
    public float detectionRange = PlayerParameters.ENEMY_DETECTION_RANGE;
    public float loseRange      = PlayerParameters.ENEMY_LOSE_RANGE;

    [Header("Ranged Attack")]
    public float      damage           = 8f;
    public float      fireRate         = 1f;
    public GameObject projectilePrefab;
    public Transform  firePoint;

    [Header("Emergency Close Attack")]
    public float emergencyRange      = 3f;
    public float emergencyDamage     = 15f;
    public float emergencyAttackRate = 2f;

    private float fireCooldown      = 0f;
    private float emergencyCooldown = 0f;
    private bool  hasDetectedPlayer = false;

    // Must be a protected override so EnemyActor.Start() runs and populates
    // playerTarget from the injected EnemyReferences. The original void Start()
    // (non-override) shadowed the base method, leaving playerTarget null and
    // falling back on an expensive FindFirstObjectByType scene search.
    protected override void Start()
    {
        base.Start(); // sets playerTarget from EnemyReferences
    }

    public override bool MeetsRequirements()
    {
        if (playerTarget == null) return false;

        float dist = Vector3.Distance(transform.position, playerTarget.position);

        // Mirror the hysteresis pattern used by EnemyChaseActor.
        if (!hasDetectedPlayer)
        {
            if (dist <= detectionRange)
            {
                hasDetectedPlayer = true;
                return true;
            }
            return false;
        }

        if (dist > loseRange)
        {
            hasDetectedPlayer = false;
            return false;
        }

        return true;
    }

    public override void UpdateExecution()
    {
        if (playerTarget == null) return;

        Vector3 targetPos = playerTarget.position;
        Vector3 myPos     = transform.position;

        Vector3 toPlayer = targetPos - myPos;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        // Rotate to face the player on the horizontal plane.
        if (toPlayer != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Emergency melee takes priority over all ranged behaviour.
        if (dist <= emergencyRange)
        {
            EmergencyAttack();
            return;
        }

        // Orbit at preferred distance.
        Vector3 moveDir = Vector3.zero;
        if (dist > preferredDistance + 1f)
            moveDir = toPlayer.normalized;
        else if (dist < preferredDistance - 1f)
            moveDir = -toPlayer.normalized;

        // Altitude correction — moves toward flyHeight above Y=0.
        float heightError  = flyHeight - myPos.y;
        Vector3 verticalMove = Vector3.up * heightError * 0.5f;

        transform.position += (moveDir * speed + verticalMove) * Time.deltaTime;

        // Ranged attack.
        fireCooldown -= Time.deltaTime;
        if (dist <= attackRange && fireCooldown <= 0f)
        {
            FireProjectile();
            fireCooldown = 1f / fireRate;
        }
    }

    private void EmergencyAttack()
    {
        emergencyCooldown -= Time.deltaTime;
        if (emergencyCooldown <= 0f)
        {
            if (playerTarget != null)
            {
                StructManager playerStruct = playerTarget.GetComponentInParent<StructManager>();
                if (playerStruct != null)
                    playerStruct.TakeDamage(emergencyDamage, transform.position);
            }
            emergencyCooldown = 1f / emergencyAttackRate;
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile p    = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.damage   = damage;
            p.target   = playerTarget;
            p.speed    = PlayerParameters.PROJECTILE_SPEED;
            p.lifetime = PlayerParameters.PROJECTILE_LIFETIME;
        }
    }

    public override void StopExecution()
    {
        base.StopExecution();
        fireCooldown      = 0f;
        emergencyCooldown = 0f;
    }
}
