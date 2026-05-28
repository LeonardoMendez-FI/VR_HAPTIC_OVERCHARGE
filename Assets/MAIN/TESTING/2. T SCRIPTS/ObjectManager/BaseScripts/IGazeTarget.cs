using UnityEngine;

/// <summary>
/// Implemented by any GameObject that can be scanned by the robot's gaze system.
///
/// RESPONSIBILITY SEPARATION:
///   GazeManager calls    → OnGazeEnter, OnGazeExit, OnGazeFocused
///   DetectionVisualActor → OnGazeFocusUpdate, OnGazeLost
///
/// Targets own their own visuals. These callbacks are notifications only —
/// the target decides what to do with them (enable outlines, update shaders, etc.).
/// The gaze system never touches renderers or materials directly.
/// </summary>
public interface IGazeTarget
{
    // ─── Called by GazeManager ────────────────────────────────────────────────

    /// <summary>
    /// Gaze raycast first acquired this target.
    /// Trigger: show holographic detection outline.
    /// </summary>
    void OnGazeEnter();

    /// <summary>
    /// Gaze raycast no longer hits this target (immediate logic event).
    /// Visual fade-out is handled separately via OnGazeLost, after a delay.
    /// </summary>
    void OnGazeExit();

    /// <summary>
    /// Full lock achieved — FocusProgress reached 1.
    /// Called once per lock acquisition.
    /// Trigger: activate lock VFX, pulse outline, change color.
    /// </summary>
    void OnGazeFocused();

    // ─── Called by DetectionVisualActor ───────────────────────────────────────

    /// <summary>
    /// Per-frame focus progress update while this is the active target (0..1).
    /// Trigger: drive progress ring shader, show scanning animation.
    /// </summary>
    /// <param name="progress">Normalized focus value (0 = just detected, 1 = fully locked).</param>
    void OnGazeFocusUpdate(float progress);

    /// <summary>
    /// Called after a configurable fade delay once gaze was lost.
    /// Gives the target time to animate a smooth fade-out rather than snapping off.
    /// Trigger: start outline/ring fade coroutine.
    /// </summary>
    void OnGazeLost();

    // ─── Properties ───────────────────────────────────────────────────────────

    /// <summary>
    /// Layer mask this target accepts gaze from.
    /// Used by GazeManager to validate raycast hits.
    /// </summary>
    LayerMask GazeLayerMask { get; }

    /// <summary>
    /// Transform of this target (convenience — avoids casting to MonoBehaviour).
    /// </summary>
    Transform TargetTransform { get; }
}
