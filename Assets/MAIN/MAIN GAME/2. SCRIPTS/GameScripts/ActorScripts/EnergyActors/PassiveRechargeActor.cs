using UnityEngine;

public class PassiveRechargeActor : EnergyActor
{
    public FlightEnergyDrainActor flightDrainActor;
    public MoveManager moveManager;
    public GazeEnergyDrainActor gazeDrainActor;   // <-- nuevo: arrastra el GazeEnergyDrainActor del jugador

    [Header("Recharge Settings")]
    public float rechargeTimeMultiplier = 2f;
    public float rechargeDelay = 3f;

    [Header("Debug")]
    public bool showDebug = false;

    private float rechargeRate;
    private float timeSinceStart;
    private bool hasTouchedGround;
    private float _computedRechargeTime;

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (moveManager == null) return false;

        // Solo en suelo, sin atacar, y sin estar drenando a un enemigo
        if (moveManager.isFlying || moveManager.isAttacking) return false;
        if (gazeDrainActor != null && gazeDrainActor.IsDraining) return false;   // <-- pausa recarga pasiva

        return true;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        _computedRechargeTime = flightDrainActor != null ? flightDrainActor.hoverTime * rechargeTimeMultiplier : 10f;
        rechargeRate = managerScript.max_energy / _computedRechargeTime;
        timeSinceStart = 0f;
        hasTouchedGround = false;
        if (showDebug) Debug.Log("[PassiveRecharge] StartExecution. Tasa recarga: " + rechargeRate);
    }

    public override void UpdateExecution()
    {
        if (!hasTouchedGround)
        {
            if (moveManager.IsGrounded())
            {
                hasTouchedGround = true;
                timeSinceStart = 0f;
                if (showDebug) Debug.Log("[PassiveRecharge] Tocó el suelo, inicia cuenta atrás.");
            }
            else
            {
                return;
            }
        }

        timeSinceStart += Time.deltaTime;

        if (timeSinceStart >= rechargeDelay && rechargeRate > 0f)
        {
            float rechargeThisFrame = rechargeRate * Time.deltaTime;
            managerScript.modify_energy(rechargeThisFrame);
            if (showDebug) Debug.Log($"[PassiveRecharge] +{rechargeThisFrame:F2} energía. Actual: {managerScript.normalized_local:F2}");
        }
    }

    public override void StopExecution()
    {
        base.StopExecution();
        timeSinceStart = 0f;
        hasTouchedGround = false;
        if (showDebug) Debug.Log("[PassiveRecharge] StopExecution.");
    }
}