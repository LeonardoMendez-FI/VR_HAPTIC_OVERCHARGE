using UnityEngine;

/// <summary>
/// Prevents rapid target flickering by holding the last known target during
/// a brief grace period after the gaze raycast momentarily misses.
///
/// DESIGN: GazeManager calls Stabilize(rawDetected) each frame BEFORE the
/// target transition logic, so sticky stabilization happens upstream.
/// This actor is a filter utility, not a traditional exclusive actor.
///
/// The grace window resets every time a valid target is detected.
/// Once the window expires, null is returned and GazeManager transitions away.
/// </summary>
public class StickyTargetActor : GazeActor
{
    [Header("Stabilization")]
    [Tooltip("How many seconds to hold the last target after the gaze raycast stops hitting it.\n" +
             "Prevents flickering when the player looks near a target's edge.")]
    [Range(0.05f, 1.0f)]
    public float graceTime = 0.15f;

    // ─── State ────────────────────────────────────────────────────────────────

    private IGazeTarget _lastKnownTarget;
    private float _graceTimer;

    // ─── Actor interface ──────────────────────────────────────────────────────

    public override bool MeetsRequirements() => GazeManager != null;

    /// <summary>
    /// Standard actor update — stabilization happens via Stabilize(), not Solve().
    /// This keeps the actor registered in the manager's actor list while doing
    /// nothing in the execution path (Stabilize is called directly by GazeManager).
    /// </summary>
    public override void UpdateExecution() { }

    // ─── Core API (called directly by GazeManager) ────────────────────────────

    /// <summary>
    /// Accepts the raw detected target each frame and returns a stabilized result.
    ///
    /// Rules:
    ///   - If rawDetected is non-null → refresh grace window, return rawDetected.
    ///   - If rawDetected is null and grace timer is active → hold last target.
    ///   - If rawDetected is null and grace timer expired → return null (allow transition).
    /// </summary>
    /// <param name="rawDetected">The target the raycast hit this frame (may be null).</param>
    /// <returns>The stabilized target to use this frame.</returns>
    public IGazeTarget Stabilize(IGazeTarget rawDetected)
    {
        if (rawDetected != null)
        {
            // Active hit — refresh the grace window
            _lastKnownTarget = rawDetected;
            _graceTimer = graceTime;
            return rawDetected;
        }

        // No hit — burn down the grace timer
        if (_graceTimer > 0f)
        {
            _graceTimer -= Time.deltaTime;
            return _lastKnownTarget; // hold last target
        }

        // Grace expired — allow the target to be cleared
        _lastKnownTarget = null;
        return null;
    }

    /// <summary>
    /// Immediately clears the sticky hold without waiting for grace to expire.
    /// Called by GazeManager.ClearTarget() and useful for scene transitions.
    /// </summary>
    public void ClearSticky()
    {
        _lastKnownTarget = null;
        _graceTimer = 0f;
    }
}
