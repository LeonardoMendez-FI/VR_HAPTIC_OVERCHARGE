using UnityEngine;

public class EnemyEnergyScaledStatsComponent : MonoBehaviour
{
    [Header("Energy Source")]
    public EnergyManager energyManager;

    [Header("Locomotion Actors (assign in Inspector)")]
    public EnemyChaseActor     chaseActor;
    public EnemyPatrolActor    patrolActor;
    public FlyingPursuitActor  flyingPursuitActor;

    [Header("Attack Actors (assign in Inspector)")]
    public MeleeAttackActor         meleeAttackActor;
    public RangedAttackActor        rangedAttackActor;
    public FlyingRangedAttackActor  flyingRangedAttackActor;   // ← cambiado

    [Header("Scaling Curve")]
    public AnimationCurve scalingCurve = AnimationCurve.Linear(0f, 0.2f, 1f, 1f);

    private float _baseChaseSpeed;
    private float _basePatrolSpeed;
    private float _baseFlySpeed;
    private float _baseMeleeDamage;
    private float _baseRangedDamage;
    private float _baseFlyRangedDamage;
    private float _baseFlyEmergencyDamage;
    private bool  _originalValuesCaptured = false;

    private void Start()
    {
        if (energyManager == null)
            energyManager = GetComponent<EnergyManager>()
                         ?? GetComponentInParent<EnergyManager>();

        CaptureOriginalValues();
        if (energyManager != null)
            energyManager.OnEnergyChanged.AddListener(ApplyScaling);
    }

    private void OnDestroy()
    {
        if (energyManager != null)
            energyManager.OnEnergyChanged.RemoveListener(ApplyScaling);
    }

    private void CaptureOriginalValues()
    {
        if (_originalValuesCaptured) return;
        _originalValuesCaptured = true;

        if (chaseActor  != null && chaseActor.agent  != null) _baseChaseSpeed  = chaseActor.agent.speed;
        if (patrolActor != null && patrolActor.agent != null) _basePatrolSpeed = patrolActor.agent.speed;
        if (flyingPursuitActor      != null) _baseFlySpeed           = flyingPursuitActor.speed;
        if (meleeAttackActor        != null) _baseMeleeDamage        = meleeAttackActor.damage;
        if (rangedAttackActor       != null) _baseRangedDamage       = rangedAttackActor.damage;
        if (flyingRangedAttackActor != null)
        {
            _baseFlyRangedDamage    = flyingRangedAttackActor.damage;
            _baseFlyEmergencyDamage = flyingRangedAttackActor.emergencyDamage;
        }
    }

    private void ApplyScaling(float normalizedEnergy)
    {
        if (!_originalValuesCaptured) return;
        float scale = scalingCurve.Evaluate(normalizedEnergy);

        if (chaseActor  != null && chaseActor.agent  != null)
            chaseActor.agent.speed = _baseChaseSpeed * scale;

        if (patrolActor != null && patrolActor.agent != null)
            patrolActor.agent.speed = _basePatrolSpeed * scale;

        if (flyingPursuitActor != null)
            flyingPursuitActor.speed = _baseFlySpeed * scale;

        if (meleeAttackActor != null)
            meleeAttackActor.damage = _baseMeleeDamage * scale;

        if (rangedAttackActor != null)
            rangedAttackActor.damage = _baseRangedDamage * scale;

        if (flyingRangedAttackActor != null)
        {
            flyingRangedAttackActor.damage         = _baseFlyRangedDamage   * scale;
            flyingRangedAttackActor.emergencyDamage = _baseFlyEmergencyDamage * scale;
        }
    }
}