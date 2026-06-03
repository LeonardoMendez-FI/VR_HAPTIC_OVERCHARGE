using UnityEngine;
using UnityEngine.UI;

public class ReticleInnerCircleView : ViewBase
{
    public Image innerCircle;
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
        if (innerCircle != null)
            innerCircle.enabled = (newTarget != null);
    }
}