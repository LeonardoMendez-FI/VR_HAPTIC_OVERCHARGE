using UnityEngine;
using System.Collections.Generic;

public class ManagerScript : MonoBehaviour
{
    [Header("Core References")]
    public ElectronicObject electronicObject;

    [Header("Actors")]
    [SerializeField] private List<ActorBase> _actors = new List<ActorBase>();

    public IReadOnlyList<ActorBase> actors => _actors;

    protected ActorBase current_actor = null;

    public virtual void Update()
    {
        if (_actors.Count == 0)
            return;

        if (current_actor != null && current_actor.Solve())
            return;

        current_actor = null;

        foreach (ActorBase actor in _actors)
        {
            if (actor.Solve())
            {
                current_actor = actor;
                break;
            }
        }
    }
}