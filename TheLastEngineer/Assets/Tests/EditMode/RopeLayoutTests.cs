using NUnit.Framework;
using UnityEngine;

public class RopeLayoutTests
{
    private const float Tolerance = 1e-4f;

    [Test]
    public void SegmentCount_LengthNotMultiple_RoundsUp()
    {
        Assert.AreEqual(11, RopeLayout.SegmentCount(4.1f, 0.4f, 48));
    }

    [Test]
    public void SegmentCount_ExceedsMax_ClampsToMax()
    {
        Assert.AreEqual(10, RopeLayout.SegmentCount(100f, 0.4f, 10));
    }

    [Test]
    public void SagDepth_NoSlack_IsZero()
    {
        Assert.AreEqual(0f, RopeLayout.SagDepth(5f, 5f));
    }

    [Test]
    public void SagDepth_WithSlack_MatchesShallowParabolaLength()
    {
        const float distance = 5f;
        const float ropeLength = 5.4f;

        float sag = RopeLayout.SagDepth(distance, ropeLength);

        Assert.AreEqual(ropeLength, distance + 8f * sag * sag / (3f * distance), Tolerance);
    }

    [Test]
    public void FillPoints_HorizontalSpan_EndsMatchAndMiddleSagsDown()
    {
        var points = new Vector3[5];

        RopeLayout.FillPoints(Vector3.zero, new Vector3(4f, 0f, 0f), 1f, Vector3.down, points);

        AssertVector(Vector3.zero, points[0]);
        AssertVector(new Vector3(4f, 0f, 0f), points[4]);
        AssertVector(new Vector3(2f, -1f, 0f), points[2]);
    }

    [Test]
    public void SagDirection_VerticalSpan_ReturnsUnitPerpendicular()
    {
        Vector3 direction = RopeLayout.SagDirection(Vector3.up * 3f, Vector3.down);

        Assert.AreEqual(1f, direction.magnitude, Tolerance);
        Assert.AreEqual(0f, Vector3.Dot(direction, Vector3.up), Tolerance);
    }

    private static void AssertVector(Vector3 expected, Vector3 actual)
    {
        Assert.AreEqual(expected.x, actual.x, Tolerance);
        Assert.AreEqual(expected.y, actual.y, Tolerance);
        Assert.AreEqual(expected.z, actual.z, Tolerance);
    }
}
