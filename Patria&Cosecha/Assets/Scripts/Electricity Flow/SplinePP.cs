using UnityEngine.Splines;

public class SplinePP : ISplinePathProvider
{
    private readonly SplineContainer[] _splines;

    public SplinePP(SplineContainer[] splines)
    {
        _splines = splines;
    }

    public SplineContainer GetNextSpline(int currentIndex)
    {
        return currentIndex < _splines.Length ? _splines[currentIndex] : _splines[_splines.Length - 1];
    }
}
