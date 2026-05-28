using UnityEngine;

/// <summary>
/// Bridges GazeManager events to IGazeTarget visual callbacks.
///
/// THIS ACTOR CONTAINS NO RENDERING CODE.
/// It does not touch renderers, materials, shaders, or UI elements.
/// Targets own their own visuals entirely.
///
/// Responsibilities:
///   1. Forward per-frame focus progress → target.OnGazeFocusUpdate(progress)
///   2. Schedule target.OnGazeLost() after a configurable delay (for smooth fade-out)
///   3. Cancel pending fades when a new target is acquired before the delay completes
///
/// The delay between gaze loss and OnGazeLost() gives targets time to animate
/// a graceful fade rather than snapping off immediately.
/// </summary>
public class DetectionVisualActor : GazeActor
{
    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("Fade Timing")]
    [Tooltip("Seconds between OnGazeExit (immediate logic event) and OnGazeLost " +
             "(visual fade trigger). Set to 0 for instant visual cutoff.")]
    [Range(0f, 2f)]
    public float lostFadeDelay = 0.4f;

    // ─── State ────────────────────────────────────────────────────────────────

    private IGazeTarget _trackedTarget;    // target currently receiving visual updates
    private float _lostFadeTimer = -1f;    // -1 = no fade pending; ≥0 = countdown active

    // ─── Actor lifecycle ──────────────────────────────────────────────────────

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

        // Always unsubscribe, even if GazeManager is being destroyed
        if (GazeManager != null)
        {
            GazeManager.OnGazeTargetChanged -= HandleTargetChanged;
            GazeManager.OnGazeTargetFocused -= HandleTargetFocused;
            GazeManager.OnGazeTargetLost    -= HandleTargetLost;
        }

        ForceCompleteFade();
    }

    /// <summary>
    /// Called every frame by GazeManager's actor loop (non-exclusive).
    /// Forwards focus progress to the tracked target and ticks the fade timer.
    /// </summary>
    public override void UpdateExecution()
    {
        // ── 1. Forward focus progress to active target ─────────────────────────
        // Skip if a fade is in progress — the target is mid fade-out, not actively tracking
        if (_trackedTarget != null && _lostFadeTimer < 0f)
        {
            _trackedTarget.OnGazeFocusUpdate(GazeManager.FocusProgress);
        }

        // ── 2. Tick the lost-fade timer ────────────────────────────────────────
        if (_lostFadeTimer >= 0f)
        {
            _lostFadeTimer -= Time.deltaTime;

            if (_lostFadeTimer < 0f)
            {
                // Delay complete — tell target to begin its visual fade-out
                _trackedTarget?.OnGazeLost();
                _trackedTarget = null;
            }
        }
    }

    // Clean up if this actor is destroyed mid-session
    private void OnDestroy() => ForceCompleteFade();

    // ─── Event handlers ───────────────────────────────────────────────────────

    /// <summary>
    /// A new target has been acquired (or null if gaze went idle).
    /// OnGazeEnter() was already called on the new target by GazeManager.
    /// </summary>
    private void HandleTargetChanged(IGazeTarget newTarget)
    {
        // If the previous target's fade was pending, complete it immediately
        // (it's about to become irrelevant — don't leave it in a broken visual state)
        if (_lostFadeTimer >= 0f && _trackedTarget != null)
        {
            _trackedTarget.OnGazeLost();
        }

        _lostFadeTimer = -1f;
        _trackedTarget = newTarget;
        // OnGazeEnter() already handled by GazeManager — no duplicate call here
    }

    /// <summary>
    /// Full lock was achieved. OnGazeFocused() was already called by GazeManager.
    /// This hook is reserved for cross-actor coordination (e.g. triggering energy drain).
    /// </summary>
    private void HandleTargetFocused(IGazeTarget target)
    {
        // OnGazeFocused() is already called on the target by GazeManager.
        // Extend here if DetectionVisualActor needs to coordinate with other actors on lock.
    }

    /// <summary>
    /// Active target was lost (gaze moved off it). OnGazeExit() was already called
    /// by GazeManager. We start the fade delay countdown here.
    /// </summary>
    private void HandleTargetLost()
    {
        // Only schedule a fade if we have a target waiting to receive it
        if (_trackedTarget != null)
            _lostFadeTimer = lostFadeDelay;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Immediately triggers OnGazeLost on the tracked target regardless of timer.
    /// Called when the actor is stopped or destroyed to prevent visual orphans.
    /// </summary>
    private void ForceCompleteFade()
    {
        if (_trackedTarget != null)
            _trackedTarget.OnGazeLost();

        _lostFadeTimer = -1f;
        _trackedTarget = null;
    }
}
