using UnityEngine;

public class DetectionVisualActor : GazeActor
{
    [Header("Fade Timing")]
    [Range(0f, 2f)] public float lostFadeDelay = 0.4f;

    private IGazeTarget _trackedTarget;
    private float _lostFadeTimer = -1f;

    public override bool MeetsRequirements() => GazeManager != null;

    public override void StartExecution()
    {
        base.StartExecution();
        GazeManager.OnGazeTargetChanged += HandleTargetChanged;
        GazeManager.OnGazeTargetFocused += HandleTargetFocused;
        GazeManager.OnGazeTargetLost    += HandleTargetLost;
    }

    public override void StopExecution()
    {
        base.StopExecution();

        if (GazeManager != null)
        {
            GazeManager.OnGazeTargetChanged -= HandleTargetChanged;
            GazeManager.OnGazeTargetFocused -= HandleTargetFocused;
            GazeManager.OnGazeTargetLost    -= HandleTargetLost;
        }

        ForceCompleteFade();
    }

    public override void UpdateExecution()
    {
        if (_trackedTarget != null && _lostFadeTimer < 0f)
        {
            _trackedTarget.OnGazeFocusUpdate(GazeManager.FocusProgress);
        }

        if (_lostFadeTimer >= 0f)
        {
            _lostFadeTimer -= Time.deltaTime;
            if (_lostFadeTimer < 0f)
            {
                _trackedTarget?.OnGazeLost();
                _trackedTarget = null;
            }
        }
    }

    private void OnDestroy() => ForceCompleteFade();

    private void HandleTargetChanged(IGazeTarget newTarget)
    {
        if (_lostFadeTimer >= 0f && _trackedTarget != null)
            _trackedTarget.OnGazeLost();

        _lostFadeTimer = -1f;
        _trackedTarget = newTarget;
    }

    private void HandleTargetFocused(IGazeTarget target) { }

    private void HandleTargetLost()
    {
        if (_trackedTarget != null)
            _lostFadeTimer = lostFadeDelay;
    }

    private void ForceCompleteFade()
    {
        // Verificar que el target aún es válido (no destruido) antes de llamar a sus métodos
        if (_trackedTarget != null)
        {
            // Aseguramos que el target sigue vivo (es MonoBehaviour y no ha sido destruido)
            if (_trackedTarget is MonoBehaviour mb && mb != null)
                _trackedTarget.OnGazeLost();
            else if (!(_trackedTarget is MonoBehaviour)) // Si no es MonoBehaviour, asumimos que no se destruye
                _trackedTarget.OnGazeLost();
        }
        _lostFadeTimer = -1f;
        _trackedTarget = null;
    }

    public void ForceClear()
    {
        _lostFadeTimer = -1f;
        _trackedTarget = null;
    }
}