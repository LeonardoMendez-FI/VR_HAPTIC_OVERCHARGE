using UnityEngine;
using System.Collections.Generic;

public class ManagerScript : MonoBehaviour
{
    [Header("Core References")]
    public ElectronicObject electronicObject;

    [Header("Actors")]
    public List<IActorScript> actors = new();

    protected IActorScript current_actor = null;

    public virtual void Update()
    {
        if (actors.Count == 0)
            return;

        if (current_actor != null && current_actor.Solve())
            return;

        current_actor = null;

        foreach (IActorScript actor in actors)
        {
            if (actor.Solve())
            {
                current_actor = actor;
                break;
            }
        }
    }
}