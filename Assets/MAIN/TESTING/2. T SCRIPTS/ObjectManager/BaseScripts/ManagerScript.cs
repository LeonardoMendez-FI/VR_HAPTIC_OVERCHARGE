using UnityEngine;
using System.Collections.Generic;

public class ManagerScript : MonoBehaviour
{
    [Header("Core References")]
    public ElectronicObject electronicObject;

    [Header("Actors")]
    public List<ActorBase> actors = new();

    protected ActorBase current_actor = null;

    public virtual void Update()
    {
        if (actors.Count == 0)
            return;

        if (current_actor != null && current_actor.Solve())
            return;

        current_actor = null;

        foreach (ActorBase actor in actors)
        {
            if (actor.Solve())
            {
                current_actor = actor;
                break;
            }
        }
    }
}