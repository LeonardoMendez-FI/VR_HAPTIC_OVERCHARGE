using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class DrainEvent : UnityEvent<IGazeTarget> { }

public class GazeEnergyDrainActor : GazeActor
{
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
        if (GazeManager == null || !GazeManager.IsLocked || GazeManager.CurrentTarget == null)
            return false;

        var gazeTarget = GazeManager.CurrentTarget as GazeTargetBehaviour;
        if (gazeTarget == null || gazeTarget.targetElectronicObject == null)
            return false;

        EnergyManager energy = gazeTarget.targetElectronicObject.energyManager;
        if (energy == null || energy.is_empty)
            return false;

        return true;
    }

    public override void StartExecution()
    {
        base.StartExecution();

        if (GazeManager != null)
            GazeManager.OnGazeTargetLost += OnTargetLost;

        var gazeTarget = GazeManager.CurrentTarget as GazeTargetBehaviour;
        if (gazeTarget != null && gazeTarget.targetElectronicObject != null)
        {
            targetEnergy = gazeTarget.targetElectronicObject.energyManager;
            targetVisual = gazeTarget.GetComponentInChildren<GazeVisualController>();
        }

        if (targetEnergy == null)
            StopExecution();
    }

    public override void UpdateExecution()
    {
        if (targetEnergy == null)
        {
            StopExecution();
            return;
        }

        // Tasa global de drenaje
        float absorbed = PlayerParameters.DrainRate * Time.deltaTime;
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
        if (GazeManager != null)
            GazeManager.OnGazeTargetLost -= OnTargetLost;

        base.StopExecution();
        targetEnergy = null;
        targetVisual = null;
    }

    private void OnTargetLost()
    {
        if (is_executing)
            StopExecution();
    }
}