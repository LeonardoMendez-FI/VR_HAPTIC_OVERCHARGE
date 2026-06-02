using UnityEngine;
using UnityEngine.UI;

public class ReticleInnerCircleView : MonoBehaviour
{
    [Header("References")]
    public Image innerCircle;
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
        OnTargetChanged(gazeManager != null ? gazeManager.CurrentTarget : null);
    }

    private void OnTargetChanged(IGazeTarget newTarget)
    {
        if (innerCircle != null)
            innerCircle.enabled = (newTarget != null);
    }
}