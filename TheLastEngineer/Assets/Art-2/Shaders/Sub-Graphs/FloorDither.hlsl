// Floor dither: hides the floors the player is not on, driven by globals that FloorDitherManager publishes.
//   _FloorCutHeights.xyz : world Y where floors 0|1, 1|2 and 2|3 meet (w unused).
//   _FloorFade           : how hidden each floor is, 0 = visible, 1 = hidden (x = lowest floor).
//   _FloorOccluderPass   : set to 1 by PlayerXRayFeature around its occluder depth pass, so every
//                          floor that is even partially hidden is discarded there instead of dithered.
// Unset globals are 0, which means every floor is visible: scenes without a manager look normal.

#ifndef FLOOR_DITHER_INCLUDED
#define FLOOR_DITHER_INCLUDED

float4 _FloorCutHeights;
float4 _FloorFade;
float _FloorOccluderPass;

// Same 4x4 Bayer thresholds as Shader Graph's Dither node, so the pattern matches the old floor shader.
static const float FLOOR_DITHER_THRESHOLDS[16] =
{
    1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
    13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
    4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
    16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
};

float FloorDither_Fade(float worldY)
{
    float floorIndex = step(_FloorCutHeights.x, worldY)
                     + step(_FloorCutHeights.y, worldY)
                     + step(_FloorCutHeights.z, worldY);
    float4 isFloor = float4(floorIndex == 0.0, floorIndex == 1.0, floorIndex == 2.0, floorIndex == 3.0);
    return dot(_FloorFade, isFloor);
}

// Visibility is 1 (keep) or 0 (discard). Multiply it into Alpha; opaque graphs need Alpha Clip on.
// Only a _float version: it takes a world position, which must not drop to half precision, so the
// Custom Function node is set to Single precision.
void FloorDither_float(float3 PositionWS, float4 ScreenPosition, out float Visibility)
{
    float fade = FloorDither_Fade(PositionWS.y);

    if (_FloorOccluderPass > 0.5)
    {
        Visibility = fade > 0.0 ? 0.0 : 1.0;
        return;
    }

    uint2 pixel = (uint2)(ScreenPosition.xy * _ScreenParams.xy);
    uint index = (pixel.x % 4) * 4 + pixel.y % 4;
    Visibility = (1.0 - fade) > FLOOR_DITHER_THRESHOLDS[index] ? 1.0 : 0.0;
}

#endif
