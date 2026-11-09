using NUnit.Framework;

public class GlitchSwapRulesTests
{
    [Test]
    public void EvaluateSet_GlitchedNodeToCleanGlitcheable_Swapped()
    {
        var result = GlitchSwapRules.EvaluateSet(GlitchState.Glitched, GlitchContainerKind.Glitcheable, GlitchState.Clean);
        Assert.AreEqual(GlitchTransferResult.Swapped, result);
    }

    [Test]
    public void EvaluateSet_IntangibleNodeToCleanIntangible_Swapped()
    {
        var result = GlitchSwapRules.EvaluateSet(GlitchState.Intangible, GlitchContainerKind.Intangible, GlitchState.Clean);
        Assert.AreEqual(GlitchTransferResult.Swapped, result);
    }

    [Test]
    public void EvaluateSet_IntangibleNodeToGlitcheable_Incompatible()
    {
        var result = GlitchSwapRules.EvaluateSet(GlitchState.Intangible, GlitchContainerKind.Glitcheable, GlitchState.Clean);
        Assert.AreEqual(GlitchTransferResult.Incompatible, result);
    }

    [Test]
    public void EvaluateSet_GlitchedNodeToIntangible_Incompatible()
    {
        var result = GlitchSwapRules.EvaluateSet(GlitchState.Glitched, GlitchContainerKind.Intangible, GlitchState.Clean);
        Assert.AreEqual(GlitchTransferResult.Incompatible, result);
    }

    [Test]
    public void EvaluateSet_CleanNode_NodeClean()
    {
        var result = GlitchSwapRules.EvaluateSet(GlitchState.Clean, GlitchContainerKind.Glitcheable, GlitchState.Clean);
        Assert.AreEqual(GlitchTransferResult.NodeClean, result);
    }

    [Test]
    public void EvaluateSet_TargetNotClean_TargetNotClean()
    {
        var result = GlitchSwapRules.EvaluateSet(GlitchState.Glitched, GlitchContainerKind.Glitcheable, GlitchState.Glitched);
        Assert.AreEqual(GlitchTransferResult.TargetNotClean, result);
    }

    [Test]
    public void EvaluateTake_CleanNodeFromGlitched_Swapped()
    {
        Assert.AreEqual(GlitchTransferResult.Swapped, GlitchSwapRules.EvaluateTake(GlitchState.Clean, GlitchState.Glitched));
    }

    [Test]
    public void EvaluateTake_CleanNodeFromIntangible_Swapped()
    {
        Assert.AreEqual(GlitchTransferResult.Swapped, GlitchSwapRules.EvaluateTake(GlitchState.Clean, GlitchState.Intangible));
    }

    [Test]
    public void EvaluateTake_NodeNotClean_NodeNotClean()
    {
        Assert.AreEqual(GlitchTransferResult.NodeNotClean, GlitchSwapRules.EvaluateTake(GlitchState.Intangible, GlitchState.Glitched));
    }

    [Test]
    public void EvaluateTake_TargetClean_TargetClean()
    {
        Assert.AreEqual(GlitchTransferResult.TargetClean, GlitchSwapRules.EvaluateTake(GlitchState.Clean, GlitchState.Clean));
    }

    [Test]
    public void IsValidState_GlitcheableIntangible_False()
    {
        Assert.IsFalse(GlitchSwapRules.IsValidState(GlitchContainerKind.Glitcheable, GlitchState.Intangible));
    }

    [Test]
    public void IsValidState_IntangibleGlitched_False()
    {
        Assert.IsFalse(GlitchSwapRules.IsValidState(GlitchContainerKind.Intangible, GlitchState.Glitched));
    }

    [TestCase(GlitchState.Clean)]
    [TestCase(GlitchState.Intangible)]
    [TestCase(GlitchState.Glitched)]
    public void IsValidState_NodeAnyState_True(GlitchState state)
    {
        Assert.IsTrue(GlitchSwapRules.IsValidState(GlitchContainerKind.Node, state));
    }

    [TestCase(GlitchContainerKind.Glitcheable, GlitchState.Clean)]
    [TestCase(GlitchContainerKind.Glitcheable, GlitchState.Glitched)]
    [TestCase(GlitchContainerKind.Intangible, GlitchState.Clean)]
    [TestCase(GlitchContainerKind.Intangible, GlitchState.Intangible)]
    public void IsValidState_ObjectOwnStates_True(GlitchContainerKind kind, GlitchState state)
    {
        Assert.IsTrue(GlitchSwapRules.IsValidState(kind, state));
    }
}
