using UnityEngine;

/// <summary>
/// Dispatcher for all per-enemy actors (locomotion + attack).
///
/// WHY THIS OVERRIDES Update():
/// The base ManagerScript.Update() uses a single-actor-per-frame priority model:
/// it locks onto the first actor whose MeetsRequirements() returns true and skips
/// all others. That model is correct for the player (mutually exclusive states like
/// ground vs flight), but enemies need locomotion and attack to run simultaneously —
/// a CHOMPY must chase AND bite at the same time, a SKY-SNIPER must orbit AND fire.
///
/// WHY EnemyManager EXISTS SEPARATELY:
/// Enemy locomotion actors use NavMeshAgent, not Rigidbody, so they cannot live
/// under MoveManager without polluting player-specific locomotion logic. Enemy
/// attack actors could conceptually belong to AttackManager, but AttackManager is
/// a player-side score/event manager, not an actor dispatcher. Keeping all enemy
/// actors under EnemyManager gives each enemy a clean, self-contained dispatch
/// loop without coupling to player systems.
/// </summary>
public class EnemyManager : ManagerScript
{
    public override void Update()
    {
        foreach (ActorBase actor in actors)
        {
            if (actor == null) continue;

            if (actor.MeetsRequirements())
                actor.Solve();
            else
                actor.StopExecution();
        }
    }
}
