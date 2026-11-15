using UnityEngine;

/// <summary>
/// Pure geometry behind <c>Rope</c>: how many segments it needs and the sagging pose it starts in,
/// so the chain is built already at its full length and doesn't snap on the first physics step.
/// </summary>
public static class RopeLayout
{
    private const float DegenerateSqrMagnitude = 1e-6f;

    public static float RopeLength(float distance, float slack) => distance * (1f + Mathf.Max(0f, slack));

    public static int SegmentCount(float ropeLength, float segmentLength, int maxSegments)
    {
        int max = Mathf.Max(1, maxSegments);
        if (segmentLength <= 0f) return max;

        return Mathf.Clamp(Mathf.CeilToInt(ropeLength / segmentLength), 1, max);
    }

    /// <summary>
    /// Mid-span drop of a parabola of length <paramref name="ropeLength"/> hung over
    /// <paramref name="distance"/>, from the shallow-sag approximation L ≈ D + 8s² / (3D).
    /// </summary>
    public static float SagDepth(float distance, float ropeLength)
    {
        if (distance <= 0f || ropeLength <= distance) return 0f;

        return Mathf.Sqrt(3f * distance * (ropeLength - distance) / 8f);
    }

    /// <summary>Fills <paramref name="points"/> (first = start, last = end) along the sagging parabola.</summary>
    public static void FillPoints(Vector3 start, Vector3 end, float sag, Vector3 down, Vector3[] points)
    {
        int last = points.Length - 1;
        if (last < 1)
        {
            if (last == 0) points[0] = start;
            return;
        }

        Vector3 sagDirection = SagDirection(end - start, down);
        for (int i = 0; i <= last; i++)
        {
            float t = (float)i / last;
            points[i] = Vector3.Lerp(start, end, t) + sagDirection * (4f * sag * t * (1f - t));
        }
    }

    /// <summary>"Down" made perpendicular to the span; any perpendicular when the span is vertical.</summary>
    public static Vector3 SagDirection(Vector3 span, Vector3 down)
    {
        Vector3 direction = Vector3.ProjectOnPlane(down, span);
        if (direction.sqrMagnitude < DegenerateSqrMagnitude) direction = Vector3.ProjectOnPlane(Vector3.right, span);
        if (direction.sqrMagnitude < DegenerateSqrMagnitude) direction = Vector3.ProjectOnPlane(Vector3.forward, span);

        return direction.normalized;
    }
}
