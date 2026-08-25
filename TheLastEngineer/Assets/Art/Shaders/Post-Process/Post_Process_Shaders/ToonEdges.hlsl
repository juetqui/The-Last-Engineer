#ifndef TOON_EDGES_INCLUDED
#define TOON_EDGES_INCLUDED

// ---------------------------------------------------------------
// Depth / normals propios (ToonDepthNormalsFeature)
//
// No se usan _CameraDepthTexture / _CameraNormalsTexture: el prepass de URP corre con
// RenderQueueRange.opaque fijo y ademas se resuelve en un momento del frame sobre el que
// no tenemos control. ToonDepthNormalsFeature rellena estas dos en AfterRenderingPrePasses,
// antes de cualquiera de las dos pasadas toon.
//
// Solo contiene OPACOS: el outline se compone en BeforeRenderingTransparents, cuando los
// transparentes todavia no se dibujaron, asi que sus siluetas no pueden participar.
//
// Las normales llegan en world space sin codificar: el pass dibuja con los light modes
// DepthNormals / DepthNormalsOnly, nunca al G-buffer, asi que no aplica _GBUFFER_NORMALS_OCT.
// ---------------------------------------------------------------
TEXTURE2D_X_FLOAT(_ToonDepthTexture);
float4 _ToonDepthTexture_TexelSize;
TEXTURE2D_X_FLOAT(_ToonNormalsTexture);

// Cobertura del outline escrita por ToonOutlineFeature (R8, 0 = sin linea).
// La lee la posterizacion para no cuantizar los pixeles de linea.
TEXTURE2D_X(_ToonOutlineMask);

float SampleToonDepth(float2 uv)
{
    return SAMPLE_TEXTURE2D_X(_ToonDepthTexture, sampler_PointClamp,
                              UnityStereoTransformScreenSpaceTex(uv)).r;
}

float3 SampleToonNormals(float2 uv)
{
    return SAMPLE_TEXTURE2D_X(_ToonNormalsTexture, sampler_PointClamp,
                              UnityStereoTransformScreenSpaceTex(uv)).xyz;
}

float SampleToonOutlineMask(float2 uv)
{
    return SAMPLE_TEXTURE2D_X(_ToonOutlineMask, sampler_PointClamp,
                              UnityStereoTransformScreenSpaceTex(uv)).r;
}

// El skybox nunca escribe profundidad: en reversed-Z el raw depth queda en 0.
bool IsBackground(float rawDepth)
{
#if UNITY_REVERSED_Z
    return rawDepth <= 1e-6;
#else
    return rawDepth >= 1.0 - 1e-6;
#endif
}

// Profundidad lineal en metros, acotada al far plane para que los taps que caen
// sobre el skybox no generen infinitos en la resta de Roberts.
float SampleEyeDepth(float2 uv)
{
    float raw = SampleToonDepth(uv);
    return min(LinearEyeDepth(raw, _ZBufferParams), _ProjectionParams.z);
}

struct ToonEdgeParams
{
    float thickness;
    float depthThreshold;
    float depthStrength;
    float normalThreshold;
    float normalStrength;
    float grazingSuppress;
    float fadeStart;
    float fadeEnd;
    float opacity;
};

// Cobertura del borde en [0, 1] para un pixel que NO es background (el caller ya lo descarto).
float ComputeToonEdge(float2 uv, float rawDepth, ToonEdgeParams edgeParams)
{
    // -----------------------------------------------------------
    // Roberts Cross: 4 taps en aspa alrededor del pixel.
    // (Para Sobel: reemplazar por 8 taps y pesos [1 2 1] en cada eje;
    //  duplica el costo y suaviza un poco mas las diagonales.)
    // -----------------------------------------------------------
    float2 texel = _ToonDepthTexture_TexelSize.xy * edgeParams.thickness;

    float2 uvTL = uv + float2(-1.0,  1.0) * texel;
    float2 uvTR = uv + float2( 1.0,  1.0) * texel;
    float2 uvBL = uv + float2(-1.0, -1.0) * texel;
    float2 uvBR = uv + float2( 1.0, -1.0) * texel;

    // --- Bordes por profundidad (siluetas / cambios de plano) ---
    float dC  = min(LinearEyeDepth(rawDepth, _ZBufferParams), _ProjectionParams.z);
    float dTL = SampleEyeDepth(uvTL);
    float dTR = SampleEyeDepth(uvTR);
    float dBL = SampleEyeDepth(uvBL);
    float dBR = SampleEyeDepth(uvBR);

    float dDiag1 = dBR - dTL;
    float dDiag2 = dBL - dTR;
    // Normalizado por la distancia: un escalon de 10cm pesa igual cerca que lejos.
    float depthEdge = sqrt(dDiag1 * dDiag1 + dDiag2 * dDiag2) / max(dC, 1e-4);

    // --- Bordes por normales (esquinas internas, quiebres de angulo) ---
    float3 nC  = SampleToonNormals(uv);
    float3 nTL = SampleToonNormals(uvTL);
    float3 nTR = SampleToonNormals(uvTR);
    float3 nBL = SampleToonNormals(uvBL);
    float3 nBR = SampleToonNormals(uvBR);

    float3 nDiag1 = nBR - nTL;
    float3 nDiag2 = nBL - nTR;
    float normalEdge = sqrt(dot(nDiag1, nDiag1) + dot(nDiag2, nDiag2));

    // --- Supresion en angulos rasantes ---
    // Un piso visto casi de canto tiene un gradiente de profundidad enorme sin ser
    // un borde real: se sube el umbral en funcion de N.V.
    float3 positionWS = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
    float3 viewDirWS  = normalize(GetCameraPositionWS() - positionWS);
    float  NdotV      = saturate(dot(nC, viewDirWS));
    float  grazing    = lerp(1.0, NdotV, edgeParams.grazingSuppress);
    float  depthThreshold = edgeParams.depthThreshold / max(grazing, 0.05);

    float dEdge = saturate((depthEdge  - depthThreshold)        * edgeParams.depthStrength);
    float nEdge = saturate((normalEdge - edgeParams.normalThreshold) * edgeParams.normalStrength);
    float edge  = max(dEdge, nEdge);

    // Desvanecido por distancia: evita ruido de lineas en geometria lejana.
    float fade = 1.0 - smoothstep(edgeParams.fadeStart, edgeParams.fadeEnd, dC);
    return edge * fade * edgeParams.opacity;
}

#endif // TOON_EDGES_INCLUDED
