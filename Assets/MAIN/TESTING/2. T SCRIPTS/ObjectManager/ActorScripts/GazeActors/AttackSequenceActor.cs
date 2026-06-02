using UnityEngine;

public class AttackSequenceActor : GazeActor
{
    [Header("Attack Settings")]
    public float attackDuration = 2f;

    [Header("Haptic Feedback")]
    public HapticManager hapticManager;

    [Header("References")]
    public GazeEnergyDrainActor drainActor;
    public AttackManager attackManager;
    public MoveManager moveManager;

    private bool isAttacking;
    private float attackTimer;
    private IGazeTarget currentTarget;
    private ElectronicObject targetElectronics;
    private GazeVisualController targetVisual;

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

        // Bloquear movimiento del jugador
        moveManager?.SetAttacking(true);
        // Iniciar ataque (dispara el evento que activa la sobreimpresión roja)
        attackManager?.StartAttack();

        hapticManager?.StartAttackEffect(attackDuration);

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

        float maxEnergy = targetElectronics.MaxEnergy();
        float damage = (maxEnergy / attackDuration) * Time.deltaTime;
        targetElectronics.TakeDamage(damage);

        attackManager?.RegisterDamageDealt(damage);

        if (targetVisual != null)
            targetVisual.SetAttackProgress(progress);

        if (progress >= 1f)
        {
            DestroyEnemy();
        }
    }

    void DestroyEnemy()
    {
        // Liberar el objetivo del GazeManager antes de destruir el objeto
        GazeManager?.ForceReleaseTarget();

        hapticManager?.EndAttackEffect();
        attackManager?.AddElimination();
        attackManager?.EndAttack();

        if (currentTarget?.TargetTransform != null)
            Destroy(currentTarget.TargetTransform.gameObject);

        // Desbloquear movimiento
        moveManager?.SetAttacking(false);

        isAttacking = false;
        StopExecution();
    }

    public override void StopExecution()
    {
        base.StopExecution();
        if (isAttacking)
        {
            isAttacking = false;
            hapticManager?.EndAttackEffect();
            attackManager?.EndAttack();
            moveManager?.SetAttacking(false);
        }
    }
}