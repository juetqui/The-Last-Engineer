using UnityEngine.Splines;

public interface ISplinePathProvider
{
    public SplineContainer GetNextSpline(int currentIndex);
}
