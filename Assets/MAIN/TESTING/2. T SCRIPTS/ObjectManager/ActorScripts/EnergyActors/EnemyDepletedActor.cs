using UnityEngine;
using System.Collections;

public class EnemyDepletedActor : EnergyActor
{
    [Tooltip("Tiempo que permanece inactivo tras agotar la energía")]
    public float disableTime = 5f;

    [Tooltip("Porcentaje de energía a restaurar (0..1)")]
    [Range(0, 1)] public float restoreFraction = 0.5f;

    private MoveManager moveManager;
    private bool isDisabled = false;

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        return managerScript.is_empty && !isDisabled;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        isDisabled = true;
        // Desactiva movimiento
        if (moveManager == null)
            moveManager = (managerScript.electronicObject as Robot)?.moveManager;
        if (moveManager != null)
            moveManager.enabled = false; // o desactivar actores específicos
        StartCoroutine(RecoverAfterDelay());
    }

    private IEnumerator RecoverAfterDelay()
    {
        yield return new WaitForSeconds(disableTime);
        // Restaurar energía al porcentaje configurado
        managerScript.modify_energy(managerScript.max_energy * restoreFraction);
        // Reactivar movimiento
        if (moveManager != null)
            moveManager.enabled = true;
        isDisabled = false;
        StopExecution();
    }

    public override void UpdateExecution()
    {
        // No necesita hacer nada cada frame, la corrutina maneja la espera
    }
}