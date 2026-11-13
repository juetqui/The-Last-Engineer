using UnityEngine;

/// <summary>
/// Platform displacement a passenger still has to apply. Accumulates instead of overwriting, so a
/// frame where a platform moves twice (e.g. it passes a waypoint) or two platforms report is not lost.
/// </summary>
public sealed class PassengerDisplacement
{
    private Vector3 _pending = Vector3.zero;

    public void Add(Vector3 displacement)
    {
        _pending += displacement;
    }

    /// <summary>Returns everything accumulated since the last call and clears it.</summary>
    public Vector3 Consume()
    {
        Vector3 pending = _pending;
        _pending = Vector3.zero;
        return pending;
    }
}
