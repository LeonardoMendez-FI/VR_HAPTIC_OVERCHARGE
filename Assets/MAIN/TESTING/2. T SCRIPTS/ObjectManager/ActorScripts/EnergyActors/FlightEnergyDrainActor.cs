using UnityEngine;

public class FlightEnergyDrainActor : EnergyActor
{
    [Header("Drain Configuration")]
    public float hoverTime = 15f;
    [Range(0, 3)] public float movementCostMultiplier = 1.5f;
    [Range(0, 3)] public float rotationCostMultiplier = 0.5f;

    [Header("Debug")]
    public bool showDebug = false;

    private MoveManager moveManager;
    private float baseDrainRate;

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (moveManager == null)
            moveManager = (managerScript.electronicObject as Robot)?.moveManager;
        return moveManager != null && moveManager.isFlying && !moveManager.isAttacking;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        baseDrainRate = managerScript.max_energy / hoverTime;
        if (showDebug)
            Debug.Log($"FlightEnergyDrainActor StartExecution: maxEnergy={managerScript.max_energy} baseDrainRate={baseDrainRate:F2}");
    }

    public override void UpdateExecution()
    {
        if (moveManager == null || moveManager.playerRigidbody == null) return;

        Rigidbody rb = moveManager.playerRigidbody;
        float linearFraction = Mathf.Clamp01(rb.linearVelocity.magnitude / moveManager.max_linear_speed);
        float angularFraction = Mathf.Clamp01(Mathf.Abs(rb.angularVelocity.y) / moveManager.max_angular_speed);

        float totalDrain = baseDrainRate * (1f + movementCostMultiplier * linearFraction + rotationCostMultiplier * angularFraction);
        float drainThisFrame = totalDrain * Time.deltaTime;
        managerScript.modify_energy(-drainThisFrame);

        if (showDebug)
            Debug.Log($"FlightDrain: linFrac={linearFraction:F2} angFrac={angularFraction:F2} drain={drainThisFrame:F2}");

        // Auto-aterrizaje si se agotó la energía
        if (managerScript.is_empty)
        {
            moveManager.ForceLand();
            StopExecution();
            if (showDebug) Debug.Log("FlightDrain: energía vacía → aterrizaje forzoso");
        }
    }
}