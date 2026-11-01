#ifndef VORONOI_TEXTURE_INCLUDED
#define VORONOI_TEXTURE_INCLUDED

// Voronoi como el de Shader Graph, pero las celdas salen de la textura en vez de un hash:
// cada celda lee el texel que cae en su centro y usa R/G como "random" para ubicar su punto.
// AngleOffset se aplica igual que en el nodo Voronoi, así que la animación es la misma.
float2 VT_TextureVector(float2 cell, float cellDensity, float angleOffset,
                        UnityTexture2D tex, UnitySamplerState ss, out float4 texel)
{
    // frac para que las celdas vecinas del borde (-1 o CellDensity) repitan la textura
    float2 texUV = frac((cell + 0.5) / cellDensity);
    // LOD 0 fijo: dentro del loop no hay derivadas válidas y queremos el valor exacto del texel
    texel = SAMPLE_TEXTURE2D_LOD(tex.tex, ss.samplerstate, texUV, 0);
    float2 h = texel.rg;
    return float2(sin(h.y * angleOffset) * 0.5 + 0.5,
                  cos(h.x * angleOffset) * 0.5 + 0.5);
}

void VoronoiTexture_float(
    float2 UV, float AngleOffset, float CellDensity, float Jitter,
    UnityTexture2D Tex, UnitySamplerState SS,
    out float4 Color, out float Distance, out float CellID, out float2 CellUV)
{
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);

    float  best      = 8.0;
    float2 bestDelta = 0;
    float4 bestTexel = 0;

    // El punto más cercano puede caer en cualquiera de las 8 celdas vecinas
    for (int y = -1; y <= 1; y++)
    for (int x = -1; x <= 1; x++)
    {
        float2 lattice = float2(x, y);
        float4 texel;
        float2 p = VT_TextureVector(lattice + g, CellDensity, AngleOffset, Tex, SS, texel);
        // Jitter 0 = puntos en el centro (grilla regular), 1 = igual que el Voronoi de Unity
        p = lerp(0.5, p, Jitter);
        float2 delta = lattice + p - f;
        float  d     = dot(delta, delta);
        if (d < best) { best = d; bestDelta = delta; bestTexel = texel; }
    }

    Distance = sqrt(best);
    Color    = bestTexel;          // color de la textura que "manda" en esa celda
    CellID   = bestTexel.r;        // mismo criterio que la salida Cells del Voronoi
    CellUV   = -bestDelta + 0.5;   // UV local centrado en el punto de la celda
}

// Variante half por si el graph o el nodo se ponen en precisión Half
void VoronoiTexture_half(
    half2 UV, half AngleOffset, half CellDensity, half Jitter,
    UnityTexture2D Tex, UnitySamplerState SS,
    out half4 Color, out half Distance, out half CellID, out half2 CellUV)
{
    float4 c; float d; float id; float2 cuv;
    VoronoiTexture_float(UV, AngleOffset, CellDensity, Jitter, Tex, SS, c, d, id, cuv);
    Color = c; Distance = d; CellID = id; CellUV = cuv;
}

#endif
