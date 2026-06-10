using UnityEngine;
using System.Collections;

public class EnemyDepletedActor : EnergyActor
{
    [Tooltip("Tiempo que permanece inactivo tras agotar la energía")]
    public float disableTime = 5f;

    [Tooltip("Porcentaje de energía a restaurar")]
    [Range(0, 1)] public float restoreFraction = 0.5f;

    [Header("References")]
    public AttackSequenceActor attackSequenceActor;
    public EnemyManager enemyManager;   // ← asignar en el Inspector

    private bool isDisabled = false;
    private Coroutine _recoverCoroutine;

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (!managerScript.is_empty) return false;
        if (attackSequenceActor != null && attackSequenceActor.IsDraining) return false;
        return !isDisabled;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        isDisabled = true;

        if (enemyManager != null)
            enemyManager.enabled = false;

        _recoverCoroutine = StartCoroutine(RecoverAfterDelay());
    }

    private IEnumerator RecoverAfterDelay()
    {
        yield return new WaitForSeconds(disableTime);

        if (managerScript != null)
            managerScript.modify_energy(managerScript.max_energy * restoreFraction);

        if (enemyManager != null)
            enemyManager.enabled = true;

        isDisabled = false;
        StopExecution();
    }

    private void OnDestroy()
    {
        if (_recoverCoroutine != null)
            StopCoroutine(_recoverCoroutine);

        if (enemyManager != null)
            enemyManager.enabled = true;
    }

    public override void UpdateExecution() { }
}