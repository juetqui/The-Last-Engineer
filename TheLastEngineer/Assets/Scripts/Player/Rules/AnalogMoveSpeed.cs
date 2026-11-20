using UnityEngine;

/// <summary>
/// Maps how far the move stick is tilted (0..1) to the fraction of the player's max move speed.
/// A digital input (keyboard) always arrives with tilt 1, so it keeps full speed.
/// </summary>
public static class AnalogMoveSpeed
{
    /// <summary>Below this tilt the input counts as no movement (already past the Input System deadzone).</summary>
    public const float NoInputTilt = 0.0001f;

    /// <param name="tilt">Stick magnitude; clamped to 0..1.</param>
    /// <param name="speedByTilt">Tilt → speed fraction. Null falls back to linear.</param>
    /// <param name="minFraction">Lowest fraction while there is any input; clamped to 0..1.</param>
    /// <returns>0 with no input, otherwise a fraction in [minFraction, 1].</returns>
    public static float Evaluate(float tilt, AnimationCurve speedByTilt, float minFraction)
    {
        tilt = Mathf.Clamp01(tilt);
        if (tilt < NoInputTilt) return 0f;

        float fraction = speedByTilt != null && speedByTilt.length > 0 ? speedByTilt.Evaluate(tilt) : tilt;
        return Mathf.Clamp(fraction, Mathf.Clamp01(minFraction), 1f);
    }
}
