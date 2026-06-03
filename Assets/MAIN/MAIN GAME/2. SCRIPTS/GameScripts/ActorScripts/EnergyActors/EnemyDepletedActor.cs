using UnityEngine;
using System.Collections;

public class EnemyDepletedActor : EnergyActor
{
    [Tooltip("Tiempo que permanece inactivo tras agotar la energía")]
    public float disableTime = 5f;

    [Tooltip("Porcentaje de energía a restaurar")]
    [Range(0, 1)] public float restoreFraction = 0.5f;

    [Header("References")]
    public AttackSequenceActor attackSequenceActor;   // referencia desde el inspector

    private MoveManager moveManager;
    private bool isDisabled = false;

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (!managerScript.is_empty) return false;
        // Si el jugador está atacando, no desactivamos el enemigo
        if (attackSequenceActor != null && attackSequenceActor.IsDraining) return false;
        return !isDisabled;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        isDisabled = true;

        var robot = managerScript.electronicObject as Robot;
        if (robot != null)
            moveManager = robot.moveManager;

        if (moveManager != null)
            moveManager.enabled = false;

        StartCoroutine(RecoverAfterDelay());
    }

    private IEnumerator RecoverAfterDelay()
    {
        yield return new WaitForSeconds(disableTime);
        managerScript.modify_energy(managerScript.max_energy * restoreFraction);
        if (moveManager != null)
            moveManager.enabled = true;
        isDisabled = false;
        StopExecution();
    }

    public override void UpdateExecution() { }
}