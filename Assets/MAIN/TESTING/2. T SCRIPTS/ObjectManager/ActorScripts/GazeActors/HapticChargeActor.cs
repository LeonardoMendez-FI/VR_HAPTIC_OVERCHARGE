using UnityEngine;

public class HapticChargeActor : GazeActor
{
    [Header("References")]
    public HapticService hapticService;
    public GazeEnergyDrainActor drainActor;

    private bool wasDraining;

    public override bool MeetsRequirements() => hapticService != null && drainActor != null;

    public override void UpdateExecution()
    {
        if (drainActor.IsDraining)
        {
            float progress = 1f - drainActor.currentEnergyNorm;
            hapticService.SetChargeProgress(progress);
            wasDraining = true;
        }
        else if (wasDraining)
        {
            hapticService.StopChargeEffect();
            wasDraining = false;
        }
    }
}