using System;
using UnityEngine;

public class GazeManager : ManagerScript
{
    [Header("Gaze Detection")]
    public Transform gazeOrigin;
    public LayerMask gazeLayerMask;
    public float maxGazeDistance = 20f;

    [Header("Focus Timing")]
    public float focusThreshold = 1.5f;
    [Range(0.1f, 5f)] public float decayRate = 0.6f;
    public bool resetProgressOnTargetChange = true;

    [Header("Sticky Stabilization")]
    public StickyTargetLogic stickyTargetLogic;   // renombrado

    public IGazeTarget CurrentTarget { get; private set; }
    public float FocusProgress { get; private set; }
    public bool IsLocked { get; private set; }

    public event Action<IGazeTarget> OnGazeTargetChanged;
    public event Action<IGazeTarget> OnGazeTargetFocused;
    public event Action OnGazeTargetLost;
    public event Action<float> OnGazeFocusProgress;

    private float prevFocusProgress;

    private void Awake()
    {
        if (gazeOrigin == null)
            gazeOrigin = Camera.main?.transform;
    }

    public override void Update()
    {
        IGazeTarget detected = DetectTarget();

        IGazeTarget stabilizedTarget = stickyTargetLogic != null
            ? stickyTargetLogic.Stabilize(detected)
            : detected;

        HandleTargetTransition(stabilizedTarget);
        UpdateFocusProgress();

        if (!Mathf.Approximately(prevFocusProgress, FocusProgress))
        {
            OnGazeFocusProgress?.Invoke(FocusProgress);
            prevFocusProgress = FocusProgress;
        }

        foreach (ActorBase actor in actors)
        {
            if (actor != null)
                actor.Solve();
        }
    }

    private void HandleTargetTransition(IGazeTarget newTarget)
    {
        if (newTarget == CurrentTarget) return;

        if (CurrentTarget != null)
        {
            CurrentTarget.OnGazeExit();
            IsLocked = false;

            if (resetProgressOnTargetChange)
                FocusProgress = 0f;

            OnGazeTargetLost?.Invoke();
        }

        CurrentTarget = newTarget;

        if (CurrentTarget != null)
        {
            CurrentTarget.OnGazeEnter();
            OnGazeTargetChanged?.Invoke(CurrentTarget);
        }
    }

    private void UpdateFocusProgress()
    {
        if (CurrentTarget != null)
        {
            FocusProgress = Mathf.Clamp01(FocusProgress + Time.deltaTime / focusThreshold);

            if (!IsLocked && FocusProgress >= 1f)
            {
                IsLocked = true;
                CurrentTarget.OnGazeFocused();
                OnGazeTargetFocused?.Invoke(CurrentTarget);
            }
        }
        else
        {
            FocusProgress = Mathf.Clamp01(FocusProgress - Time.deltaTime * decayRate);
        }
    }

    private IGazeTarget DetectTarget()
    {
        if (gazeOrigin == null) return null;

        Ray ray = new Ray(gazeOrigin.position, gazeOrigin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxGazeDistance, gazeLayerMask))
        {
            var target = hit.collider.GetComponent<IGazeTarget>();
            if (target != null && ((1 << hit.collider.gameObject.layer) & target.GazeLayerMask) != 0)
                return target;
        }
        return null;
    }

    public void ClearTarget()
    {
        HandleTargetTransition(null);
        stickyTargetLogic?.ClearSticky();
    }

    public void ForceReleaseTarget()
    {
        HandleTargetTransition(null);
        stickyTargetLogic?.ClearSticky();

        foreach (var actor in actors)
        {
            if (actor is DetectionVisualActor visualActor)
                visualActor.ForceClear();
        }
    }
}