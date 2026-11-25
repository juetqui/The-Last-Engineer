using System;
using NUnit.Framework;

public class FloorFadeRulesTests
{
    private const float Tolerance = 1e-5f;
    private const float LowerOpacity = 0.4f;
    private const float UpperOpacity = 0.2f;

    private static float[] Compute(int currentFloor, int floorCount = FloorFadeRules.MaxFloors)
    {
        var fades = new float[floorCount];
        FloorFadeRules.Compute(currentFloor, LowerOpacity, UpperOpacity, fades);
        return fades;
    }

    [Test]
    public void MiddleFloor_IsVisible_NeighboursUseTheirOpacity_OthersHidden()
    {
        var fades = Compute(currentFloor: 1);

        Assert.AreEqual(1f - LowerOpacity, fades[0], Tolerance);
        Assert.AreEqual(0f, fades[1], Tolerance);
        Assert.AreEqual(1f - UpperOpacity, fades[2], Tolerance);
        Assert.AreEqual(1f, fades[3], Tolerance);
    }

    [Test]
    public void LowestFloor_HasNoFloorBelow()
    {
        var fades = Compute(currentFloor: 0);

        Assert.AreEqual(0f, fades[0], Tolerance);
        Assert.AreEqual(1f - UpperOpacity, fades[1], Tolerance);
        Assert.AreEqual(1f, fades[2], Tolerance);
        Assert.AreEqual(1f, fades[3], Tolerance);
    }

    [Test]
    public void HighestFloor_HasNoFloorAbove()
    {
        var fades = Compute(currentFloor: 3);

        Assert.AreEqual(1f, fades[0], Tolerance);
        Assert.AreEqual(1f, fades[1], Tolerance);
        Assert.AreEqual(1f - LowerOpacity, fades[2], Tolerance);
        Assert.AreEqual(0f, fades[3], Tolerance);
    }

    [Test]
    public void OpacitiesOutsideZeroToOne_AreClamped()
    {
        var fades = new float[FloorFadeRules.MaxFloors];
        FloorFadeRules.Compute(1, -2f, 5f, fades);

        Assert.AreEqual(1f, fades[0], Tolerance);
        Assert.AreEqual(0f, fades[2], Tolerance);
    }

    [Test]
    public void FewerFloorsThanMax_AreSupported()
    {
        var fades = Compute(currentFloor: 1, floorCount: 2);

        Assert.AreEqual(1f - LowerOpacity, fades[0], Tolerance);
        Assert.AreEqual(0f, fades[1], Tolerance);
    }

    [TestCase(-1)]
    [TestCase(4)]
    public void CurrentFloorOutOfRange_Throws(int currentFloor)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Compute(currentFloor));
    }

    [Test]
    public void MoreFloorsThanTheShaderSupports_Throws()
    {
        Assert.Throws<ArgumentException>(() => Compute(0, FloorFadeRules.MaxFloors + 1));
    }

    [Test]
    public void NullBuffer_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => FloorFadeRules.Compute(0, LowerOpacity, UpperOpacity, null));
    }
}
