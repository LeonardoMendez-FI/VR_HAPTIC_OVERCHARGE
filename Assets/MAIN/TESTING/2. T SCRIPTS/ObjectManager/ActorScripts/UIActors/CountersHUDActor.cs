using UnityEngine;

public class CountersHUDActor : UIActor<AttackManager>
{
    public CountersUI countersUI;

    protected override void Subscribe()
    {
        manager.OnEliminationCountChanged.AddListener(OnElimination);
        manager.OnObjectiveCountChanged.AddListener(OnObjective);
    }

    protected override void Unsubscribe()
    {
        manager.OnEliminationCountChanged.RemoveListener(OnElimination);
        manager.OnObjectiveCountChanged.RemoveListener(OnObjective);
    }

    void OnElimination(int count) => countersUI?.SetEliminations(count);
    void OnObjective(int remaining) => countersUI?.SetObjectives(remaining);
}