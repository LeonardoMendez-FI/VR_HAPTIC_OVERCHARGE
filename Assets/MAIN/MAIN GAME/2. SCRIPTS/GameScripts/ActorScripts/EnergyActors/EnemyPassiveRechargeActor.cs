using UnityEngine;
using UnityEngine.Events;

public class EnemyPassiveRechargeActor : EnergyActor
{
    [Header("Recharge Settings")]
    [Tooltip("Time in seconds to fully recharge from empty. " +
             "The actual per-frame rate is derived from max_energy / rechargeTime, " +
             "so enemies with different energy pools all take the same duration to recover.")]
    public float rechargeTime             = PlayerParameters.ENEMY_RECHARGE_TIME;
    public float rechargeDelayAfterDamage = 2f;

    [Header("References (auto-assigned if left empty)")]
    public GazeManager       playerGazeManager;
    public GazeTargetBehaviour ownGazeTarget;

    private float              _rechargeRate;      // derived in StartExecution
    private float              lastDamageTime = -10f;
    private UnityAction<Vector3> onDamagedAction;
    private bool               _listenerAttached = false;

    private void Awake()
    {
        onDamagedAction = OnDamaged;
    }

    private void Start()
    {
        if (playerGazeManager == null)
        {
            var refs = GetComponentInParent<EnemyReferences>();
            if (refs != null)
                playerGazeManager = refs.playerGazeManager;
        }

        if (ownGazeTarget == null)
        {
            ownGazeTarget = GetComponentInParent<GazeTargetBehaviour>();
            if (ownGazeTarget == null)
                ownGazeTarget = GetComponentInChildren<GazeTargetBehaviour>();
        }
    }

    private void OnDestroy()
    {
        RemoveDamageListener();
    }

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (managerScript.is_full) return false;

        // Halt passive recharge while the player is actively draining this enemy.
        if (playerGazeManager != null && ownGazeTarget != null)
        {
            if (playerGazeManager.IsLocked &&
                playerGazeManager.CurrentTarget == (IGazeTarget)ownGazeTarget)
                return false;
        }

        if (Time.time - lastDamageTime < rechargeDelayAfterDamage)
            return false;

        return true;
    }

    public override void StartExecution()
    {
        base.StartExecution();

        // Derive the per-frame recharge rate from the enemy's actual max_energy so
        // that every enemy type — regardless of pool size — takes rechargeTime
        // seconds to recover from empty. The previous hardcoded 3 f/s was arbitrary
        // and produced wildly different effective recovery times across enemy types.
        _rechargeRate = (rechargeTime > 0f)
            ? managerScript.max_energy / rechargeTime
            : managerScript.max_energy; // instant recover as a safe fallback

        AttachDamageListener();
    }

    public override void StopExecution()
    {
        base.StopExecution();
        RemoveDamageListener();
    }

    private void AttachDamageListener()
    {
        if (_listenerAttached) return;
        var structMgr = managerScript.electronicObject?.structManager;
        if (structMgr != null)
        {
            structMgr.OnDamagedWithDirection.AddListener(onDamagedAction);
            _listenerAttached = true;
        }
    }

    private void RemoveDamageListener()
    {
        if (!_listenerAttached) return;
        var structMgr = managerScript.electronicObject?.structManager;
        if (structMgr != null)
            structMgr.OnDamagedWithDirection.RemoveListener(onDamagedAction);
        _listenerAttached = false;
    }

    private void OnDamaged(Vector3 direction)
    {
        lastDamageTime = Time.time;
    }

    public override void UpdateExecution()
    {
        managerScript.modify_energy(_rechargeRate * Time.deltaTime);
    }
}
