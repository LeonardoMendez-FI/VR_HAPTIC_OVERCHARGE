using UnityEngine;

public class EnergyAbsorbActor : EnergyActor
{
    [Header("Drain Reference")]
    public GazeEnergyDrainActor drainActor;   // arrastra desde el Inspector

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (drainActor == null) return false;
        return drainActor.IsDraining;   // solo activo mientras drenamos a un enemigo
    }

    public override void StartExecution()
    {
        base.StartExecution();
        // Suscribirse al evento que dispara el drenaje cada frame
        drainActor.onEnergyAbsorbed.AddListener(OnEnergyAbsorbed);
    }

    private void OnEnergyAbsorbed(float amount)
    {
        // Sumar la energía absorbida a nuestra propia energía
        managerScript.modify_energy(amount);
    }

    public override void UpdateExecution()
    {
        // No necesita hacer nada cada frame, el evento se encarga de la recarga
    }

    public override void StopExecution()
    {
        base.StopExecution();
        drainActor.onEnergyAbsorbed.RemoveListener(OnEnergyAbsorbed);
    }
}