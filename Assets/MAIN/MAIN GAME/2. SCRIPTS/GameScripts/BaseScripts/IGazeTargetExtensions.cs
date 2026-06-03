using UnityEngine;

public static class IGazeTargetExtensions
{
    /// <summary>
    /// Devuelve true si el IGazeTarget sigue siendo un MonoBehaviour válido (no destruido).
    /// </summary>
    public static bool IsAlive(this IGazeTarget target)
    {
        return target != null && (target is MonoBehaviour mb && mb != null);
    }
}