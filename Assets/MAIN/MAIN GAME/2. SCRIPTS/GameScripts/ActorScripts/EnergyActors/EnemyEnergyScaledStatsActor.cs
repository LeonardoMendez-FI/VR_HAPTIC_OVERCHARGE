using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Scales enemy combat stats (speed, rotation, damage, acceleration) according
/// to the enemy's current energy level. Fully charged = max stats; empty = min stats.
///
/// EXECUTION ORDER:
/// ConfigureAgent() in EnemyChaseActor/EnemyPatrolActor runs in Awake().
/// This script captures the configured values in Start(), which Unity guarantees
/// runs after ALL Awake() calls on all objects — making this fully order-safe
/// without relying on Script Execution Order settings or Inspector component ordering.
/// </summary>
public class EnemyEnergyScaledStatsActor : MonoBehaviour
{
    [Header("Energy Manager")]
    public EnergyManager energyManager;

    [Header("Components to Scale")]
    public NavMeshAgent       navAgent;
    public FlyingPursuitActor flyingPursuit;
    public FlyingRangedEnemy  flyingRanged;
    public MeleeAttackActor   meleeAttack;
    public RangedAttackActor  rangedAttack;

    [Header("Scale Settings")]
    public float minSpeedMultiplier        = 0.3f;
    public float maxSpeedMultiplier        = 1.5f;
    public float minDamageMultiplier       = 0.2f;
    public float maxDamageMultiplier       = 1.2f;
    public float minRotationMultiplier     = 0.4f;
    public float maxRotationMultiplier     = 1.3f;
    public float minAccelerationMultiplier = 0.2f;
    public float maxAccelerationMultiplier = 1.5f;

    // Baseline values captured in Start() after all Awake() calls have completed,
    // guaranteeing that ConfigureAgent() in the locomotion actors has already
    // written the correct PlayerParameters-derived speeds to the NavMeshAgent.
    private float originalNavSpeed;
    private float originalNavAngularSpeed;
    private float originalNavAcceleration;
    private float originalFlySpeed;
    private float originalFlyRotation;
    private float originalFlyRangedSpeed;
    private float originalFlyRangedRotation;
    private float originalMeleeDamage;
    private float originalRangedDamage;

    private void Start()
    {
        // Auto-resolve EnergyManager if not wired in the Inspector.
        if (energyManager == null)
            energyManager = GetComponentInParent<EnergyManager>();

        // Capture original values NOW — after all Awake() calls have run.
        // EnemyChaseActor.Awake() and EnemyPatrolActor.Awake() call ConfigureAgent(),
        // which sets navAgent.speed from PlayerParameters. Reading those values here
        // in Start() is guaranteed to see the correct, configured numbers.
        if (navAgent != null)
        {
            originalNavSpeed        = navAgent.speed;
            originalNavAngularSpeed = navAgent.angularSpeed;
            originalNavAcceleration = navAgent.acceleration;
        }
        if (flyingPursuit != null)
        {
            originalFlySpeed    = flyingPursuit.speed;
            originalFlyRotation = flyingPursuit.rotationSpeed;
        }
        if (flyingRanged != null)
        {
            originalFlyRangedSpeed    = flyingRanged.speed;
            originalFlyRangedRotation = flyingRanged.rotationSpeed;
        }
        if (meleeAttack != null)
            originalMeleeDamage = meleeAttack.damage;
        if (rangedAttack != null)
            originalRangedDamage = rangedAttack.damage;

        if (energyManager != null)
        {
            energyManager.OnEnergyChanged.AddListener(OnEnergyChanged);

            // Apply initial scaling — enemy spawns fully charged so normalized = 1,
            // but an explicit call ensures the stats are correct from the first frame.
            OnEnergyChanged(energyManager.normalized_local);
        }
    }

    private void OnDestroy()
    {
        if (energyManager != null)
            energyManager.OnEnergyChanged.RemoveListener(OnEnergyChanged);
    }

    private void OnEnergyChanged(float normalized)
    {
        float speedMult  = Mathf.Lerp(minSpeedMultiplier,        maxSpeedMultiplier,        normalized);
        float damageMult = Mathf.Lerp(minDamageMultiplier,       maxDamageMultiplier,       normalized);
        float rotMult    = Mathf.Lerp(minRotationMultiplier,     maxRotationMultiplier,     normalized);
        float accelMult  = Mathf.Lerp(minAccelerationMultiplier, maxAccelerationMultiplier, normalized);

        if (navAgent != null)
        {
            navAgent.speed        = originalNavSpeed        * speedMult;
            navAgent.angularSpeed = originalNavAngularSpeed * rotMult;
            navAgent.acceleration = originalNavAcceleration * accelMult;
        }
        if (flyingPursuit != null)
        {
            flyingPursuit.speed         = originalFlySpeed    * speedMult;
            flyingPursuit.rotationSpeed = originalFlyRotation * rotMult;
        }
        if (flyingRanged != null)
        {
            flyingRanged.speed         = originalFlyRangedSpeed    * speedMult;
            flyingRanged.rotationSpeed = originalFlyRangedRotation * rotMult;
        }
        if (meleeAttack != null)
            meleeAttack.damage = originalMeleeDamage * damageMult;
        if (rangedAttack != null)
            rangedAttack.damage = originalRangedDamage * damageMult;
    }
}
