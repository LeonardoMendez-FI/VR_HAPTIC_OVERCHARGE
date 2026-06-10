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

    public float damage      => PlayerParameters.ENEMY_BASE_MELEE_DAMAGE * damageMultiplier;
    public float attackRange => PlayerParameters.MEDIUM_LINEAR_SPEED
                              * PlayerParameters.ENEMY_MELEE_TIME
                              * timeMultiplier;

    private float attackCooldown = 0f;
    private EnemyEnergyScaledStatsComponent _scaler;

    private void Start()
    {
        _scaler = GetComponentInParent<EnemyEnergyScaledStatsComponent>();
    }

    public override bool MeetsRequirements()
    {
        if (playerTarget == null) return false;
        Transform origin = attackPoint != null ? attackPoint : transform;
        return Vector3.Distance(origin.position, playerTarget.position) <= attackRange;
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
        float scaledDamage = damage;
        if (_scaler != null) scaledDamage *= _scaler.CurrentDamageScale;

        StructManager playerStruct = playerTarget.GetComponentInParent<StructManager>();
        playerStruct?.TakeDamage(scaledDamage, transform.position);
    }

    public override void StopExecution()
    {
        base.StopExecution();
        attackCooldown = 0f;
    }
}