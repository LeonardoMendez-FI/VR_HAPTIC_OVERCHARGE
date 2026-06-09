using UnityEngine;
using UnityEngine.Events;

public class EnemyPassiveRechargeActor : EnergyActor
{
    [Header("Recharge Settings")]
    public float rechargeTime             = PlayerParameters.ENEMY_RECHARGE_TIME;
    public float rechargeDelayAfterDamage = 2f;

    [Header("References (injected by EnemyReferences)")]
    public GazeManager         playerGazeManager;
    public GazeTargetBehaviour ownGazeTarget;

    private float                _rechargeRate;
    private float                lastDamageTime    = -10f;
    private UnityAction<Vector3> onDamagedAction;
    private bool                 _listenerAttached = false;

    private void Awake()
    {
        onDamagedAction = OnDamaged;
    }

    private void Start()
    {
        // ownGazeTarget se busca en el mismo objeto o padres (el GazeTargetBehaviour está en GazeCollider)
        if (ownGazeTarget == null)
        {
            ownGazeTarget = GetComponentInParent<GazeTargetBehaviour>();
            if (ownGazeTarget == null)
                ownGazeTarget = GetComponentInChildren<GazeTargetBehaviour>();
        }
    }

    private void OnDestroy() => RemoveDamageListener();

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (managerScript.is_full) return false;

        if (playerGazeManager != null && ownGazeTarget != null)
        {
            if (playerGazeManager.IsLocked &&
                playerGazeManager.CurrentTarget == (IGazeTarget)ownGazeTarget)
                return false;
        }

        return Time.time - lastDamageTime >= rechargeDelayAfterDamage;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        _rechargeRate = (rechargeTime > 0f)
            ? managerScript.max_energy / rechargeTime
            : managerScript.max_energy;
        AttachDamageListener();
    }

    public override void StopExecution()
    {
        base.StopExecution();
        RemoveDamageListener();
    }

    public override void UpdateExecution()
    {
        managerScript.modify_energy(_rechargeRate * Time.deltaTime);
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

    private void OnDamaged(Vector3 _) => lastDamageTime = Time.time;
}