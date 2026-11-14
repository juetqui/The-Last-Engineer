using NUnit.Framework;

public class PlatformHumRulesTests
{
    private const float Tolerance = 1e-5f;
    private const float FullVolume = 0.6f;
    private const float FullDuration = 1.2f;

    [Test]
    public void FullSequence_PowerOnThenStopAndRest_FadesInHoldsAndFadesOutToStop()
    {
        HumState state = HumState.Silent;

        state = PlatformHumRules.Next(state, HumSignal.PoweredOn, out var command);
        Assert.AreEqual(HumState.FadingIn, state);
        Assert.AreEqual(HumCommand.PlayAndFadeIn, command);

        state = PlatformHumRules.Next(state, HumSignal.FadeCompleted, out command);
        Assert.AreEqual(HumState.On, state);
        Assert.AreEqual(HumCommand.None, command);

        // Disconnecting while moving only enters ToStop: no signal reaches the hum until the platform rests.
        state = PlatformHumRules.Next(state, HumSignal.BecameInactive, out command);
        Assert.AreEqual(HumState.FadingOut, state);
        Assert.AreEqual(HumCommand.FadeOut, command);

        state = PlatformHumRules.Next(state, HumSignal.FadeCompleted, out command);
        Assert.AreEqual(HumState.Silent, state);
        Assert.AreEqual(HumCommand.Stop, command);
    }

    [Test]
    public void FadingOut_PoweredOnAgain_FadesBackInWithoutRestartingTheClip()
    {
        var state = PlatformHumRules.Next(HumState.FadingOut, HumSignal.PoweredOn, out var command);

        Assert.AreEqual(HumState.FadingIn, state);
        Assert.AreEqual(HumCommand.FadeIn, command);
    }

    [TestCase(HumState.On)]
    [TestCase(HumState.FadingIn)]
    public void AlreadyPowered_PoweredOnAgain_DoesNothing(HumState from)
    {
        var state = PlatformHumRules.Next(from, HumSignal.PoweredOn, out var command);

        Assert.AreEqual(from, state);
        Assert.AreEqual(HumCommand.None, command);
    }

    [Test]
    public void FadingIn_BecameInactive_FadesOutFromWhereItIs()
    {
        var state = PlatformHumRules.Next(HumState.FadingIn, HumSignal.BecameInactive, out var command);

        Assert.AreEqual(HumState.FadingOut, state);
        Assert.AreEqual(HumCommand.FadeOut, command);
    }

    [TestCase(HumSignal.BecameInactive)]
    [TestCase(HumSignal.FadeCompleted)]
    public void Silent_SignalOtherThanPoweredOn_StaysSilent(HumSignal signal)
    {
        var state = PlatformHumRules.Next(HumState.Silent, signal, out var command);

        Assert.AreEqual(HumState.Silent, state);
        Assert.AreEqual(HumCommand.None, command);
    }

    [Test]
    public void FadingOut_BecameInactiveAgain_KeepsFading()
    {
        var state = PlatformHumRules.Next(HumState.FadingOut, HumSignal.BecameInactive, out var command);

        Assert.AreEqual(HumState.FadingOut, state);
        Assert.AreEqual(HumCommand.None, command);
    }

    [Test]
    public void ScaledDuration_FromSilenceToFullVolume_IsTheFullDuration()
    {
        Assert.AreEqual(FullDuration, PlatformHumRules.ScaledDuration(0f, FullVolume, FullVolume, FullDuration), Tolerance);
    }

    [Test]
    public void ScaledDuration_FromHalfVolume_IsHalfTheDuration()
    {
        float half = FullVolume * 0.5f;

        Assert.AreEqual(FullDuration * 0.5f, PlatformHumRules.ScaledDuration(half, FullVolume, FullVolume, FullDuration), Tolerance);
        Assert.AreEqual(FullDuration * 0.5f, PlatformHumRules.ScaledDuration(half, 0f, FullVolume, FullDuration), Tolerance);
    }

    [Test]
    public void ScaledDuration_AlreadyAtTarget_IsZero()
    {
        Assert.AreEqual(0f, PlatformHumRules.ScaledDuration(FullVolume, FullVolume, FullVolume, FullDuration), Tolerance);
    }

    [Test]
    public void ScaledDuration_DistanceBeyondFullVolume_IsClampedToTheFullDuration()
    {
        Assert.AreEqual(FullDuration, PlatformHumRules.ScaledDuration(1f, 0f, FullVolume, FullDuration), Tolerance);
    }

    [Test]
    public void LoadedPitch_Endpoints_ReturnEmptyAndLoadedPitch()
    {
        Assert.AreEqual(1f, PlatformHumRules.LoadedPitch(1f, 0.9f, 0f), Tolerance);
        Assert.AreEqual(0.9f, PlatformHumRules.LoadedPitch(1f, 0.9f, 1f), Tolerance);
    }

    [Test]
    public void LoadedPitch_LoadOutOfRange_IsClamped()
    {
        Assert.AreEqual(0.9f, PlatformHumRules.LoadedPitch(1f, 0.9f, 2f), Tolerance);
        Assert.AreEqual(1f, PlatformHumRules.LoadedPitch(1f, 0.9f, -1f), Tolerance);
    }

    [Test]
    public void LoadedCutoff_HalfLoad_IsTheGeometricMean()
    {
        // 22000 Hz and 2200 Hz: half-way on a log scale is sqrt(22000 * 2200) = 6957 Hz, not the linear 12100 Hz.
        float expected = UnityEngine.Mathf.Sqrt(22000f * 2200f);

        Assert.AreEqual(expected, PlatformHumRules.LoadedCutoff(22000f, 2200f, 0.5f), 0.01f);
    }

    [Test]
    public void LoadedCutoff_Endpoints_ReturnEmptyAndLoadedCutoff()
    {
        Assert.AreEqual(22000f, PlatformHumRules.LoadedCutoff(22000f, 2200f, 0f), 0.01f);
        Assert.AreEqual(2200f, PlatformHumRules.LoadedCutoff(22000f, 2200f, 1f), 0.01f);
    }

    [Test]
    public void LoadedCutoff_NonPositiveCutoff_FallsBackToLinearWithoutNaN()
    {
        float cutoff = PlatformHumRules.LoadedCutoff(0f, 2200f, 0.5f);

        Assert.IsFalse(float.IsNaN(cutoff));
        Assert.AreEqual(1100f, cutoff, 0.01f);
    }

    [TestCase(0f, FullDuration)]
    [TestCase(-1f, FullDuration)]
    [TestCase(FullVolume, 0f)]
    public void ScaledDuration_NonPositiveRange_IsZero(float fullVolume, float fullDuration)
    {
        Assert.AreEqual(0f, PlatformHumRules.ScaledDuration(0f, FullVolume, fullVolume, fullDuration));
    }
}
