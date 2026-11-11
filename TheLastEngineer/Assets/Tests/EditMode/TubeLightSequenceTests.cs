using System;
using NUnit.Framework;

public class TubeLightSequenceTests
{
    private const int ThreeTubes = 3;
    private const int LastOfThree = ThreeTubes - 1;

    [Test]
    public void Constructor_NegativeCount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TubeLightSequence(-1));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Start_EmptyList_ReturnsNoTube(bool fill)
    {
        var sequence = new TubeLightSequence(0);

        Assert.AreEqual(TubeLightSequence.NoTube, sequence.Start(fill));
        Assert.IsFalse(sequence.IsRunning);
    }

    [Test]
    public void Start_FillFromIdle_ReturnsFirstTube()
    {
        var sequence = new TubeLightSequence(ThreeTubes);

        Assert.AreEqual(0, sequence.Start(fill: true));
        Assert.IsTrue(sequence.IsRunning);
        Assert.IsTrue(sequence.Fill);
    }

    [Test]
    public void Start_DrainFromIdle_ReturnsLastTube()
    {
        var sequence = new TubeLightSequence(ThreeTubes);

        Assert.AreEqual(LastOfThree, sequence.Start(fill: false));
        Assert.IsFalse(sequence.Fill);
    }

    [Test]
    public void Advance_Fill_GoesForwardThenFinishes()
    {
        var sequence = new TubeLightSequence(ThreeTubes);
        sequence.Start(fill: true);

        Assert.AreEqual(1, sequence.Advance());
        Assert.AreEqual(2, sequence.Advance());
        Assert.AreEqual(TubeLightSequence.NoTube, sequence.Advance());
        Assert.IsFalse(sequence.IsRunning);
    }

    [Test]
    public void Advance_Drain_GoesBackwardThenFinishes()
    {
        var sequence = new TubeLightSequence(ThreeTubes);
        sequence.Start(fill: false);

        Assert.AreEqual(1, sequence.Advance());
        Assert.AreEqual(0, sequence.Advance());
        Assert.AreEqual(TubeLightSequence.NoTube, sequence.Advance());
        Assert.IsFalse(sequence.IsRunning);
    }

    [Test]
    public void Advance_WhenIdle_ReturnsNoTube()
    {
        var sequence = new TubeLightSequence(ThreeTubes);

        Assert.AreEqual(TubeLightSequence.NoTube, sequence.Advance());
        Assert.IsFalse(sequence.IsRunning);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Start_SingleTube_UsesIndexZeroAndFinishesOnAdvance(bool fill)
    {
        var sequence = new TubeLightSequence(1);

        Assert.AreEqual(0, sequence.Start(fill));
        Assert.AreEqual(TubeLightSequence.NoTube, sequence.Advance());
    }

    [Test]
    public void Start_InterruptFillWithDrain_KeepsCurrentTubeAndGoesBackward()
    {
        var sequence = new TubeLightSequence(ThreeTubes);
        sequence.Start(fill: true);
        sequence.Advance(); // tube 1 in transition

        Assert.AreEqual(1, sequence.Start(fill: false));
        Assert.AreEqual(0, sequence.Advance());
        Assert.AreEqual(TubeLightSequence.NoTube, sequence.Advance());
    }

    [Test]
    public void Start_InterruptDrainWithFill_KeepsCurrentTubeAndGoesForward()
    {
        var sequence = new TubeLightSequence(ThreeTubes);
        sequence.Start(fill: false);
        sequence.Advance(); // tube 1 in transition

        Assert.AreEqual(1, sequence.Start(fill: true));
        Assert.AreEqual(2, sequence.Advance());
        Assert.AreEqual(TubeLightSequence.NoTube, sequence.Advance());
    }

    [Test]
    public void Start_SameDirectionWhileRunning_KeepsCurrentTube()
    {
        var sequence = new TubeLightSequence(ThreeTubes);
        sequence.Start(fill: true);
        sequence.Advance();

        Assert.AreEqual(1, sequence.Start(fill: true));
        Assert.AreEqual(2, sequence.Advance());
    }

    [Test]
    public void Start_TwoInterruptionsInARow_FollowsLastDirection()
    {
        var sequence = new TubeLightSequence(ThreeTubes);
        sequence.Start(fill: true);
        sequence.Advance(); // tube 1

        sequence.Start(fill: false);
        Assert.AreEqual(1, sequence.Start(fill: true));
        Assert.IsTrue(sequence.Fill);
        Assert.AreEqual(2, sequence.Advance());
    }

    [Test]
    public void FullCycle_FillThenDrain_StartFromOppositeEnds()
    {
        var sequence = new TubeLightSequence(ThreeTubes);

        Assert.AreEqual(0, sequence.Start(fill: true));
        RunToEnd(sequence);
        Assert.AreEqual(LastOfThree, sequence.Start(fill: false));
        RunToEnd(sequence);
        Assert.AreEqual(0, sequence.Start(fill: true));
    }

    // Regression: an interrupted fill used to leave the next fills starting from the far end.
    [Test]
    public void InterruptedFillThenDrainCompletes_NextFillStartsAtFirstTube()
    {
        var sequence = new TubeLightSequence(ThreeTubes);
        sequence.Start(fill: true);
        sequence.Advance();

        sequence.Start(fill: false);
        RunToEnd(sequence);

        Assert.AreEqual(0, sequence.Start(fill: true));
    }

    [Test]
    public void Stop_WhileRunning_NextStartBeginsAtEnd()
    {
        var sequence = new TubeLightSequence(ThreeTubes);
        sequence.Start(fill: true);
        sequence.Advance();

        sequence.Stop();

        Assert.IsFalse(sequence.IsRunning);
        Assert.AreEqual(LastOfThree, sequence.Start(fill: false));
    }

    private static void RunToEnd(TubeLightSequence sequence)
    {
        while (sequence.Advance() != TubeLightSequence.NoTube) { }
    }
}
