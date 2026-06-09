using UnityEngine;

/// <summary>
/// Base class for all actors that operate under a GazeManager.
///
/// Skips the ElectronicObject requirement from the base ActorScript —
/// GazeManager is a standalone manager that doesn't need an ElectronicObject.
///
/// Usage: inherit from GazeActor and access GazeManager directly via
/// the <see cref="GazeManager"/> property.
/// </summary>
public abstract class GazeActor : ActorScript<GazeManager>
{
    /// <summary>
    /// Typed reference to the manager. Always valid when MeetsRequirements() returns true.
    /// </summary>
    protected GazeManager GazeManager => managerScript;

    /// <summary>
    /// GazeActors only require a valid GazeManager reference.
    /// The base class ElectronicObject check is intentionally skipped here.
    /// </summary>
    public override bool MeetsRequirements()
    {
        return managerScript != null;
    }
}
