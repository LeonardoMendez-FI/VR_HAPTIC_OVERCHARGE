using UnityEngine;

public class HapticChargeActor : GazeActor
{
    [Header("References")]
    public HapticManager hapticManager;
    public GazeEnergyDrainActor drainActor;

    [Header("Settings")]
    [Range(0.1f, 2f)] public float stopRampDuration = 1.5f;

    private bool wasDraining;

    public override bool MeetsRequirements() => hapticManager != null && drainActor != null;

    public override void UpdateExecution()
    {
        if (drainActor.IsDraining)
        {
            float progress = 1f - drainActor.currentEnergyNorm;
            hapticManager.SetChargeProgress(progress);
            wasDraining = true;
        }
        else if (wasDraining)
        {
            hapticManager.StopChargeEffect();
            wasDraining = false;
        }
    }
}