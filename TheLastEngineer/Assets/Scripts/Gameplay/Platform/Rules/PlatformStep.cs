using UnityEngine;

/// <summary>
/// Pure math of a moving platform's step toward its current route point. No scene state, so EditMode
/// tests can reach it; PlatformMotor applies the result to the Rigidbody and the passenger.
/// </summary>
public static class PlatformStep
{
    /// <summary>Within this distance of the target the platform never goes slower than <see cref="MinApproachSpeed"/>.</summary>
    public const float ApproachRadius = 0.15f;

    /// <summary>
    /// Units per second. The speed tweens can reach 0 a little short of the target; this crawl lets
    /// the platform land exactly on it instead of stopping short or snapping a visible gap in one frame.
    /// </summary>
    public const float MinApproachSpeed = 0.5f;

    /// <summary>Distance the platform may travel this frame.</summary>
    public static float StepDistance(float speed, float deltaTime, float distanceToTarget)
    {
        float effectiveSpeed = Mathf.Max(0f, speed);

        if (distanceToTarget <= ApproachRadius)
            effectiveSpeed = Mathf.Max(effectiveSpeed, MinApproachSpeed);

        return effectiveSpeed * Mathf.Max(0f, deltaTime);
    }

    /// <summary>
    /// Moves from <paramref name="current"/> toward <paramref name="target"/> by at most
    /// <paramref name="maxDistance"/>. When the budget reaches the target, the result lands exactly on it
    /// and reports the unspent budget as leftover.
    /// </summary>
    public static PlatformStepResult Advance(Vector3 current, Vector3 target, float maxDistance)
    {
        float budget = Mathf.Max(0f, maxDistance);
        Vector3 toTarget = target - current;
        float distance = toTarget.magnitude;

        if (budget >= distance)
            return new PlatformStepResult(target, toTarget, true, budget - distance);

        Vector3 delta = toTarget * (budget / distance);
        return new PlatformStepResult(current + delta, delta, false, 0f);
    }
}
