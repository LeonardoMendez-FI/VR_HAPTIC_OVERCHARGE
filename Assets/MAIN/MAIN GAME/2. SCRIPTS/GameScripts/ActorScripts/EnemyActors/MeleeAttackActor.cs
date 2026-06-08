using UnityEngine;

public class MeleeAttackActor : EnemyActor
{
    [Header("Attack Settings")]
    public float damage = 10f;
    public float attackRange = 2.5f;
    public float attackRate = 1f;        // ataques por segundo
    public Transform attackPoint;        // punto de referencia para medir distancia

    private float attackCooldown = 0f;

    public override bool MeetsRequirements()
    {
        if (playerTarget == null) return false;
        if (attackPoint == null) return false;

        float dist = Vector3.Distance(attackPoint.position, playerTarget.position);
        return dist <= attackRange;
    }

    public override void UpdateExecution()
    {
        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            AttackPlayer();
            attackCooldown = 1f / attackRate;
        }
    }

    void AttackPlayer()
    {
        StructManager playerStruct = playerTarget.GetComponentInParent<StructManager>();
        if (playerStruct != null)
        {
            playerStruct.TakeDamage(damage, transform.position);
        }
    }

    public override void StopExecution()
    {
        attackCooldown = 0f;
    }
}