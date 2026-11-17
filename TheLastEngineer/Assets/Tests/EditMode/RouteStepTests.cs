using NUnit.Framework;

public class RouteStepTests
{
    private const int Forward = 1;
    private const int Backward = -1;

    [Test]
    public void Advance_InsideRoute_MovesOnePointInItsDirection()
    {
        int index = 1, dir = Forward;

        RouteStep.Advance(4, false, ref index, ref dir);

        Assert.AreEqual(2, index);
        Assert.AreEqual(Forward, dir);
    }

    [Test]
    public void Advance_PastTheEndWithoutLoop_BouncesBack()
    {
        int index = 3, dir = Forward;

        RouteStep.Advance(4, false, ref index, ref dir);

        Assert.AreEqual(2, index);
        Assert.AreEqual(Backward, dir);
    }

    [Test]
    public void Advance_PastTheStartWithoutLoop_BouncesForward()
    {
        int index = 0, dir = Backward;

        RouteStep.Advance(4, false, ref index, ref dir);

        Assert.AreEqual(1, index);
        Assert.AreEqual(Forward, dir);
    }

    [Test]
    public void Advance_PastTheEndWithLoop_GoesBackToTheFirstPoint()
    {
        int index = 3, dir = Forward;

        RouteStep.Advance(4, true, ref index, ref dir);

        Assert.AreEqual(0, index);
        Assert.AreEqual(Forward, dir);
    }

    [Test]
    public void Advance_SinglePointRoute_StaysPut()
    {
        int index = 0, dir = Forward;

        RouteStep.Advance(1, false, ref index, ref dir);

        Assert.AreEqual(0, index);
    }

    [Test]
    public void NextStation_CurrentPointIsStation_ReturnsIt()
    {
        var stations = new[] { true, false, true };

        Assert.AreEqual(2, RouteStep.NextStation(stations, 2, Forward, false));
    }

    [Test]
    public void NextStation_SkipsPassThroughPoints()
    {
        var stations = new[] { true, false, false, true };

        Assert.AreEqual(3, RouteStep.NextStation(stations, 1, Forward, false));
    }

    [Test]
    public void NextStation_PastTheEndWithoutLoop_FindsTheStationOnTheWayBack()
    {
        var stations = new[] { true, false, false };

        Assert.AreEqual(0, RouteStep.NextStation(stations, 1, Forward, false));
    }

    [Test]
    public void NextStation_PastTheEndWithLoop_FindsTheFirstStation()
    {
        var stations = new[] { false, true, false, false };

        Assert.AreEqual(1, RouteStep.NextStation(stations, 2, Forward, true));
    }

    [Test]
    public void NextStation_RouteWithoutStations_ReturnsNoStation()
    {
        var stations = new[] { false, false, false };

        Assert.AreEqual(RouteStep.NoStation, RouteStep.NextStation(stations, 0, Forward, false));
    }

    [Test]
    public void NextStation_IndexOutOfRange_ReturnsNoStation()
    {
        var stations = new[] { true };

        Assert.AreEqual(RouteStep.NoStation, RouteStep.NextStation(stations, 5, Forward, false));
        Assert.AreEqual(RouteStep.NoStation, RouteStep.NextStation(null, 0, Forward, false));
    }
}
