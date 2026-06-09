using UnityEngine;
using System.Collections;

public class EnemyDepletedActor : EnergyActor
{
    [Tooltip("Tiempo que permanece inactivo tras agotar la energía")]
    public float disableTime = 5f;

    [Tooltip("Porcentaje de energía a restaurar al volver en sí")]
    [Range(0, 1)] public float restoreFraction = 0.5f;

    [Header("References")]
    public AttackSequenceActor attackSequenceActor;

    // Cached on StartExecution. Enemies are ElectronicObjects, not Robots, so we
    // look for the EnemyManager on the same prefab rather than casting to Robot.
    private EnemyManager enemyManager;
    private bool         isDisabled = false;
    private Coroutine    _recoverCoroutine;

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

        // Locate the EnemyManager on this prefab and disable it so all locomotion
        // and attack actors stop ticking. This is correct for all enemy types
        // (ground and flying) regardless of whether they use NavMeshAgent or
        // direct transform movement.
        enemyManager = managerScript.electronicObject
                                    .GetComponentInParent<EnemyManager>();
        if (enemyManager != null)
            enemyManager.enabled = false;

        _recoverCoroutine = StartCoroutine(RecoverAfterDelay());
    }

    private IEnumerator RecoverAfterDelay()
    {
        yield return new WaitForSeconds(disableTime);

        // Restore a fraction of energy so the enemy wakes up with partial capacity.
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

        // Safety: re-enable EnemyManager if destroyed mid-depletion so the
        // prefab pool or any lingering references aren't left in a disabled state.
        if (enemyManager != null)
            enemyManager.enabled = true;
    }

    public override void UpdateExecution() { }
}
