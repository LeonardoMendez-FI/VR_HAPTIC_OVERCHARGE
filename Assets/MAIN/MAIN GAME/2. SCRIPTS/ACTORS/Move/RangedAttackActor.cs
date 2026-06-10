using UnityEngine;

public class RangedAttackActor : ActorScript<EnemyManager>
{
    [Header("Player Reference (injected by EnemyReferences)")]
    public Transform playerTarget;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform  firePoint;

    [Header("Multipliers")]
    [Range(0.1f, 5f)] public float damageMultiplier = 1f;
    [Range(0.1f, 5f)] public float timeMultiplier   = 1f;

    [Header("Fire Rate")]
    public float fireRate = 0.5f;

    public float damage      => PlayerParameters.ENEMY_BASE_RANGED_DAMAGE * damageMultiplier;
    public float attackRange => PlayerParameters.MEDIUM_LINEAR_SPEED
                              * PlayerParameters.ENEMY_RANGED_TIME
                              * timeMultiplier;

    private float fireCooldown = 0f;
    private EnemyEnergyScaledStatsComponent _scaler;

    private void Start()
    {
        _scaler = GetComponentInParent<EnemyEnergyScaledStatsComponent>();
    }

    public override bool MeetsRequirements()
    {
        if (playerTarget == null) return false;
        if (projectilePrefab == null || firePoint == null) return false;
        return Vector3.Distance(firePoint.position, playerTarget.position) <= attackRange;
    }

    public override void UpdateExecution()
    {
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            FireProjectile();
            fireCooldown = 1f / fireRate;
        }
    }

    private void FireProjectile()
    {
        float scaledDamage = damage;
        if (_scaler != null) scaledDamage *= _scaler.CurrentDamageScale;

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
        fireCooldown = 0f;
    }
}