using UnityEngine;

public class ReticleTargetAnimView : ViewBase
{
    public Animator targetAnimator;
    public GazeManager gazeManager;

    protected override void Subscribe()
    {
        if (gazeManager != null)
            gazeManager.OnGazeTargetChanged += OnTargetChanged;
    }

    protected override void Unsubscribe()
    {
        if (gazeManager != null)
            gazeManager.OnGazeTargetChanged -= OnTargetChanged;
    }

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