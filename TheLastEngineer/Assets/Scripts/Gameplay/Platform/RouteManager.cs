using UnityEngine;

public class RouteManager
{
    private readonly Transform[] _points;
    private int _index;

    public RouteManager(Transform[] points) => _points = points;

    public Vector3 CurrentPoint => _points[_index].position;

    public void Advance()
    {
        _index = (_index + 1) % _points.Length;
    }
}
