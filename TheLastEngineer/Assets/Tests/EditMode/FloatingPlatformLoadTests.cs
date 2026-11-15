using NUnit.Framework;
using UnityEngine;

public class FloatingPlatformLoadTests
{
    private const float Tolerance = 1e-5f;
    private static readonly Vector3 BoxSize = new Vector3(4f, 0.3f, 4f);

    [Test]
    public void ClampToTopFace_PointInsideFootprint_KeepsXZAndSnapsToTop()
    {
        var result = FloatingPlatformLoad.ClampToTopFace(new Vector3(1f, 3f, -0.5f), Vector3.zero, BoxSize);

        AssertVector(new Vector3(1f, 0.15f, -0.5f), result);
    }

    [Test]
    public void ClampToTopFace_PointPastEdge_ClampsToEdge()
    {
        var result = FloatingPlatformLoad.ClampToTopFace(new Vector3(5f, 0f, -9f), new Vector3(0f, 1f, 0f), BoxSize);

        AssertVector(new Vector3(2f, 1.15f, -2f), result);
    }

    private static void AssertVector(Vector3 expected, Vector3 actual)
    {
        Assert.AreEqual(expected.x, actual.x, Tolerance);
        Assert.AreEqual(expected.y, actual.y, Tolerance);
        Assert.AreEqual(expected.z, actual.z, Tolerance);
    }

    [Test]
    public void StepLoad_Loaded_RisesByStepOverRampTime()
    {
        Assert.AreEqual(0.25f, FloatingPlatformLoad.StepLoad(0f, true, 0.2f, 0.05f), Tolerance);
    }

    [Test]
    public void StepLoad_Unloaded_FallsAndNeverGoesBelowZero()
    {
        Assert.AreEqual(0f, FloatingPlatformLoad.StepLoad(0.1f, false, 0.2f, 0.05f), Tolerance);
    }

    [Test]
    public void StepLoad_ZeroRampTime_JumpsToTarget()
    {
        Assert.AreEqual(1f, FloatingPlatformLoad.StepLoad(0f, true, 0f, 0.02f));
    }
}
