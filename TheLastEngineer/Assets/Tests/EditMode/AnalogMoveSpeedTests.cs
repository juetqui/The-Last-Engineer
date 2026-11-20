using NUnit.Framework;
using UnityEngine;

public class AnalogMoveSpeedTests
{
    private const float Tolerance = 0.0001f;
    private PlayerDataValues _data;

    [SetUp]
    public void LoadPlayerData() => _data = PlayerDataValues.Load();

    private float Evaluate(float tilt) => AnalogMoveSpeed.Evaluate(tilt, _data.MoveSpeedByTilt, _data.MinMoveSpeedFraction);

    [Test]
    public void Evaluate_NoTilt_IsZero()
    {
        Assert.AreEqual(0f, Evaluate(0f), Tolerance);
    }

    [Test]
    public void Evaluate_TinyTilt_ReturnsTheMinimum()
    {
        Assert.AreEqual(_data.MinMoveSpeedFraction, Evaluate(0.01f), Tolerance);
    }

    [Test]
    public void Evaluate_HalfTilt_FollowsTheCurve()
    {
        float expected = Mathf.Max(_data.MinMoveSpeedFraction, _data.MoveSpeedByTilt.Evaluate(0.5f));

        Assert.AreEqual(expected, Evaluate(0.5f), Tolerance);
    }

    [Test]
    public void Evaluate_FullTilt_IsFullSpeed()
    {
        Assert.AreEqual(1f, Evaluate(1f), Tolerance);
    }

    [Test]
    public void Evaluate_TiltAboveOne_IsClampedToFullSpeed()
    {
        Assert.AreEqual(1f, Evaluate(1.41f), Tolerance);
    }

    [Test]
    public void Evaluate_NegativeTilt_IsZero()
    {
        Assert.AreEqual(0f, Evaluate(-1f), Tolerance);
    }

    [Test]
    public void Evaluate_GrowsWithTilt()
    {
        float previous = 0f;
        for (float tilt = 0.05f; tilt <= 1f; tilt += 0.05f)
        {
            float fraction = Evaluate(tilt);
            Assert.GreaterOrEqual(fraction, previous, $"tilt {tilt}");
            previous = fraction;
        }
    }

    [Test]
    public void Evaluate_CurveAboveOne_IsClampedToFullSpeed()
    {
        var overshoot = AnimationCurve.Constant(0f, 1f, 2f);

        Assert.AreEqual(1f, AnalogMoveSpeed.Evaluate(0.5f, overshoot, 0f), Tolerance);
    }

    [Test]
    public void Evaluate_NullCurve_FallsBackToLinear()
    {
        Assert.AreEqual(0.6f, AnalogMoveSpeed.Evaluate(0.6f, null, 0f), Tolerance);
    }
}
