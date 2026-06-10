using UnityEngine;

public class EnemyEnergyScaledStatsComponent : MonoBehaviour
{
    [Header("Energy Source")]
    public EnergyManager energyManager;

    [Header("Locomotion Actors (assign in Inspector)")]
    public EnemyChaseActor     chaseActor;
    public EnemyPatrolActor    patrolActor;
    public FlyingPursuitActor  flyingPursuitActor;

    [Header("Scaling Curve")]
    public AnimationCurve scalingCurve = AnimationCurve.Linear(0f, 0.2f, 1f, 1f);

    // ── Valores base ────────────────────────────────────────
    private float _baseChaseSpeed;
    private float _basePatrolSpeed;
    private float _baseFlySpeed;

    // ── Multiplicador de daño actual (1 = normal) ──────────
    public float CurrentDamageScale { get; private set; } = 1f;

    private bool _originalValuesCaptured = false;

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
        if (flyingPursuitActor      != null) _baseFlySpeed = flyingPursuitActor.speed;
    }

    private void ApplyScaling(float normalizedEnergy)
    {
        if (!_originalValuesCaptured) return;
        float scale = scalingCurve.Evaluate(normalizedEnergy);

        // Velocidades: se asignan directamente
        if (chaseActor  != null && chaseActor.agent  != null)
            chaseActor.agent.speed = _baseChaseSpeed * scale;

        if (patrolActor != null && patrolActor.agent != null)
            patrolActor.agent.speed = _basePatrolSpeed * scale;

        if (flyingPursuitActor != null)
            flyingPursuitActor.speed = _baseFlySpeed * scale;

        // Daño: solo actualizamos el multiplicador público
        CurrentDamageScale = scale;
    }
}