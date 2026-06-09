using UnityEngine;

[RequireComponent(typeof(GazeVisualController))]
[DisallowMultipleComponent]
public class GazeTargetBehaviour : MonoBehaviour, IGazeTarget
{
    [Header("Gaze Detection")]
    public LayerMask gazeLayerMask;

    [Header("Target References (manual)")]
    [Tooltip("Arrastra aquí el ElectronicObject del enemigo.")]
    public ElectronicObject targetElectronicObject;

    [Tooltip("Arrastra aquí el GameObject raíz del enemigo (el que debe ser destruido al final del ataque).")]
    public GameObject rootToDestroy;

    private GazeVisualController _visuals;

    public LayerMask GazeLayerMask => gazeLayerMask;
    public Transform TargetTransform => transform;

    protected virtual void Awake()
    {
        _visuals = GetComponent<GazeVisualController>();
        if (_visuals == null)
            Debug.LogError($"[GazeTargetBehaviour] GazeVisualController no encontrado en {name}.", this);
    }

    public virtual void OnGazeEnter()
    {
        _visuals?.ShowDetected();
        OnGazeEnterInternal();
    }

    public virtual void OnGazeExit()
    {
        OnGazeExitInternal();
    }

    public virtual void OnGazeFocusUpdate(float progress)
    {
        _visuals?.UpdateProgress(progress);
        OnGazeFocusUpdateInternal(progress);
    }

    public virtual void OnGazeFocused()
    {
        _visuals?.ShowLocked();
        OnGazeFocusedInternal();
    }

    public virtual void OnGazeLost()
    {
        _visuals?.StartFadeOut();
        OnGazeLostInternal();
    }

    protected virtual void OnGazeEnterInternal() { }
    protected virtual void OnGazeExitInternal() { }
    protected virtual void OnGazeFocusUpdateInternal(float progress) { }

    protected virtual void OnGazeFocusedInternal()
    {
        // Para botones de menú (StartMenu, Tutorial, etc.)
        var button = GetComponent<WorldSpaceStartButton>();
        if (button != null) button.OnGazeLocked();
    }

    protected virtual void OnGazeLostInternal() { }
}