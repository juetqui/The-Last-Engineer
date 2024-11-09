using System.Diagnostics;
using UnityEngine.Splines;

public class TrailController
{
    private SplineAnimate _animator = default;
    private ISplinePathProvider _pathProvider = default;
    private int _currentIndex = default;

    public int CurrentIndex { get { return _currentIndex; } }

    public TrailController(SplineAnimate animator, ISplinePathProvider pathProvider)
    {
        _animator = animator;
        _pathProvider = pathProvider;
        _currentIndex = 0;
    }

    public void OnStart()
    {
        _animator.Container = _pathProvider.GetNextSpline(_currentIndex);
    }

    public void MoveToNextSpline()
    {
        _currentIndex++;
        _animator.Container = _pathProvider.GetNextSpline(_currentIndex);
    }
}
