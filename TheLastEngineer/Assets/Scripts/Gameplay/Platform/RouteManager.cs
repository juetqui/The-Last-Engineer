using UnityEngine;

public class RouteManager
{
    private readonly Vector3[] _points;
    private readonly PlatformController _pc;
    private int _index;
    private int _dir = 1;

    public RouteManager(Vector3[] points, PlatformController platformController)
    {
        _points = points;
        _pc = platformController;
    }

    public bool IsValid => _points != null && _points.Length > 0;
    public Vector3 CurrentPoint => IsValid ? _points[_index] : Vector3.zero;
    public bool AtStart() => _index == 0;
    public bool HasToWait(bool algo) => algo;
    public int TravelDir => _dir;


    public int CurrentIndex => _index;

    public void Advance()
    {
        if (!IsValid) return;

        RouteStep.Advance(_points.Length, _pc.isReversed, ref _index, ref _dir);
    }

    /// <summary>Index of the station the platform is heading to, or <see cref="RouteStep.NoStation"/>.</summary>
    public int NextStation(bool[] isStation)
    {
        if (!IsValid) return RouteStep.NoStation;

        return RouteStep.NextStation(isStation, _index, _dir, _pc.isReversed);
    }

    public void ForceReverse()
    {
        _dir = -1;
    }
}
