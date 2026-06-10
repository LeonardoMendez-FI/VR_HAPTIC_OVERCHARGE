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

    public float damage          => PlayerParameters.ENEMY_BASE_RANGED_DAMAGE * damageMultiplier;
    public float emergencyDamage => PlayerParameters.ENEMY_BASE_MELEE_DAMAGE  * emergencyDamageMultiplier;
    public float attackRange     => PlayerParameters.MEDIUM_LINEAR_SPEED * PlayerParameters.ENEMY_RANGED_TIME * rangedTimeMultiplier;
    public float emergencyRange  => PlayerParameters.MEDIUM_LINEAR_SPEED * PlayerParameters.ENEMY_MELEE_TIME  * meleeTimeMultiplier;

    private float detectionRange;
    private float loseRange;
    private bool  hasDetectedPlayer;
    private float fireCooldown      = 0f;
    private float emergencyCooldown = 0f;
    private EnemyEnergyScaledStatsComponent _scaler;

    private void Awake()
    {
        detectionRange = PlayerParameters.MEDIUM_LINEAR_SPEED * PlayerParameters.ENEMY_DETECTION_TIME * detectionTimeMultiplier;
        loseRange      = PlayerParameters.MEDIUM_LINEAR_SPEED * PlayerParameters.ENEMY_LOSE_TIME      * loseTimeMultiplier;
    }

    private void Start()
    {
        _scaler = GetComponentInParent<EnemyEnergyScaledStatsComponent>();
    }

    public override bool MeetsRequirements()
    {
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
        }
    }

    private void EmergencyAttack()
    {
        emergencyCooldown -= Time.deltaTime;
        if (emergencyCooldown <= 0f)
        {
            float scaledDamage = emergencyDamage;
            if (_scaler != null) scaledDamage *= _scaler.CurrentDamageScale;

            StructManager playerStruct = playerTarget.GetComponentInParent<StructManager>();
            playerStruct?.TakeDamage(scaledDamage, transform.position);
            emergencyCooldown = 1f / emergencyAttackRate;
        }
    }

    private void FireProjectile()
    {
        float scaledDamage = damage;
        if (_scaler != null) scaledDamage *= _scaler.CurrentDamageScale;

        if (projectilePrefab == null || firePoint == null) return;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile p    = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.damage   = scaledDamage;
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