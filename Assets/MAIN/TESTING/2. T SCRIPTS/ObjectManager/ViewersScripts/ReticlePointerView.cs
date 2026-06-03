using UnityEngine;
using UnityEngine.UI;

public class ReticlePointerView : ViewBase
{
    public Image pointerImage;
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
        if (pointerImage != null)
            pointerImage.enabled = (newTarget == null);
    }
}