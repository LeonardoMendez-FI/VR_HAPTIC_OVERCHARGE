using UnityEngine;

public class FlyingRangedAttackActor : ActorScript<EnemyManager>
{
    [Header("Player Reference (injected by EnemyReferences)")]
    public Transform playerTarget;

    [Header("Detection")]
    [Range(0.1f, 3f)] public float detectionTimeMultiplier = 1f;
    [Range(0.1f, 3f)] public float loseTimeMultiplier      = 1f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform  firePoint;

    [Header("Ranged Settings")]
    [Range(0.1f, 5f)] public float rangedTimeMultiplier  = 1f;
    [Range(0.1f, 5f)] public float damageMultiplier      = 1f;
    public float fireRate = 1f;

    [Header("Emergency Melee")]
    [Range(0.1f, 5f)] public float meleeTimeMultiplier       = 1f;
    [Range(0.1f, 5f)] public float emergencyDamageMultiplier = 1.5f;
    public float emergencyAttackRate = 2f;

    [HideInInspector] public float damage;
    [HideInInspector] public float emergencyDamage;
    [HideInInspector] public float attackRange;
    [HideInInspector] public float emergencyRange;

    private float detectionRange;
    private float loseRange;
    private bool  hasDetectedPlayer;
    private float fireCooldown      = 0f;
    private float emergencyCooldown = 0f;

    private void Awake()
    {
        detectionRange  = PlayerParameters.MEDIUM_LINEAR_SPEED
                        * PlayerParameters.ENEMY_DETECTION_TIME
                        * detectionTimeMultiplier;
        loseRange       = PlayerParameters.MEDIUM_LINEAR_SPEED
                        * PlayerParameters.ENEMY_LOSE_TIME
                        * loseTimeMultiplier;
        attackRange     = PlayerParameters.MEDIUM_LINEAR_SPEED
                        * PlayerParameters.ENEMY_RANGED_TIME
                        * rangedTimeMultiplier;
        emergencyRange  = PlayerParameters.MEDIUM_LINEAR_SPEED
                        * PlayerParameters.ENEMY_MELEE_TIME
                        * meleeTimeMultiplier;
        damage          = PlayerParameters.ENEMY_BASE_RANGED_DAMAGE * damageMultiplier;
        emergencyDamage = PlayerParameters.ENEMY_BASE_MELEE_DAMAGE  * emergencyDamageMultiplier;
    }

    public override bool MeetsRequirements()
    {
        if (managerScript == null)
        {
            Debug.LogWarning("[FlyRanged] managerScript es null");
            return false;
        }
        if (playerTarget == null)
        {
            Debug.LogWarning("[FlyRanged] playerTarget es null");
            return false;
        }
        float dist = Vector3.Distance(transform.position, playerTarget.position);
        if (!hasDetectedPlayer)
        {
            if (dist <= detectionRange) { hasDetectedPlayer = true; Debug.Log($"[FlyRanged] Detectado a {dist:F1}"); return true; }
            Debug.Log($"[FlyRanged] Fuera detección: dist={dist:F1} rango={detectionRange:F1}");
            return false;
        }
        if (dist > loseRange) { hasDetectedPlayer = false; Debug.Log($"[FlyRanged] Perdido a {dist:F1}"); return false; }
        return true;
    }

    public override void UpdateExecution()
    {
        if (playerTarget == null) return;

        float dist = Vector3.Distance(transform.position, playerTarget.position);

        if (dist <= emergencyRange)
        {
            EmergencyAttack();
            return;
        }

        fireCooldown -= Time.deltaTime;
        if (dist <= attackRange && fireCooldown <= 0f)
        {
            FireProjectile();
            fireCooldown = 1f / fireRate;
            Debug.Log("[FlyRanged] Disparo");
        }
    }

    private void EmergencyAttack()
    {
        emergencyCooldown -= Time.deltaTime;
        if (emergencyCooldown <= 0f)
        {
            StructManager playerStruct = playerTarget.GetComponentInParent<StructManager>();
            playerStruct?.TakeDamage(emergencyDamage, transform.position);
            emergencyCooldown = 1f / emergencyAttackRate;
            Debug.Log("[FlyRanged] Ataque emergencia");
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