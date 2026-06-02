using UnityEngine;

public class ReticleTargetAnimView : MonoBehaviour
{
    [Header("References")]
    public Animator targetAnimator;
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
        if (targetAnimator != null)
            targetAnimator.speed = (newTarget != null) ? 0f : 1f;
    }
}