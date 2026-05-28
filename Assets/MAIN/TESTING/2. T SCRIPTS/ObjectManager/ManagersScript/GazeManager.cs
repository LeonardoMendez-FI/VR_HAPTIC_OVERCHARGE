using System;
using UnityEngine;

/// <summary>
/// Manages all gaze logic: raycasting, target state transitions, focus progression,
/// and lock detection. Fires events that Actors consume.
///
/// This class contains ZERO rendering logic. Visual feedback is handled entirely
/// by DetectionVisualActor and the targets themselves.
///
/// Actor loop: unlike the base ManagerScript (exclusive, first-wins),
/// GazeManager runs ALL actors every frame so visual and logic actors coexist.
/// </summary>
public class GazeManager : ManagerScript
{
    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("Gaze Detection")]
    [Tooltip("Origin of the gaze ray. Assign the camera or eye transform. Falls back to Camera.main.")]
    public Transform gazeOrigin;

    [Tooltip("Layer mask for raycast. Only objects on matching layers are considered gaze targets.")]
    public LayerMask gazeLayerMask;

    [Tooltip("Maximum gaze detection range in world units.")]
    public float maxGazeDistance = 20f;

    [Header("Focus Timing")]
    [Tooltip("Seconds of continuous gaze required to achieve full lock (FocusProgress = 1).")]
    public float focusThreshold = 1.5f;

    [Tooltip("Rate at which FocusProgress decays per second when no target is visible.")]
    [Range(0.1f, 5f)]
    public float decayRate = 0.6f;

    [Tooltip("If true, FocusProgress resets to 0 when the active target changes. " +
             "If false, partial progress is carried forward to the new target.")]
    public bool resetProgressOnTargetChange = true;

    // ─── State (read by Actors, UI, other systems) ────────────────────────────

    /// <summary>The target currently being gazed at (null if none).</summary>
    public IGazeTarget CurrentTarget { get; private set; }

    /// <summary>Normalized focus progress toward lock (0..1).</summary>
    public float FocusProgress { get; private set; }

    /// <summary>True once FocusProgress reaches 1 on the current target.</summary>
    public bool IsLocked { get; private set; }

    // ─── Events ───────────────────────────────────────────────────────────────

    /// <summary>Fired when the active target changes (including to null).</summary>
    public event Action<IGazeTarget> OnGazeTargetChanged;

    /// <summary>Fired when full lock is achieved on a target.</summary>
    public event Action<IGazeTarget> OnGazeTargetFocused;

    /// <summary>Fired when the active target is lost (immediately, before fade).</summary>
    public event Action OnGazeTargetLost;

    /// <summary>Fired every frame with the current FocusProgress value (0..1).</summary>
    public event Action<float> OnGazeFocusProgress;

    // ─── Private ──────────────────────────────────────────────────────────────

    private StickyTargetActor _stickyActor;

    // ─── Unity lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        if (gazeOrigin == null)
            gazeOrigin = Camera.main?.transform;
    }

    /// <summary>
    /// Overrides ManagerScript.Update to use non-exclusive actor execution.
    /// All actors run every frame (unlike the base which stops at first successful actor).
    /// </summary>
    public override void Update()
    {
        // ── 1. Raw detection ──────────────────────────────────────────────────
        IGazeTarget detected = DetectTarget();

        // ── 2. Sticky stabilization (reduces flicker on fast movement) ────────
        EnsureStickyActor();
        IGazeTarget stabilizedTarget = _stickyActor != null
            ? _stickyActor.Stabilize(detected)
            : detected;

        // ── 3. Target transition ──────────────────────────────────────────────
        HandleTargetTransition(stabilizedTarget);

        // ── 4. Focus progression ──────────────────────────────────────────────
        UpdateFocusProgress();

        // ── 5. Broadcast focus progress ───────────────────────────────────────
        OnGazeFocusProgress?.Invoke(FocusProgress);

        // ── 6. Run ALL actors (non-exclusive) ─────────────────────────────────
        foreach (IActorScript actor in actors)
        {
            if (actor != null)
                actor.Solve();
        }
    }

    // ─── Target transition ────────────────────────────────────────────────────

    private void HandleTargetTransition(IGazeTarget newTarget)
    {
        if (newTarget == CurrentTarget) return;

        // ── Exit previous target ───────────────────────────────────────────────
        if (CurrentTarget != null)
        {
            CurrentTarget.OnGazeExit();
            IsLocked = false;

            if (resetProgressOnTargetChange)
                FocusProgress = 0f;

            OnGazeTargetLost?.Invoke();
        }

        CurrentTarget = newTarget;

        // ── Enter new target ───────────────────────────────────────────────────
        if (CurrentTarget != null)
        {
            CurrentTarget.OnGazeEnter();
            OnGazeTargetChanged?.Invoke(CurrentTarget);
        }
    }

    // ─── Focus progression ────────────────────────────────────────────────────

    private void UpdateFocusProgress()
    {
        if (CurrentTarget != null)
        {
            // Build up toward lock
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
            // Decay when no target
            FocusProgress = Mathf.Clamp01(FocusProgress - Time.deltaTime * decayRate);
        }
    }

    // ─── Raycast ─────────────────────────────────────────────────────────────

    private IGazeTarget DetectTarget()
    {
        if (gazeOrigin == null) return null;

        Ray ray = new Ray(gazeOrigin.position, gazeOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxGazeDistance, gazeLayerMask))
        {
            var target = hit.collider.GetComponent<IGazeTarget>();

            // Validate the target accepts gaze from this layer
            if (target != null && ((1 << hit.collider.gameObject.layer) & target.GazeLayerMask) != 0)
                return target;
        }

        return null;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private void EnsureStickyActor()
    {
        if (_stickyActor != null) return;
        _stickyActor = GetComponentInChildren<StickyTargetActor>();
    }

    /// <summary>
    /// Force-drops the current target immediately (useful for cut-scenes, UI, etc.).
    /// </summary>
    public void ClearTarget()
    {
        HandleTargetTransition(null);
        _stickyActor?.ClearSticky();
    }
}
