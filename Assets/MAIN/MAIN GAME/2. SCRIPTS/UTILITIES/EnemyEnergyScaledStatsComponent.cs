using UnityEngine;

/// <summary>
/// Centralised energy-to-stats scaler. Listens to EnergyManager.OnEnergyChanged
/// and applies a curve-sampled scale factor to every locomotion actor's agent speed.
/// Attack actors read CurrentDamageScale directly when computing damage.
/// </summary>
public class EnemyEnergyScaledStatsComponent : MonoBehaviour
{
    [Header("Energy Source")]
    public EnergyManager energyManager;

    [Header("Locomotion Actors — assign whichever this enemy uses")]
    public GroundMoveActor groundMoveActor;
    public FlightMoveActor flightMoveActor;
    public PatrolActor     patrolActor;

    [Header("Scaling Curve")]
    [Tooltip("X = normalised energy (0–1). Y = speed/damage multiplier.")]
    public AnimationCurve scalingCurve = AnimationCurve.Linear(0f, 0.2f, 1f, 1f);

    /// <summary>Current damage multiplier. Read by attack actors.</summary>
    public float CurrentDamageScale { get; private set; } = 1f;

    // ── Cached base speeds ───────────────────────────────────────────────────
    private float _baseGroundMoveSpeed;
    private float _baseFlightMoveSpeed;
    private float _basePatrolSpeed;
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

        if (groundMoveActor != null && groundMoveActor.agent != null)
            _baseGroundMoveSpeed = groundMoveActor.agent.speed;
        if (flightMoveActor != null && flightMoveActor.agent != null)
            _baseFlightMoveSpeed = flightMoveActor.agent.speed;
        if (patrolActor     != null && patrolActor.agent     != null)
            _basePatrolSpeed = patrolActor.agent.speed;
    }

    private void ApplyScaling(float normalizedEnergy)
    {
        if (!_originalValuesCaptured) return;

        float scale = scalingCurve.Evaluate(normalizedEnergy);
        CurrentDamageScale = scale;

        if (groundMoveActor != null && groundMoveActor.agent != null)
            groundMoveActor.agent.speed = _baseGroundMoveSpeed * scale;
        if (flightMoveActor != null && flightMoveActor.agent != null)
            flightMoveActor.agent.speed = _baseFlightMoveSpeed * scale;
        if (patrolActor     != null && patrolActor.agent     != null)
            patrolActor.agent.speed     = _basePatrolSpeed     * scale;
    }
}