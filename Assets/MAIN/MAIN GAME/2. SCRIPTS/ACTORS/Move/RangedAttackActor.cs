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

    [HideInInspector] public float damage;
    [HideInInspector] public float attackRange;

    private float fireCooldown = 0f;

    private void Awake()
    {
        damage      = PlayerParameters.ENEMY_BASE_RANGED_DAMAGE * damageMultiplier;
        attackRange = PlayerParameters.MEDIUM_LINEAR_SPEED
                    * PlayerParameters.ENEMY_RANGED_TIME
                    * timeMultiplier;
    }

    public override bool MeetsRequirements()
    {
        if (managerScript == null)
        {
            Debug.LogWarning("[RangedAttack] managerScript es null");
            return false;
        }
        if (playerTarget == null)
        {
            Debug.LogWarning("[RangedAttack] playerTarget es null");
            return false;
        }
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("[RangedAttack] projectilePrefab o firePoint es null");
            return false;
        }
        float dist = Vector3.Distance(firePoint.position, playerTarget.position);
        bool inRange = dist <= attackRange;
        if (!inRange)
        {
            Debug.Log($"[RangedAttack] Fuera de rango: dist={dist:F1} rango={attackRange:F1}");
        }
        return inRange;
    }

    public override void UpdateExecution()
    {
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            FireProjectile();
            fireCooldown = 1f / fireRate;
            Debug.Log("[RangedAttack] Proyectil disparado");
        }
    }

    private void FireProjectile()
    {
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
        fireCooldown = 0f;
    }
}