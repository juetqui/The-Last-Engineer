Shader "TheLastEngineer/PostProcess/ToonOutline"
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

        HLSLINCLUDE
        #pragma target 3.5

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // Aporta: Varyings, Vert, _BlitTexture, _BlitMipLevel, sampler_LinearClamp, sampler_PointClamp
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "ToonEdges.hlsl"

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
        CBUFFER_END

        ToonEdgeParams BuildEdgeParams()
        {
            ToonEdgeParams edgeParams;
            edgeParams.thickness       = _Thickness;
            edgeParams.depthThreshold  = _DepthThreshold;
            edgeParams.depthStrength   = _DepthStrength;
            edgeParams.normalThreshold = _NormalThreshold;
            edgeParams.normalStrength  = _NormalStrength;
            edgeParams.grazingSuppress = _GrazingSuppress;
            edgeParams.fadeStart       = _OutlineFadeStart;
            edgeParams.fadeEnd         = _OutlineFadeEnd;
            edgeParams.opacity         = _OutlineOpacity;
            return edgeParams;
        }
        ENDHLSL

        // -------------------------------------------------------------------
        // Pass 0: cobertura del borde a un R8 aparte (_ToonOutlineMask).
        //
        // Se calcula una sola vez y se reusa dos veces: la composicion de aca abajo
        // y la posterizacion en AfterRenderingPostProcessing, que usa el mask para no
        // cuantizar las lineas (el banding se comeria el anti-aliasing del borde).
        // -------------------------------------------------------------------
        Pass
        {
            Name "ToonOutlineMask"
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragMask

            half4 FragMask(Varyings input) : SV_Target0
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                float rawDepth = SampleToonDepth(uv);

                // Skybox / fondo: sin linea. El pixel se escribe igual, el target no se limpia aparte.
                if (IsBackground(rawDepth))
                    return (half4)0.0;

                return half4(ComputeToonEdge(uv, rawDepth, BuildEdgeParams()), 0.0, 0.0, 0.0);
            }
            ENDHLSL
        }

        // -------------------------------------------------------------------
        // Pass 1: composicion sobre el color de camara en BeforeRenderingTransparents.
        //
        // Blending por hardware en vez de leer el color: evita la copia del color buffer
        // que necesitaria un blit clasico. Equivale al lerp(color, _OutlineColor, cobertura).
        // ColorMask RGB deja el alpha del target intacto.
        // -------------------------------------------------------------------
        Pass
        {
            Name "ToonOutlineComposite"
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragComposite

            half4 FragComposite(Varyings input) : SV_Target0
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float coverage = SampleToonOutlineMask(input.texcoord.xy);
                return half4(_OutlineColor.rgb, coverage * _OutlineColor.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
