using UnityEngine;

/// <summary>Where the platform hum loop is (see <see cref="PlatformHumRules"/>).</summary>
public enum HumState
{
    Silent,
    FadingIn,
    On,
    FadingOut
}

/// <summary>What the platform hum loop reacts to.</summary>
public enum HumSignal
{
    /// <summary>A correct connection powered the platform on.</summary>
    PoweredOn,
    /// <summary>The platform came to rest unpowered (entered its Inactive state).</summary>
    BecameInactive,
    /// <summary>The fade in progress reached its target volume.</summary>
    FadeCompleted
}

/// <summary>What the audio side must do after a <see cref="HumSignal"/>.</summary>
public enum HumCommand
{
    None,
    /// <summary>Start the loop from silence and fade up to the loop volume.</summary>
    PlayAndFadeIn,
    /// <summary>Fade up to the loop volume from the current volume, without restarting the clip.</summary>
    FadeIn,
    /// <summary>Fade down to silence from the current volume.</summary>
    FadeOut,
    /// <summary>Stop the audio source.</summary>
    Stop
}

/// <summary>
/// Pure rules of the platform hum loop: which command each signal triggers in each state, and how long a
/// fade lasts when it starts part-way. Kept out of the MonoBehaviour so EditMode tests can cover it.
/// </summary>
public static class PlatformHumRules
{
    /// <summary>Returns the next state for <paramref name="signal"/> and the command the audio side must run.</summary>
    public static HumState Next(HumState state, HumSignal signal, out HumCommand command)
    {
        command = HumCommand.None;

        switch (state)
        {
            case HumState.Silent:
                if (signal == HumSignal.PoweredOn)
                {
                    command = HumCommand.PlayAndFadeIn;
                    return HumState.FadingIn;
                }
                return state;

            case HumState.FadingIn:
            case HumState.On:
                if (signal == HumSignal.BecameInactive)
                {
                    command = HumCommand.FadeOut;
                    return HumState.FadingOut;
                }
                if (signal == HumSignal.FadeCompleted)
                    return HumState.On;
                return state;

            case HumState.FadingOut:
                if (signal == HumSignal.PoweredOn)
                {
                    command = HumCommand.FadeIn;
                    return HumState.FadingIn;
                }
                if (signal == HumSignal.FadeCompleted)
                {
                    command = HumCommand.Stop;
                    return HumState.Silent;
                }
                return state;

            default:
                return state;
        }
    }

    /// <summary>
    /// Duration of a fade from <paramref name="from"/> to <paramref name="to"/>, scaled by how much of the full
    /// 0..<paramref name="fullVolume"/> range it covers, so a fade that starts part-way keeps the same rate.
    /// </summary>
    public static float ScaledDuration(float from, float to, float fullVolume, float fullDuration)
    {
        if (fullVolume <= 0f || fullDuration <= 0f) return 0f;

        float fraction = Mathf.Clamp01(Mathf.Abs(to - from) / fullVolume);
        return fullDuration * fraction;
    }

    /// <summary>Pitch for a load amount (0 = empty, 1 = loaded), linear between the two pitches.</summary>
    public static float LoadedPitch(float emptyPitch, float loadedPitch, float load)
    {
        return Mathf.Lerp(emptyPitch, loadedPitch, Mathf.Clamp01(load));
    }

    /// <summary>
    /// Low-pass cutoff for a load amount, interpolated on a log scale so the sweep sounds even
    /// (hearing is logarithmic in frequency). Falls back to linear if a cutoff isn't positive.
    /// </summary>
    public static float LoadedCutoff(float emptyCutoff, float loadedCutoff, float load)
    {
        float t = Mathf.Clamp01(load);

        if (emptyCutoff <= 0f || loadedCutoff <= 0f)
            return Mathf.Max(0f, Mathf.Lerp(emptyCutoff, loadedCutoff, t));

        return emptyCutoff * Mathf.Pow(loadedCutoff / emptyCutoff, t);
    }
}
