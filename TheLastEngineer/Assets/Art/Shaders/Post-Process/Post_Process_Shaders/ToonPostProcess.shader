Shader "TheLastEngineer/PostProcess/Toon"
{
    Properties
    {
        [Header(Outline)][Space(6)]
        _OutlineColor      ("Outline Color", Color) = (0.03, 0.03, 0.05, 1)
        _Thickness         ("Thickness (px)", Range(0.0, 6.0)) = 1.0
        _OutlineOpacity    ("Outline Opacity", Range(0.0, 1.0)) = 1.0

        [Space(6)]
        _DepthThreshold    ("Depth Threshold", Range(0.0, 1.0)) = 0.02
        _DepthStrength     ("Depth Strength", Range(0.0, 200.0)) = 60.0
        _NormalThreshold   ("Normal Threshold", Range(0.0, 2.0)) = 0.30
        _NormalStrength    ("Normal Strength", Range(0.0, 20.0)) = 6.0
        _GrazingSuppress   ("Grazing Angle Suppress", Range(0.0, 1.0)) = 0.75

        [Space(6)]
        _OutlineFadeStart  ("Outline Fade Start (m)", Float) = 40.0
        _OutlineFadeEnd    ("Outline Fade End (m)", Float) = 90.0

        [Header(Posterize)][Space(6)]
        _Bands             ("Bands", Range(2.0, 32.0)) = 5.0
        _BandStrength      ("Band Strength", Range(0.0, 1.0)) = 1.0
        _BandGamma         ("Band Gamma", Range(0.2, 3.0)) = 1.0
        [Enum(Luminance,0,RGB,1)] _BandMode ("Band Mode", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend Off

        Pass
        {
            Name "ToonPostProcess"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // Aporta: Varyings, Vert, _BlitTexture, _BlitMipLevel, sampler_LinearClamp, sampler_PointClamp
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // ---------------------------------------------------------------
            // Depth / normals propios (ToonDepthNormalsFeature)
            //
            // No se usan _CameraDepthTexture / _CameraNormalsTexture: el prepass de URP corre con
            // RenderQueueRange.opaque fijo, asi que los transparentes nunca entran ahi y el outline
            // terminaba calculandose sobre el opaco que esta DETRAS del vidrio, pisandolo.
            // ToonDepthNormalsFeature rellena estas dos con opacos + transparentes seleccionados.
            //
            // Las normales llegan en world space sin codificar: el pass dibuja con los light modes
            // DepthNormals / DepthNormalsOnly, nunca al G-buffer, asi que no aplica _GBUFFER_NORMALS_OCT.
            // ---------------------------------------------------------------
            TEXTURE2D_X_FLOAT(_ToonDepthTexture);
            float4 _ToonDepthTexture_TexelSize;
            TEXTURE2D_X_FLOAT(_ToonNormalsTexture);

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

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float  _Thickness;
                float  _OutlineOpacity;
                float  _DepthThreshold;
                float  _DepthStrength;
                float  _NormalThreshold;
                float  _NormalStrength;
                float  _GrazingSuppress;
                float  _OutlineFadeStart;
                float  _OutlineFadeEnd;
                float  _Bands;
                float  _BandStrength;
                float  _BandGamma;
                float  _BandMode;
            CBUFFER_END

            // ---------------------------------------------------------------
            // Helpers
            // ---------------------------------------------------------------

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

            // Cuantiza preservando el tono: escalona la luminancia y reescala el RGB.
            half3 Posterize(half3 c, float bands, float gamma, float mode)
            {
                float steps = max(round(bands), 2.0) - 1.0;
                float invG  = 1.0 / max(gamma, 1e-3);

                if (mode > 0.5)
                {
                    // Modo RGB: cuantiza cada canal (mas contrastado, puede rotar el hue).
                    half3 g = pow(saturate(c), gamma);
                    g = round(g * steps) / steps;
                    return pow(g, invG);
                }

                float lum = max(Luminance(c), 1e-4);
                float q   = pow(saturate(lum), gamma);
                q = round(q * steps) / steps;
                q = pow(q, invG);
                return c * (q / lum);
            }

            // ---------------------------------------------------------------
            // Fragment
            // ---------------------------------------------------------------
            half4 Frag(Varyings input) : SV_Target0
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                half4 source = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);

                float rawDepth = SampleToonDepth(uv);

                // Skybox / fondo: se devuelve el color original intacto, sin outline ni banding.
                if (IsBackground(rawDepth))
                    return source;

                // -----------------------------------------------------------
                // Roberts Cross: 4 taps en aspa alrededor del pixel.
                // (Para Sobel: reemplazar por 8 taps y pesos [1 2 1] en cada eje;
                //  duplica el costo y suaviza un poco mas las diagonales.)
                // -----------------------------------------------------------
                float2 texel = _ToonDepthTexture_TexelSize.xy * _Thickness;

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
                float  grazing    = lerp(1.0, NdotV, _GrazingSuppress);
                float  depthThreshold = _DepthThreshold / max(grazing, 0.05);

                float dEdge = saturate((depthEdge  - depthThreshold)   * _DepthStrength);
                float nEdge = saturate((normalEdge - _NormalThreshold) * _NormalStrength);
                float edge  = max(dEdge, nEdge);

                // Desvanecido por distancia: evita ruido de lineas en geometria lejana.
                float fade = 1.0 - smoothstep(_OutlineFadeStart, _OutlineFadeEnd, dC);
                edge *= fade * _OutlineOpacity;

                // -----------------------------------------------------------
                // Posterizacion + composicion del outline
                // -----------------------------------------------------------
                half3 banded = Posterize(source.rgb, _Bands, _BandGamma, _BandMode);
                half3 color  = lerp(source.rgb, banded, _BandStrength);
                color = lerp(color, _OutlineColor.rgb, edge * _OutlineColor.a);

                return half4(color, source.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
