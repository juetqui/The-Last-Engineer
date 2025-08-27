    using UnityEngine;

public class RouteManager
{
    private readonly Transform[] _points;
    private int _index;

    public RouteManager(Transform[] points) => _points = points;

    public bool IsValid => _points != null && _points.Length > 0;
    public Vector3 CurrentPoint => IsValid ? _points[_index].position : Vector3.zero;
    public void Advance() { if (IsValid) _index = (_index + 1) % _points.Length; }
}
