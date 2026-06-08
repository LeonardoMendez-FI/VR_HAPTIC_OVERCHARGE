using UnityEngine;
using UnityEngine.Events;

public class EnemyPassiveRechargeActor : EnergyActor
{
    [Header("Recharge Settings")]
    public float rechargeRate = 3f;             // energía por segundo
    public float rechargeDelayAfterDamage = 2f; // tiempo sin recibir daño para empezar a recargar

    [Header("References")]
    public GazeManager playerGazeManager;       // arrastrar el GazeManager del jugador
    public GazeTargetBehaviour ownGazeTarget;   // arrastrar el GazeTargetBehaviour de este enemigo

    private float lastDamageTime = -10f;
    private UnityAction<Vector3> onDamagedAction; // delegado para suscribir/desuscribir

    void Awake()
    {
        // Crear el delegado una vez
        onDamagedAction = OnDamaged;
    }

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (managerScript.is_full) return false;  // no recargar si ya está lleno

        // No recargar si el jugador nos está mirando (drenando)
        if (playerGazeManager != null && ownGazeTarget != null)
        {
            if (playerGazeManager.IsLocked && playerGazeManager.CurrentTarget == (IGazeTarget)ownGazeTarget)
                return false;
        }

        // Tiempo desde el último daño
        if (Time.time - lastDamageTime < rechargeDelayAfterDamage)
            return false;

        return true;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        // Suscribirse a daños para resetear el temporizador
        if (managerScript.electronicObject?.structManager != null)
        {
            managerScript.electronicObject.structManager.OnDamagedWithDirection.AddListener(onDamagedAction);
        }
    }

    public override void StopExecution()
    {
        base.StopExecution();
        if (managerScript.electronicObject?.structManager != null)
        {
            managerScript.electronicObject.structManager.OnDamagedWithDirection.RemoveListener(onDamagedAction);
        }
    }

    void OnDamaged(Vector3 direction)
    {
        lastDamageTime = Time.time;
    }

    public override void UpdateExecution()
    {
        managerScript.modify_energy(rechargeRate * Time.deltaTime);
    }
}