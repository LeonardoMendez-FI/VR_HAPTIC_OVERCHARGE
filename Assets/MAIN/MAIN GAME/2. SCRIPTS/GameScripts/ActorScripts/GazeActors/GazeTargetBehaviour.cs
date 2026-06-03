using UnityEngine;

/// <summary>
/// Base MonoBehaviour for any GameObject that can be scanned by the robot's gaze.
///
/// HOW TO USE:
///   1. Add GazeTargetBehaviour (or a subclass) to any scannable object.
///   2. Add GazeVisualController to the same GameObject and wire up its fields.
///   3. Set gazeLayerMask to the layer(s) this target lives on.
///   4. Make sure the object is on a layer included in GazeManager.gazeLayerMask.
///
/// EXTENDING:
///   Override the *Internal() methods to add gameplay logic (e.g. trigger
///   energy transfer on lock) without touching visual code. The visual
///   controller handles everything visual.
///
/// DESIGN NOTE:
///   This class intentionally contains no visual code. All visuals are
///   delegated to GazeVisualController. Gameplay responses go in the
///   *Internal() hooks or in subclasses.
/// </summary>
[RequireComponent(typeof(GazeVisualController))]
[DisallowMultipleComponent]
public class GazeTargetBehaviour : MonoBehaviour, IGazeTarget
{
    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("Gaze Detection")]
    [Tooltip("Layer mask this target accepts gaze from.\n" +
             "Should include the layer this GameObject is on.\n" +
             "Must overlap with GazeManager.gazeLayerMask for the target to be detected.")]
    public LayerMask gazeLayerMask;

    // ─── IGazeTarget ──────────────────────────────────────────────────────────

    public LayerMask GazeLayerMask  => gazeLayerMask;
    public Transform TargetTransform => transform;

    // ─── Private ──────────────────────────────────────────────────────────────

    private GazeVisualController _visuals;

    // ─── Unity lifecycle ──────────────────────────────────────────────────────

    protected virtual void Awake()
    {
        _visuals = GetComponent<GazeVisualController>();

        if (_visuals == null)
            Debug.LogError($"[GazeTargetBehaviour] GazeVisualController not found on {name}. " +
                           "Add one to this GameObject.", this);
    }

    // ─── IGazeTarget callbacks ────────────────────────────────────────────────

    /// <summary>
    /// Gaze first acquired this target.
    /// Visual: shows the holographic detection outline.
    /// </summary>
    public virtual void OnGazeEnter()
    {
        _visuals?.ShowDetected();
        OnGazeEnterInternal();
    }

    /// <summary>
    /// Gaze left this target (immediate logic event).
    /// Visual fade-out is deferred to OnGazeLost() for smooth animation.
    /// </summary>
    public virtual void OnGazeExit()
    {
        OnGazeExitInternal();
    }

    /// <summary>
    /// Per-frame focus progress while this is the active gaze target (0..1).
    /// Visual: drives the progress ring shader fill.
    /// </summary>
    public virtual void OnGazeFocusUpdate(float progress)
    {
        _visuals?.UpdateProgress(progress);
        OnGazeFocusUpdateInternal(progress);
    }

    /// <summary>
    /// Full lock achieved — FocusProgress reached 1.
    /// Visual: activates lock VFX, pulses outline, changes color.
    /// </summary>
    public virtual void OnGazeFocused()
    {
        _visuals?.ShowLocked();
        OnGazeFocusedInternal();
    }

    /// <summary>
    /// Called after the configurable fade delay once gaze was lost.
    /// Visual: begins the fade-out coroutine.
    /// </summary>
    public virtual void OnGazeLost()
    {
        _visuals?.StartFadeOut();
        OnGazeLostInternal();
    }

    // ─── Subclass extension hooks ─────────────────────────────────────────────
    // Override any of these to add gameplay responses without touching visual code.

    /// <summary>Override to respond to initial gaze detection (e.g. play scan sound).</summary>
    protected virtual void OnGazeEnterInternal() { }

    /// <summary>Override to respond to gaze leaving (immediate, before fade).</summary>
    protected virtual void OnGazeExitInternal() { }

    /// <summary>Override to respond to focus progress changes (e.g. build charge-up audio).</summary>
    protected virtual void OnGazeFocusUpdateInternal(float progress) { }

    /// <summary>Override to trigger gameplay effects on full lock (e.g. start energy drain).</summary>
    protected virtual void OnGazeFocusedInternal() { }

    /// <summary>Override to respond to gaze being lost after delay (e.g. stop sounds).</summary>
    protected virtual void OnGazeLostInternal() { }
}
