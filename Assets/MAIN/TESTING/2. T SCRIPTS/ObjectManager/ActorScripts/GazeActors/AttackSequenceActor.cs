using UnityEngine;
using UnityEngine.Events;

public class AttackSequenceActor : GazeActor
{
    [Header("Attack Settings")]
    public float attackDuration = 2f;

    [Header("Events (wire in Inspector)")]
    public UnityEvent OnAttackStarted;
    public UnityEvent OnAttackEnded;
    public UnityEvent OnEnemyDestroyed;

    [Header("References")]
    public GazeEnergyDrainActor drainActor;

    private bool isAttacking;
    private float attackTimer;
    private IGazeTarget currentTarget;
    private ElectronicObject targetElectronics;
    private GazeVisualController targetVisual;

    /// <summary>
    /// Propiedad pública que indica si el ataque está en curso (útil para otros actores como EnemyDepletedActor).
    /// </summary>
    public bool IsDraining => isAttacking;

    void Start()
    {
        if (drainActor != null)
            drainActor.onTargetFullyDrained.AddListener(StartAttack);
    }

    void OnDestroy()
    {
        if (drainActor != null)
            drainActor.onTargetFullyDrained.RemoveListener(StartAttack);
    }

    void StartAttack(IGazeTarget target)
    {
        if (isAttacking) return;
        isAttacking = true;
        attackTimer = 0f;
        currentTarget = target;

        var go = (target as MonoBehaviour)?.gameObject;
        targetElectronics = go?.GetComponent<ElectronicObject>();
        targetVisual = go?.GetComponentInChildren<GazeVisualController>();

        OnAttackStarted?.Invoke();
        StartExecution();
    }

    public override bool MeetsRequirements() => isAttacking;

    public override void UpdateExecution()
    {
        if (targetElectronics == null)
        {
            StopExecution();
            return;
        }

        attackTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(attackTimer / attackDuration);
        float damage = (targetElectronics.MaxEnergy() / attackDuration) * Time.deltaTime;
        targetElectronics.TakeDamage(damage);

        if (targetVisual != null)
            targetVisual.SetAttackProgress(progress);

        if (progress >= 1f)
        {
            DestroyEnemy();
        }
    }

    void DestroyEnemy()
    {
        OnEnemyDestroyed?.Invoke();
        GazeManager?.ForceReleaseTarget();
        StopExecution();
    }

    public override void StopExecution()
    {
        base.StopExecution();
        if (isAttacking)
        {
            isAttacking = false;
            OnAttackEnded?.Invoke();
        }
    }
}