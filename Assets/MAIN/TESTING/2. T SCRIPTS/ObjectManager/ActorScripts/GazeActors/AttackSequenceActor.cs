using UnityEngine;
using UnityEngine.UI;

public class AttackSequenceActor : GazeActor
{
    [Header("Attack Settings")]
    public float attackDuration = 2f;
    public RawImage redVignette;

    [Header("Haptic Feedback")]
    public HapticManager hapticManager;

    [Header("References")]
    public GazeEnergyDrainActor drainActor;
    public AttackManager attackManager;
    public MoveManager moveManager;           // ← añadido para SetAttacking

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

        // Bloquear movimiento del jugador durante el ataque
        moveManager?.SetAttacking(true);

        if (redVignette != null) redVignette.gameObject.SetActive(true);
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

        // Reportar daño solo al AttackManager (única fuente de verdad)
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
        hapticManager?.EndAttackEffect();

        // Incrementar eliminaciones en el AttackManager
        attackManager?.AddElimination();

        if (currentTarget?.TargetTransform != null)
            Destroy(currentTarget.TargetTransform.gameObject);

        // Desbloquear movimiento
        moveManager?.SetAttacking(false);

        isAttacking = false;
        StopExecution();
        if (redVignette != null) redVignette.gameObject.SetActive(false);
    }

    public override void StopExecution()
    {
        base.StopExecution();
        if (isAttacking)
        {
            isAttacking = false;
            if (redVignette != null) redVignette.gameObject.SetActive(false);
            hapticManager?.EndAttackEffect();
            moveManager?.SetAttacking(false);
        }
    }
}