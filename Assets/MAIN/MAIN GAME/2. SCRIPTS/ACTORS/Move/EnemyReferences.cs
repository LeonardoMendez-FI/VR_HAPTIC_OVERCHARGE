using UnityEngine;

public class EnemyReferences : MonoBehaviour
{
    [Header("Managers (assign in prefab)")]
    public EnemyManager enemyManager;
    public EnergyManager energyManager;

    private Transform          _playerTarget;
    private GazeManager        _playerGaze;
    private EnergyManager      _playerEnergy;
    private AttackSequenceActor _attackSequenceActor;

    public void SetReferences(Transform playerTarget, GazeManager playerGaze,
                              EnergyManager playerEnergy, AttackSequenceActor attackSeq)
    {
        _playerTarget         = playerTarget;
        _playerGaze           = playerGaze;
        _playerEnergy         = playerEnergy;
        _attackSequenceActor  = attackSeq;

        DistributeToActors();
    }

    private void DistributeToActors()
    {
        // ── Actores de EnemyManager ──────────────────────
        if (enemyManager != null)
        {
            foreach (var actor in enemyManager.actors)
            {
                if (actor is EnemyChaseActor chase)
                    chase.playerTarget = _playerTarget;
                else if (actor is EnemyPatrolActor patrol)
                    patrol.playerTarget = _playerTarget;
                else if (actor is FlyingPursuitActor flying)
                    flying.playerTarget = _playerTarget;
                else if (actor is MeleeAttackActor melee)
                    melee.playerTarget = _playerTarget;
                else if (actor is RangedAttackActor ranged)
                    ranged.playerTarget = _playerTarget;
                else if (actor is KamikazeAttackActor kamikaze)
                {
                    kamikaze.playerTarget        = _playerTarget;
                    kamikaze.playerEnergyManager = _playerEnergy;
                }
                else if (actor is FlyingRangedAttackActor flyRanged)
                    flyRanged.playerTarget = _playerTarget;
            }
        }

        // ── Actores de EnergyManager ────────────────────
        if (energyManager != null)
        {
            foreach (var actor in energyManager.actors)
            {
                if (actor is EnemyPassiveRechargeActor recharge)
                {
                    recharge.playerGazeManager = _playerGaze;
                }
                else if (actor is EnemyDepletedActor depleted)
                {
                    depleted.attackSequenceActor = _attackSequenceActor;
                }
            }
        }
    }
}