using UnityEngine;
using UnityEngine.Events;

public class AttackManager : ManagerScript
{
    [HideInInspector] public bool is_attacking = false;
    [HideInInspector] public ElectronicObject target;

    public IntEvent OnEliminationCountChanged;
    public IntEvent OnObjectiveCountChanged;
    public FloatEvent OnDamageDealt;

    public UnityEvent OnAttackStarted;
    public UnityEvent OnAttackEnded;

    public GameSessionData gameSessionData;

    private int eliminations = 0;
    private int remainingObjectives = 10;

    public void AddElimination()
    {
        eliminations++;
        OnEliminationCountChanged?.Invoke(eliminations);
        gameSessionData?.AddRobotDestroyed();
    }

    public void SetObjectives(int remaining)
    {
        remainingObjectives = remaining;
        OnObjectiveCountChanged?.Invoke(remainingObjectives);
    }

    public void RegisterDamageDealt(float damage) => OnDamageDealt?.Invoke(damage);

    public void StartAttack()
    {
        is_attacking = true;
        OnAttackStarted?.Invoke();
    }

    public void EndAttack()
    {
        is_attacking = false;
        OnAttackEnded?.Invoke();
    }
}