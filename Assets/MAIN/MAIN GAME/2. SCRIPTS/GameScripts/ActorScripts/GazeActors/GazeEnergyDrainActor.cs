using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class DrainEvent : UnityEvent<IGazeTarget> { }

public class GazeEnergyDrainActor : GazeActor
{
    [Header("Drain Settings")]
    public float drainRate = 5f;

    [Header("Events")]
    public FloatEvent onTargetEnergyChanged;
    public DrainEvent onTargetFullyDrained;
    public FloatEvent onEnergyAbsorbed;

    [HideInInspector] public float currentEnergyNorm;
    public bool IsDraining => is_executing;

    private EnergyManager targetEnergy;
    private GazeVisualController targetVisual;

    public override bool MeetsRequirements()
    {
        if (GazeManager == null || !GazeManager.IsLocked) return false;
        if (GazeManager.CurrentTarget == null) return false;
        return true;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        // Cachear referencias una sola vez
        var go = (GazeManager.CurrentTarget as MonoBehaviour)?.gameObject;
        if (go != null)
        {
            var eo = go.GetComponent<ElectronicObject>();
            targetEnergy = eo?.energyManager;
            targetVisual = go.GetComponentInChildren<GazeVisualController>();
        }
    }

    public override void UpdateExecution()
    {
        if (targetEnergy == null) { StopExecution(); return; }

        float absorbed = drainRate * Time.deltaTime;
        targetEnergy.modify_energy(-absorbed);
        currentEnergyNorm = targetEnergy.normalized_local;
        onTargetEnergyChanged?.Invoke(currentEnergyNorm);
        onEnergyAbsorbed?.Invoke(absorbed);

        if (targetVisual != null)
            targetVisual.SetDrainIntensity(1f - currentEnergyNorm);

        if (targetEnergy.is_empty)
        {
            onTargetFullyDrained?.Invoke(GazeManager.CurrentTarget);
            StopExecution();
        }
    }

    public override void StopExecution()
    {
        base.StopExecution();
        targetEnergy = null;
        targetVisual = null;
    }
}