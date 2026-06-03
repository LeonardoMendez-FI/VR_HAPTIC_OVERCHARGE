using UnityEngine;

public class AttackEventBridge : MonoBehaviour
{
    [Header("References")]
    public AttackSequenceActor attackSequenceActor;
    public MoveManager moveManager;
    public AttackManager attackManager;
    public HapticService hapticService;   // renombrado

    private void OnEnable()
    {
        if (attackSequenceActor == null) return;

        attackSequenceActor.OnAttackStarted.AddListener(HandleAttackStarted);
        attackSequenceActor.OnAttackEnded.AddListener(HandleAttackEnded);
        attackSequenceActor.OnEnemyDestroyed.AddListener(HandleEnemyDestroyed);
    }

    private void OnDisable()
    {
        if (attackSequenceActor == null) return;

        attackSequenceActor.OnAttackStarted.RemoveListener(HandleAttackStarted);
        attackSequenceActor.OnAttackEnded.RemoveListener(HandleAttackEnded);
        attackSequenceActor.OnEnemyDestroyed.RemoveListener(HandleEnemyDestroyed);
    }

    private void HandleAttackStarted()
    {
        moveManager?.SetAttacking(true);
        attackManager?.StartAttack();
        hapticService?.StartAttackEffect(attackSequenceActor.attackDuration);
    }

    private void HandleAttackEnded()
    {
        moveManager?.SetAttacking(false);
        attackManager?.EndAttack();
        hapticService?.EndAttackEffect();
    }

    private void HandleEnemyDestroyed()
    {
        attackManager?.AddElimination();
    }
}