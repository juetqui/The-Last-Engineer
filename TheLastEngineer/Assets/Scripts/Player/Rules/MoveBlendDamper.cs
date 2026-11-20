using UnityEngine;

/// <summary>
/// Smooths the Idle/Walk/Run blend parameter towards its target and lands on it exactly.
/// Animator.SetFloat's own damping only approaches the target asymptotically, so after releasing
/// the stick the Walk clip kept a tiny non-zero weight and kept firing its footstep events.
/// </summary>
public static class MoveBlendDamper
{
    /// <summary>Closer than this to the target, the value snaps onto it.</summary>
    public const float SnapDistance = 0.001f;

    /// <param name="current">Blend value last frame.</param>
    /// <param name="target">Blend value wanted (the speed fraction, 0..1).</param>
    /// <param name="dampTime">Time constant in seconds; 0 or less jumps straight to the target.</param>
    /// <param name="deltaTime">Frame time in seconds.</param>
    public static float Step(float current, float target, float dampTime, float deltaTime)
    {
        if (dampTime <= 0f) return target;

        // Frame-rate independent exponential approach.
        float t = 1f - Mathf.Exp(-Mathf.Max(0f, deltaTime) / dampTime);
        float next = Mathf.Lerp(current, target, t);

        return Mathf.Abs(target - next) < SnapDistance ? target : next;
    }
}
