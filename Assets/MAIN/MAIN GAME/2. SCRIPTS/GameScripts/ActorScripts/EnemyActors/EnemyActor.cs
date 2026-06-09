using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyActor : ActorBase
{
    [Header("Enemy References")]
    public Transform playerTarget;
    public NavMeshAgent agent;

    protected virtual void Start()
    {
        var refs = GetComponentInParent<EnemyReferences>();
        if (refs != null && refs.playerTarget != null)
            playerTarget = refs.playerTarget;
    }
}