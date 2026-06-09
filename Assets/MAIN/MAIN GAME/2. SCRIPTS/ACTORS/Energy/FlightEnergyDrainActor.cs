using UnityEngine;

public class FlightEnergyDrainActor : EnergyActor
{
    [Header("Drain Configuration")]
    public float hoverTime = 15f;
    [Range(0, 3)] public float movementCostMultiplier = 1.5f;
    [Range(0, 3)] public float rotationCostMultiplier = 0.5f;

    [Header("References")]
    public MoveManager moveManager;
    public GazeEnergyDrainActor gazeDrainActor;
    public PlayerPermissions permissions;   // Añadido

    private float baseDrainRate;

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (moveManager == null || permissions == null) return false;
        if (!moveManager.isFlying || moveManager.isAttacking) return false;
        if (!permissions.flightEnergyDrainEnabled) return false;
        if (gazeDrainActor != null && gazeDrainActor.IsDraining) return false;

        return true;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        baseDrainRate = managerScript.max_energy / hoverTime;
    }

    public override void UpdateExecution()
    {
        if (moveManager == null || moveManager.playerRigidbody == null) return;

        Rigidbody rb = moveManager.playerRigidbody;
        float linearFraction = Mathf.Clamp01(rb.linearVelocity.magnitude / moveManager.max_linear_speed);
        float angularFraction = Mathf.Clamp01(Mathf.Abs(rb.angularVelocity.y) / moveManager.max_angular_speed);

        float totalDrain = baseDrainRate * (1f + movementCostMultiplier * linearFraction + rotationCostMultiplier * angularFraction);
        managerScript.modify_energy(-totalDrain * Time.deltaTime);

        if (managerScript.is_empty)
        {
            moveManager.ForceLand();
            StopExecution();
        }
    }
}