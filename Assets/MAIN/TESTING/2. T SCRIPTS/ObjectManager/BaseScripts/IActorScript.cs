using UnityEngine;

public abstract class IActorScript : MonoBehaviour
{
    protected bool is_executing = false;

    public virtual bool Solve()
    {
        if (!MeetsRequirements())
            return false;

        if (!is_executing)
            StartExecution();
        else
            UpdateExecution();

        return true;
    }

    public virtual void StartExecution()
    {
        is_executing = true;
    }

    public virtual void StopExecution()
    {
        is_executing = false;
    }

    public abstract bool MeetsRequirements();
    public abstract void UpdateExecution();

}