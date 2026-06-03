using UnityEngine;

public class StickyTargetLogic : MonoBehaviour
{
    [Header("Stabilization")]
    [Tooltip("Segundos que se mantiene el objetivo tras perderlo.")]
    [Range(0.05f, 1.0f)]
    public float graceTime = 0.15f;

    private IGazeTarget _lastKnownTarget;
    private float _graceTimer;

    public IGazeTarget Stabilize(IGazeTarget rawDetected)
    {
        if (_lastKnownTarget != null && (_lastKnownTarget is MonoBehaviour mb && mb == null))
            _lastKnownTarget = null;

        if (rawDetected != null)
        {
            _lastKnownTarget = rawDetected;
            _graceTimer = graceTime;
            return rawDetected;
        }

        if (_graceTimer > 0f && _lastKnownTarget != null)
        {
            _graceTimer -= Time.deltaTime;
            return _lastKnownTarget;
        }

        _lastKnownTarget = null;
        return null;
    }

    public void ClearSticky()
    {
        _lastKnownTarget = null;
        _graceTimer = 0f;
    }
}