using UnityEngine;

/// <summary>
/// Pure math behind <c>FloatingPlatform</c>: where a passenger's weight is applied and how it fades
/// in and out, so stepping on or off doesn't kick the platform in a single physics step.
/// </summary>
public static class FloatingPlatformLoad
{
    /// <summary>
    /// Projects a point (platform local space) onto the platform's top face, clamped inside its
    /// footprint so a passenger hanging over the edge never gets more leverage than the edge itself.
    /// </summary>
    public static Vector3 ClampToTopFace(Vector3 localPoint, Vector3 boxCenter, Vector3 boxSize)
    {
        Vector3 half = boxSize * 0.5f;
        return new Vector3(
            Mathf.Clamp(localPoint.x, boxCenter.x - half.x, boxCenter.x + half.x),
            boxCenter.y + half.y,
            Mathf.Clamp(localPoint.z, boxCenter.z - half.z, boxCenter.z + half.z));
    }

    /// <summary>Moves the load factor (0..1) toward 1 while loaded and toward 0 otherwise.</summary>
    public static float StepLoad(float current, bool loaded, float rampTime, float deltaTime)
    {
        float target = loaded ? 1f : 0f;
        if (rampTime <= 0f) return target;

        return Mathf.MoveTowards(current, target, deltaTime / rampTime);
    }
}
