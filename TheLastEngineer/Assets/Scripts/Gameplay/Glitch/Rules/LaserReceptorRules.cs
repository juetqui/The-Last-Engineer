/// <summary>
/// Pure rules of which lasers charge a LaserReceptor. No Unity types so EditMode tests can reach
/// them; LaserReceptor applies what these allow.
/// </summary>
public static class LaserReceptorRules
{
    /// <summary>A receptor can only require Clean or Intangible lasers.</summary>
    public static bool IsValidRequiredState(GlitchState required)
    {
        return required == GlitchState.Clean || required == GlitchState.Intangible;
    }

    /// <summary>An invalid required state (data the Editor dropdown could not stop) falls back to Clean.</summary>
    public static GlitchState SanitizeRequiredState(GlitchState required)
    {
        return IsValidRequiredState(required) ? required : GlitchState.Clean;
    }

    /// <summary>
    /// A laser charges the receptor only when its emitter is in exactly the required state.
    /// A laser with no glitch holder is reported as Clean by its controller.
    /// </summary>
    public static bool AcceptsEmitter(GlitchState required, GlitchState emitter)
    {
        return emitter == required;
    }
}
