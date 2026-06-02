using UnityEngine;
using UnityEngine.UI;

public class ReticlePointerView : MonoBehaviour
{
    [Header("References")]
    public Image pointerImage;
    public GazeManager gazeManager;

    private void OnEnable()
    {
        if (gazeManager != null)
            gazeManager.OnGazeTargetChanged += OnTargetChanged;
    }

    private void OnDisable()
    {
        if (gazeManager != null)
            gazeManager.OnGazeTargetChanged -= OnTargetChanged;
    }

    private void Start()
    {
        // Asegurar el estado inicial
        OnTargetChanged(gazeManager != null ? gazeManager.CurrentTarget : null);
    }

    private void OnTargetChanged(IGazeTarget newTarget)
    {
        if (pointerImage != null)
            pointerImage.enabled = (newTarget == null);
    }
}