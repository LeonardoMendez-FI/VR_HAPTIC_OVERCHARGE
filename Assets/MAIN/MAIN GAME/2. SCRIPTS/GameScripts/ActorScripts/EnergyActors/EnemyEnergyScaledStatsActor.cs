using UnityEngine;
using UnityEngine.AI;

public class EnemyEnergyScaledStatsActor : MonoBehaviour
{
    [Header("Energy Manager")]
    public EnergyManager energyManager;

    [Header("Components to Scale")]
    public NavMeshAgent navAgent;                  // solo terrestres
    public FlyingPursuitActor flyingPursuit;      // solo voladores
    public MeleeAttackActor meleeAttack;
    public RangedAttackActor rangedAttack;

    [Header("Scale Settings")]
    public float minSpeedMultiplier = 0.3f;
    public float maxSpeedMultiplier = 1.5f;
    public float minDamageMultiplier = 0.2f;
    public float maxDamageMultiplier = 1.2f;
    public float minRotationMultiplier = 0.4f;
    public float maxRotationMultiplier = 1.3f;

    private float originalNavSpeed;
    private float originalFlySpeed;
    private float originalFlyRotation;
    private float originalMeleeDamage;
    private float originalMeleeRate;
    private float originalRangedDamage;
    private float originalRangedRate;

    void Start()
    {
        if (energyManager == null)
            energyManager = GetComponentInParent<EnergyManager>();

        if (energyManager != null)
        {
            energyManager.OnEnergyChanged.AddListener(OnEnergyChanged);
            // Guardar valores originales
            if (navAgent != null) originalNavSpeed = navAgent.speed;
            if (flyingPursuit != null)
            {
                originalFlySpeed = flyingPursuit.speed;
                originalFlyRotation = flyingPursuit.rotationSpeed;
            }
            if (meleeAttack != null)
            {
                originalMeleeDamage = meleeAttack.damage;
                originalMeleeRate = meleeAttack.attackRate;
            }
            if (rangedAttack != null)
            {
                originalRangedDamage = rangedAttack.damage;
                originalRangedRate = rangedAttack.fireRate;
            }
            // Aplicar estado inicial
            OnEnergyChanged(energyManager.normalized_local);
        }
    }

    void OnDestroy()
    {
        if (energyManager != null)
            energyManager.OnEnergyChanged.RemoveListener(OnEnergyChanged);
    }

    void OnEnergyChanged(float normalized)
    {
        float speedMult = Mathf.Lerp(minSpeedMultiplier, maxSpeedMultiplier, normalized);
        float damageMult = Mathf.Lerp(minDamageMultiplier, maxDamageMultiplier, normalized);
        float rotMult = Mathf.Lerp(minRotationMultiplier, maxRotationMultiplier, normalized);

        if (navAgent != null) navAgent.speed = originalNavSpeed * speedMult;
        if (flyingPursuit != null)
        {
            flyingPursuit.speed = originalFlySpeed * speedMult;
            flyingPursuit.rotationSpeed = originalFlyRotation * rotMult;
        }
        if (meleeAttack != null)
        {
            meleeAttack.damage = originalMeleeDamage * damageMult;
            // mantiene attackRate sin cambios? Se puede escalar también si se desea
        }
        if (rangedAttack != null)
        {
            rangedAttack.damage = originalRangedDamage * damageMult;
        }
    }
}