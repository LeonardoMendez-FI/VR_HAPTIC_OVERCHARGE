using UnityEngine;

/// <summary>
/// Clase base para todas las vistas de UI. Se suscribe a eventos en OnEnable y se desuscribe en OnDisable.
/// Las referencias se asignan siempre por inspector.
/// </summary>
public abstract class ViewBase : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        Subscribe();
    }

    protected virtual void OnDisable()
    {
        Unsubscribe();
    }

    protected abstract void Subscribe();
    protected abstract void Unsubscribe();
}