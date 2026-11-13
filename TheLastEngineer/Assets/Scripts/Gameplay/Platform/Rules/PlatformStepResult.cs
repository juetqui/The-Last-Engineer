using UnityEngine;

/// <summary>Outcome of one platform advance toward a target (see <see cref="PlatformStep"/>).</summary>
public readonly struct PlatformStepResult
{
    /// <summary>Where the platform ends up. Exactly the target when <see cref="Arrived"/>.</summary>
    public Vector3 Position { get; }

    /// <summary>Displacement applied this advance; what the passenger must be moved by.</summary>
    public Vector3 Delta { get; }

    public bool Arrived { get; }

    /// <summary>Distance budget not spent because the target was reached first (0 when not arrived).</summary>
    public float Leftover { get; }

    public PlatformStepResult(Vector3 position, Vector3 delta, bool arrived, float leftover)
    {
        Position = position;
        Delta = delta;
        Arrived = arrived;
        Leftover = leftover;
    }
}
