using UnityEngine;

public class ReticleTargetAnimView : ViewBase
{
    public Animator targetAnimator;
    public GazeManager gazeManager;

    protected override void Subscribe()
    {
        if (gazeManager != null)
        {
            gazeManager.OnGazeTargetChanged += OnTargetChanged;
            gazeManager.OnGazeTargetLost += HandleTargetLost;
        }
    }

    protected override void Unsubscribe()
    {
        if (gazeManager != null)
        {
            gazeManager.OnGazeTargetChanged -= OnTargetChanged;
            gazeManager.OnGazeTargetLost -= HandleTargetLost;
        }
    }

    // FIX: GazeManager no dispara OnGazeTargetChanged(null) al dejar de mirar.
    // Escuchamos OnGazeTargetLost para restablecer el retículo (reanudar la animación).
    private void HandleTargetLost() => OnTargetChanged(null);

    private void Start()
    {
        OnTargetChanged(gazeManager != null ? gazeManager.CurrentTarget : null);
    }

    private void OnTargetChanged(IGazeTarget newTarget)
    {
        if (targetAnimator != null)
            targetAnimator.speed = (newTarget != null) ? 0f : 1f;
    }
}