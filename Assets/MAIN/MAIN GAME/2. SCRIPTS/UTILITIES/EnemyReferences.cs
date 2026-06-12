using UnityEngine;

public class EnemyReferences : MonoBehaviour
{
    [Header("Managers (assign in prefab)")]
    public EnemyManager  enemyManager;
    public EnergyManager energyManager;

    private Transform           _playerTarget;
    private GazeManager         _playerGaze;
    private EnergyManager       _playerEnergy;
    private AttackSequenceActor _attackSequenceActor;

    public void SetReferences(Transform playerTarget, GazeManager playerGaze,
                              EnergyManager playerEnergy, AttackSequenceActor attackSeq)
    {
        _playerTarget        = playerTarget;
        _playerGaze          = playerGaze;
        _playerEnergy        = playerEnergy;
        _attackSequenceActor = attackSeq;

        DistributeToActors();
    }

    private void DistributeToActors()
    {
        // ── EnemyManager actors ─────────────────────────────────────────────
        if (enemyManager != null)
        {
            foreach (var actor in enemyManager.actors)
            {
                // ── Movimiento ──────────────────────────────────────────
                if (actor is GroundMoveActor groundMove)
                    groundMove.playerTarget = _playerTarget;

                else if (actor is FlightMoveActor flightMove)
                    flightMove.playerTarget = _playerTarget;

                else if (actor is PatrolActor patrolNew)
                    patrolNew.playerTarget = _playerTarget;

                // ── Ataque ──────────────────────────────────────────────
                else if (actor is NearAttackActor nearAtk)
                    nearAtk.playerTarget = _playerTarget;

                else if (actor is FarAttackActor farAtk)
                    farAtk.playerTarget = _playerTarget;

                else if (actor is KamikazeAttackActor kamikaze)
                {
                    kamikaze.playerTarget        = _playerTarget;
                    kamikaze.playerEnergyManager = _playerEnergy;
                }
            }
        }

        // ── EnergyManager actors ────────────────────────────────────────────
        if (energyManager != null)
        {
            foreach (var actor in energyManager.actors)
            {
                if (actor is EnemyPassiveRechargeActor recharge)
                    recharge.playerGazeManager = _playerGaze;

                else if (actor is EnemyDepletedActor depleted)
                    depleted.attackSequenceActor = _attackSequenceActor;
            }
        }
    }
}