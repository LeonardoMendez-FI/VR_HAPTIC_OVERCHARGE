using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyActor : ActorBase
{
    [Header("Enemy References")]
    public Transform playerTarget;
    public NavMeshAgent agent;
}