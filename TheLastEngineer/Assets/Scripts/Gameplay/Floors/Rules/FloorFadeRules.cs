using System;

/// <summary>
/// How hidden each floor is for a given current floor. Values are "fade": 0 = fully visible,
/// 1 = fully hidden. Fade (not opacity) is what the shaders read, so an unset global (all zeros)
/// means every floor is visible in scenes without a floor manager.
/// </summary>
public static class FloorFadeRules
{
    /// <summary>The shader packs one fade per floor into a float4, so there can be at most 4 floors.</summary>
    public const int MaxFloors = 4;

    /// <summary>
    /// Fills <paramref name="fades"/> (one entry per floor, lowest first):
    /// the current floor is visible, the one below uses <paramref name="lowerFloorOpacity"/>,
    /// the one above uses <paramref name="upperFloorOpacity"/> and every other floor is hidden.
    /// </summary>
    public static void Compute(int currentFloor, float lowerFloorOpacity, float upperFloorOpacity, float[] fades)
    {
        if (fades == null) throw new ArgumentNullException(nameof(fades));
        if (fades.Length == 0 || fades.Length > MaxFloors)
            throw new ArgumentException($"Expected between 1 and {MaxFloors} floors, got {fades.Length}.", nameof(fades));
        if (currentFloor < 0 || currentFloor >= fades.Length)
            throw new ArgumentOutOfRangeException(nameof(currentFloor), currentFloor, $"Must be in [0, {fades.Length - 1}].");

        for (var floor = 0; floor < fades.Length; floor++)
        {
            float opacity;
            if (floor == currentFloor) opacity = 1f;
            else if (floor == currentFloor - 1) opacity = lowerFloorOpacity;
            else if (floor == currentFloor + 1) opacity = upperFloorOpacity;
            else opacity = 0f;

            fades[floor] = 1f - Clamp01(opacity);
        }
    }

    private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
}
