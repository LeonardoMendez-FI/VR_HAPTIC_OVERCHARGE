using UnityEngine;

public class PassiveRechargeActor : EnergyActor
{
    public FlightEnergyDrainActor flightDrainActor;
    public float rechargeTimeMultiplier = 2f;
    public float rechargeTime = 0f;

    [Header("Recharge Delay")]
    public float rechargeDelay = 3f;

    [Header("Debug")]
    public bool showDebug = false;

    private MoveManager moveManager;
    private float rechargeRate;
    private float timeSinceStart;
    private bool hasTouchedGround = false;

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (moveManager == null)
            moveManager = (managerScript.electronicObject as Robot)?.moveManager;
        return moveManager != null && !moveManager.isFlying && !moveManager.isAttacking;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        if (rechargeTime <= 0f && flightDrainActor != null)
            rechargeTime = flightDrainActor.hoverTime * rechargeTimeMultiplier;
        if (rechargeTime > 0f)
            rechargeRate = managerScript.max_energy / rechargeTime;
        else
            rechargeRate = 0f;

        timeSinceStart = 0f;
        hasTouchedGround = false;
        if (showDebug)
            Debug.Log($"PassiveRecharge StartExecution: rate={rechargeRate:F2} delay={rechargeDelay}s");
    }

    public override void UpdateExecution()
    {
        // Verificar contacto con el suelo (usando el ground check del MoveManager)
        if (!hasTouchedGround)
        {
            if (moveManager.IsGrounded())
            {
                hasTouchedGround = true;
                timeSinceStart = 0f;
                if (showDebug) Debug.Log("PassiveRecharge: tocó el suelo, inicia retraso.");
            }
            else
            {
                if (showDebug) Debug.Log("PassiveRecharge: aún en el aire...");
                return;
            }
        }

        timeSinceStart += Time.deltaTime;

        if (timeSinceStart >= rechargeDelay && rechargeRate > 0f)
        {
            float rechargeThisFrame = rechargeRate * Time.deltaTime;
            managerScript.modify_energy(rechargeThisFrame);
            if (showDebug)
                Debug.Log($"PassiveRecharge: +{rechargeThisFrame:F2}");
        }
        else if (showDebug && timeSinceStart < rechargeDelay)
        {
            Debug.Log($"PassiveRecharge: esperando {rechargeDelay - timeSinceStart:F1}s...");
        }
    }

    public override void StopExecution()
    {
        base.StopExecution();
        timeSinceStart = 0f;
        hasTouchedGround = false;
        if (showDebug) Debug.Log("PassiveRecharge: ejecución detenida.");
    }
}