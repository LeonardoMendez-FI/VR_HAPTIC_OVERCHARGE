using UnityEngine;

public abstract class UIActor<TManager> : MonoBehaviour where TManager : ManagerScript
{
    [Header("Manager Reference")]
    public TManager manager;

    protected virtual void OnEnable()
    {
        if (manager != null) Subscribe();
    }

    protected virtual void OnDisable()
    {
        if (manager != null) Unsubscribe();
    }

    protected abstract void Subscribe();
    protected abstract void Unsubscribe();
}