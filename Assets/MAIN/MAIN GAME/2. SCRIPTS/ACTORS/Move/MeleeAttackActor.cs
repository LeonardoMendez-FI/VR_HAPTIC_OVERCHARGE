using UnityEngine;

public class MeleeAttackActor : ActorScript<EnemyManager>
{
    [Header("Player Reference (injected by EnemyReferences)")]
    public Transform playerTarget;

    [Header("Attack Point")]
    public Transform attackPoint;

    [Header("Multipliers")]
    [Range(0.1f, 5f)] public float damageMultiplier = 1f;
    [Range(0.1f, 5f)] public float timeMultiplier   = 1f;

    [Header("Attack Rate")]
    public float attackRate = 1f;

    [HideInInspector] public float damage;
    [HideInInspector] public float attackRange;

    private float attackCooldown = 0f;

    private void Awake()
    {
        damage      = PlayerParameters.ENEMY_BASE_MELEE_DAMAGE * damageMultiplier;
        attackRange = PlayerParameters.MEDIUM_LINEAR_SPEED
                    * PlayerParameters.ENEMY_MELEE_TIME
                    * timeMultiplier;
    }

    public override bool MeetsRequirements()
    {
        if (managerScript == null)
        {
            Debug.LogWarning("[MeleeAttack] managerScript es null");
            return false;
        }
        if (playerTarget == null)
        {
            Debug.LogWarning("[MeleeAttack] playerTarget es null");
            return false;
        }
        Transform origin = attackPoint != null ? attackPoint : transform;
        float dist = Vector3.Distance(origin.position, playerTarget.position);
        bool inRange = dist <= attackRange;
        if (!inRange)
        {
            Debug.Log($"[MeleeAttack] Fuera de rango: dist={dist:F1} rango={attackRange:F1}");
        }
        return inRange;
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

    private void AttackPlayer()
    {
        StructManager playerStruct = playerTarget.GetComponentInParent<StructManager>();
        playerStruct?.TakeDamage(damage, transform.position);
        Debug.Log($"[MeleeAttack] Daño aplicado: {damage}");
    }

    public override void StopExecution()
    {
        base.StopExecution();
        attackCooldown = 0f;
    }
}