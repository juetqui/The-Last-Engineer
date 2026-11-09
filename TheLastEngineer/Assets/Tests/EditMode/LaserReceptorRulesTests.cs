using NUnit.Framework;

public class LaserReceptorRulesTests
{
    [TestCase(GlitchState.Clean)]
    [TestCase(GlitchState.Intangible)]
    public void IsValidRequiredState_CleanOrIntangible_True(GlitchState required)
    {
        Assert.IsTrue(LaserReceptorRules.IsValidRequiredState(required));
    }

    [Test]
    public void IsValidRequiredState_Glitched_False()
    {
        Assert.IsFalse(LaserReceptorRules.IsValidRequiredState(GlitchState.Glitched));
    }

    [TestCase(GlitchState.Clean)]
    [TestCase(GlitchState.Intangible)]
    public void SanitizeRequiredState_ValidState_Unchanged(GlitchState required)
    {
        Assert.AreEqual(required, LaserReceptorRules.SanitizeRequiredState(required));
    }

    [Test]
    public void SanitizeRequiredState_Glitched_ReturnsClean()
    {
        Assert.AreEqual(GlitchState.Clean, LaserReceptorRules.SanitizeRequiredState(GlitchState.Glitched));
    }

    [TestCase(GlitchState.Clean)]
    [TestCase(GlitchState.Intangible)]
    public void AcceptsEmitter_SameState_True(GlitchState state)
    {
        Assert.IsTrue(LaserReceptorRules.AcceptsEmitter(state, state));
    }

    [TestCase(GlitchState.Clean, GlitchState.Intangible)]
    [TestCase(GlitchState.Intangible, GlitchState.Clean)]
    [TestCase(GlitchState.Clean, GlitchState.Glitched)]
    [TestCase(GlitchState.Intangible, GlitchState.Glitched)]
    public void AcceptsEmitter_DifferentState_False(GlitchState required, GlitchState emitter)
    {
        Assert.IsFalse(LaserReceptorRules.AcceptsEmitter(required, emitter));
    }
}
