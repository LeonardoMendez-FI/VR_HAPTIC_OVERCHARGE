using UnityEngine;

public class RangedAttackActor : EnemyActor
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Attack Settings")]
    public float damage = 8f;
    public float attackRange = 12f;
    public float fireRate = 0.5f;       // disparos por segundo

    private float fireCooldown = 0f;

    public override bool MeetsRequirements()
    {
        if (playerTarget == null) return false;
        if (projectilePrefab == null || firePoint == null) return false;

        float dist = Vector3.Distance(firePoint.position, playerTarget.position);
        return dist <= attackRange;
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

    void FireProjectile()
    {
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.damage = damage;
            p.target = playerTarget;
        }
    }

    public override void StopExecution()
    {
        fireCooldown = 0f;
    }
}