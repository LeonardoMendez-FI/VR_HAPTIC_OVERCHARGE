using UnityEngine;
using UnityEngine.Events;

public class AttackManager : ManagerScript
{
    [HideInInspector] public bool is_attacking = false;
    [HideInInspector] public ElectronicObject target;

    public IntEvent OnEliminationCountChanged;
    public IntEvent OnObjectiveCountChanged;
    public FloatEvent OnDamageDealt;          // ← nuevo

    int eliminations = 0;
    int remainingObjectives = 10;

    public void AddElimination()
    {
        eliminations++;
        OnEliminationCountChanged?.Invoke(eliminations);
    }

    public void SetObjectives(int remaining)
    {
        remainingObjectives = remaining;
        OnObjectiveCountChanged?.Invoke(remainingObjectives);
    }

    public void RegisterDamageDealt(float damage)
    {
        OnDamageDealt?.Invoke(damage);
    }
}