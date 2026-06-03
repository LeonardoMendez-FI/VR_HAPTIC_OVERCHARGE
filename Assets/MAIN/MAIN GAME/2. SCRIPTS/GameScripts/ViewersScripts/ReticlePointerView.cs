using UnityEngine;
using UnityEngine.UI;

public class ReticlePointerView : ViewBase
{
    public Image pointerImage;
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
    // Escuchamos OnGazeTargetLost para restablecer el retículo (mostrar el puntero).
    private void HandleTargetLost() => OnTargetChanged(null);

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