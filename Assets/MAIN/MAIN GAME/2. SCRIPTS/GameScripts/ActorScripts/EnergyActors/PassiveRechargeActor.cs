using UnityEngine;

public class PassiveRechargeActor : EnergyActor
{
    public FlightEnergyDrainActor flightDrainActor;
    public MoveManager moveManager;   // asignado en inspector

    [Header("Recharge Settings")]
    public float rechargeTimeMultiplier = 2f;
    public float rechargeDelay = 3f;

    [Header("Debug")]
    public bool showDebug = false;

    private float rechargeRate;
    private float timeSinceStart;
    private bool hasTouchedGround;
    private float _computedRechargeTime;  // BUG-09: variable local para no mutar el campo serializado

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        return moveManager != null && !moveManager.isFlying && !moveManager.isAttacking;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        _computedRechargeTime = flightDrainActor != null ? flightDrainActor.hoverTime * rechargeTimeMultiplier : 10f;
        rechargeRate = managerScript.max_energy / _computedRechargeTime;
        timeSinceStart = 0f;
        hasTouchedGround = false;
    }

    public override void UpdateExecution()
    {
        if (!hasTouchedGround)
        {
            if (moveManager.IsGrounded())
            {
                hasTouchedGround = true;
                timeSinceStart = 0f;
            }
            else return;
        }

        timeSinceStart += Time.deltaTime;
        if (timeSinceStart >= rechargeDelay)
            managerScript.modify_energy(rechargeRate * Time.deltaTime);
    }

    public override void StopExecution()
    {
        base.StopExecution();
        timeSinceStart = 0f;
        hasTouchedGround = false;
    }
}