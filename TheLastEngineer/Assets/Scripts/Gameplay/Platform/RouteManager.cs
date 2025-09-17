using UnityEngine;


public class RouteManager
{
    private readonly Vector3[] _points;
    private int _index;
    private int _dir = 1;

    public RouteManager(Vector3[] points)
    {
        _points = points;
    }

    public bool IsValid => _points != null && _points.Length > 0;
    public Vector3 CurrentPoint => IsValid ? _points[_index] : Vector3.zero;
    public bool AtStart() => _index == 0;
    public bool HasToWait(bool algo) =>/*( _points.Length > 1 && (_index == 0 || _index == _points.Length - 1))||*/algo;

    public void Advance()
    {
        if (!IsValid || _points.Length == 1) return;

        int last = _points.Length - 1;

        if (_index + _dir > last || _index + _dir < 0)
            _dir *= -1;

        _index += _dir;
    }
    

    public void ForceReverse()
    {
        _dir = -1;
    }
}
