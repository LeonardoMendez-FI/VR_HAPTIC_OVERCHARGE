using UnityEngine;

public class EnergyAbsorbActor : EnergyActor
{
    [Header("Drain Reference")]
    public GazeEnergyDrainActor drainActor;

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (drainActor == null) return false;
        return drainActor.IsDraining;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        drainActor.onEnergyAbsorbed.AddListener(OnEnergyAbsorbed);
    }

    private void OnEnergyAbsorbed(float amount)
    {
        managerScript.modify_energy(amount);
    }

    public override void UpdateExecution()
    {
        // Energy transfer is driven entirely by the onEnergyAbsorbed event fired
        // each frame by GazeEnergyDrainActor — no per-frame work needed here.
    }

    public override void StopExecution()
    {
        base.StopExecution();

        // drainActor could be null if this actor is stopped during scene teardown
        // or if the Inspector reference was never assigned. Guard defensively so
        // StopExecution (called by ManagerScript on any actor exit) never throws.
        if (drainActor != null)
            drainActor.onEnergyAbsorbed.RemoveListener(OnEnergyAbsorbed);
    }
}
