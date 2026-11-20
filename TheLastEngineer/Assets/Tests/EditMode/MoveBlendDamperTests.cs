using NUnit.Framework;

public class MoveBlendDamperTests
{
    private const float WalkBlend = 0.5f;
    private const float RunBlend = 1f;
    private const float IdleBlend = 0f;
    // Generous bound: an exponential approach covers 0.5 → 0.001 in about 6.2 time constants.
    private const float SettleTimeConstants = 10f;
    private const float HoldSeconds = 1f;

    private PlayerDataValues _data;

    [SetUp]
    public void LoadPlayerData() => _data = PlayerDataValues.Load();

    private float SettleBudget => _data.MoveAnimBlendDampTime * SettleTimeConstants;

    /// <summary>Steps at a fixed frame rate for the given seconds, from the given blend towards the target.</summary>
    private float Run(float from, float target, float seconds, float fps)
    {
        float dt = 1f / fps;
        float blend = from;
        for (float t = 0f; t < seconds; t += dt)
            blend = MoveBlendDamper.Step(blend, target, _data.MoveAnimBlendDampTime, dt);
        return blend;
    }

    [TestCase(30f)]
    [TestCase(60f)]
    [TestCase(144f)]
    public void ReleasingFromWalk_ReachesIdleExactly(float fps)
    {
        Assert.AreEqual(IdleBlend, Run(WalkBlend, IdleBlend, SettleBudget, fps));
    }

    [TestCase(30f)]
    [TestCase(60f)]
    [TestCase(144f)]
    public void ReleasingFromRun_ReachesIdleExactly(float fps)
    {
        Assert.AreEqual(IdleBlend, Run(RunBlend, IdleBlend, SettleBudget, fps));
    }

    [Test]
    public void AtIdle_StaysExactlyAtIdle()
    {
        Assert.AreEqual(IdleBlend, Run(IdleBlend, IdleBlend, HoldSeconds, 60f));
    }

    [Test]
    public void PressingFromIdle_ReachesWalkExactly()
    {
        Assert.AreEqual(WalkBlend, Run(IdleBlend, WalkBlend, SettleBudget, 60f));
    }

    [Test]
    public void FirstFrames_MoveTowardsTheTargetWithoutJumping()
    {
        float next = MoveBlendDamper.Step(WalkBlend, IdleBlend, _data.MoveAnimBlendDampTime, 1f / 60f);

        Assert.Less(next, WalkBlend);
        Assert.Greater(next, IdleBlend);
    }

    [Test]
    public void ZeroDampTime_JumpsToTheTarget()
    {
        Assert.AreEqual(IdleBlend, MoveBlendDamper.Step(WalkBlend, IdleBlend, 0f, 1f / 60f));
    }

    /// <summary>
    /// The reported bug: after the first time the stick was pushed, releasing it left the blend just
    /// above 0, so the Walk clip kept a weight and kept firing its footstep events while standing still.
    /// </summary>
    [TestCase(0.5f)]
    [TestCase(1f)]
    [TestCase(0.05f)]
    public void ReleasingTheStick_AfterMoving_SettlesOnIdle(float tilt)
    {
        const float fps = 60f;
        float dt = 1f / fps;
        float blend = IdleBlend;

        for (float t = 0f; t < HoldSeconds; t += dt)
        {
            float fraction = AnalogMoveSpeed.Evaluate(tilt, _data.MoveSpeedByTilt, _data.MinMoveSpeedFraction);
            blend = MoveBlendDamper.Step(blend, fraction, _data.MoveAnimBlendDampTime, dt);
        }
        Assert.Greater(blend, IdleBlend, "the blend should have left Idle while moving");

        for (float t = 0f; t < SettleBudget; t += dt)
        {
            float fraction = AnalogMoveSpeed.Evaluate(0f, _data.MoveSpeedByTilt, _data.MinMoveSpeedFraction);
            blend = MoveBlendDamper.Step(blend, fraction, _data.MoveAnimBlendDampTime, dt);
        }

        Assert.AreEqual(IdleBlend, blend);
    }
}
