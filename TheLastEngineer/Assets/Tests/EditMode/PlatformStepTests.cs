using NUnit.Framework;
using UnityEngine;

public class PlatformStepTests
{
    private const float Tolerance = 1e-5f;

    [Test]
    public void Advance_BudgetShorterThanDistance_MovesExactlyTheBudget()
    {
        var result = PlatformStep.Advance(Vector3.zero, new Vector3(10f, 0f, 0f), 2f);

        Assert.IsFalse(result.Arrived);
        Assert.AreEqual(2f, result.Delta.magnitude, Tolerance);
        Assert.AreEqual(new Vector3(2f, 0f, 0f), result.Position);
        Assert.AreEqual(0f, result.Leftover);
    }

    [Test]
    public void Advance_BudgetReachesTarget_LandsExactlyOnTargetWithLeftover()
    {
        var start = new Vector3(0.3f, 1.7f, -2.1f);
        var target = new Vector3(1f, 2f, -2f);

        var result = PlatformStep.Advance(start, target, 5f);

        Assert.IsTrue(result.Arrived);
        Assert.AreEqual(target, result.Position);
        Assert.AreEqual(target - start, result.Delta);
        Assert.AreEqual(5f - Vector3.Distance(start, target), result.Leftover, Tolerance);
    }

    [Test]
    public void Advance_AlreadyOnTarget_ArrivesWithoutMoving()
    {
        var target = new Vector3(4f, 0f, 1f);

        var result = PlatformStep.Advance(target, target, 0f);

        Assert.IsTrue(result.Arrived);
        Assert.AreEqual(Vector3.zero, result.Delta);
    }

    [Test]
    public void Advance_NegativeBudget_DoesNotMove()
    {
        var result = PlatformStep.Advance(Vector3.zero, Vector3.right, -1f);

        Assert.IsFalse(result.Arrived);
        Assert.AreEqual(Vector3.zero, result.Delta);
    }

    [Test]
    public void StepDistance_FarFromTarget_IsSpeedTimesDeltaTime()
    {
        float step = PlatformStep.StepDistance(2f, 0.5f, 10f);

        Assert.AreEqual(1f, step, Tolerance);
    }

    [Test]
    public void StepDistance_FarFromTargetAtZeroSpeed_IsZero()
    {
        Assert.AreEqual(0f, PlatformStep.StepDistance(0f, 0.1f, 10f));
    }

    [Test]
    public void StepDistance_InsideApproachRadiusAtZeroSpeed_CrawlsAtMinApproachSpeed()
    {
        float step = PlatformStep.StepDistance(0f, 0.1f, PlatformStep.ApproachRadius);

        Assert.AreEqual(PlatformStep.MinApproachSpeed * 0.1f, step, Tolerance);
    }

    [Test]
    public void StepDistance_InsideApproachRadiusFasterThanMin_KeepsOwnSpeed()
    {
        float step = PlatformStep.StepDistance(3f, 0.1f, 0.05f);

        Assert.AreEqual(0.3f, step, Tolerance);
    }
}
