Shader "TheLastEngineer/PostProcess/PlayerXRay"
{
    Properties
    {
        [Header(Outline)][Space(6)]
        [HDR] _OutlineColor ("Outline Color", Color) = (0.2, 0.9, 1.0, 1.0)
        _Thickness          ("Thickness (px)", Range(1.0, 8.0)) = 2.5
        _OutlineOpacity     ("Outline Opacity", Range(0.0, 1.0)) = 1.0

        [Header(Fill)][Space(6)]
        [HDR] _FillColor    ("Fill Color", Color) = (0.2, 0.9, 1.0, 1.0)
        _FillOpacity        ("Fill Opacity", Range(0.0, 1.0)) = 0.25

        [Header(Occlusion)][Space(6)]
        _DepthBias          ("Depth Bias (m)", Range(0.0, 0.5)) = 0.05
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        #pragma target 3.5

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        // Un solo CBUFFER para los dos passes: el pass 0 se usa como override material sobre el
        // renderer del jugador y el SRP Batcher exige el mismo layout en todos los passes.
        CBUFFER_START(UnityPerMaterial)
            float4 _OutlineColor;
            float  _Thickness;
            float  _OutlineOpacity;
            float4 _FillColor;
            float  _FillOpacity;
            float  _DepthBias;
        CBUFFER_END

        // Depth de los oclusores "reales" (sin jugador ni pisos dithered), lo llena PlayerXRayFeature.
        TEXTURE2D_X_FLOAT(_XRayOccluderDepth);

        // R = cobertura del jugador, G = cobertura del jugador tapada por un oclusor.
        TEXTURE2D_X(_XRayMask);

        // Render Graph bindea la mascara por RenderTargetIdentifier y no llena _XRayMask_TexelSize
        // (mismo problema que el depth toon): PlayerXRayFeature lo pasa por MaterialPropertyBlock.
        float4 _XRayMaskTexel;

        // Global, lo escribe PlayerXRayTarget: fades de muerte/cinematica/dissolve sin tocar el material.
        float _XRayIntensity;

        // Globales, los escribe PlayerXRayTarget con el color de GlitchPalette segun el nodo en mano.
        // El alpha es el peso del reemplazo: sin nadie que los escriba valen 0 y queda el color del
        // material (Scene view, escenas sin jugador). La opacidad siempre sale del material.
        float4 _XRayOutlineTint;
        float4 _XRayFillTint;
        ENDHLSL

        // -------------------------------------------------------------------
        // Pass 0: mascara. Se dibuja como override material sobre el jugador, contra un depth
        // propio (ZTest LEqual) para que decida el fragmento mas cercano: si no, una cara
        // trasera o el torso detras del brazo marcarian como "tapado" algo que se ve.
        // -------------------------------------------------------------------
        Pass
        {
            Name "XRayMask"
            Cull Back
            ZWrite On
            ZTest LEqual
            Blend Off

            HLSLPROGRAM
            #pragma vertex VertMask
            #pragma fragment FragMask
            #pragma multi_compile_instancing

            struct AttributesMask
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VaryingsMask
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            VaryingsMask VertMask(AttributesMask input)
            {
                VaryingsMask output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 FragMask(VaryingsMask input) : SV_Target0
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Load por pixel en vez de sample: la mascara y el depth de oclusores tienen el
                // mismo tamaño, asi que no hace falta filtrar ni pasar por UV.
                float occluderRaw = LOAD_TEXTURE2D_X(_XRayOccluderDepth, uint2(input.positionCS.xy)).r;

                float playerEye = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
                float occluderEye = LinearEyeDepth(occluderRaw, _ZBufferParams);

                // El bias evita que el jugador apoyado contra una pared (o la precision del depth)
                // lo marque como tapado.
                half occluded = playerEye > occluderEye + _DepthBias ? 1.0 : 0.0;
                return half4(1.0, occluded, 0.0, 0.0);
            }
            ENDHLSL
        }

        // -------------------------------------------------------------------
        // Pass 1: composicion full screen sobre el color de camara.
        //
        // Relleno donde el jugador esta tapado; outline solo FUERA de la silueta (R == 0), asi
        // el borde de la pared no dibuja una linea cruzando la parte del jugador que se ve.
        // Blending por hardware, igual que ToonOutline: no hace falta copiar el color.
        // -------------------------------------------------------------------
        Pass
        {
            Name "XRayComposite"
            Cull Off
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB

            HLSLPROGRAM
            // Aporta: Varyings, Vert, sampler_PointClamp
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment FragComposite

            #define XRAY_RING_TAPS 12

            half2 SampleMask(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_XRayMask, sampler_PointClamp,
                                          UnityStereoTransformScreenSpaceTex(uv)).rg;
            }

            half4 FragComposite(Varyings input) : SV_Target0
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                if (_XRayIntensity <= 0.0)
                    return (half4)0.0;

                float2 uv = input.texcoord.xy;
                half2 center = SampleMask(uv);

                // Dentro de la silueta: solo relleno, y solo en la parte tapada.
                if (center.r > 0.5)
                    return half4(lerp(_FillColor.rgb, _XRayFillTint.rgb, _XRayFillTint.a), center.g * _FillColor.a * _FillOpacity * _XRayIntensity);

                float2 texel = _XRayMaskTexel.xy;

                // Dos anillos (radio completo y medio) para que con grosores altos la linea no
                // se quiebre entre taps.
                half coverage = 0.0;
                [unroll]
                for (int i = 0; i < XRAY_RING_TAPS; i++)
                {
                    float angle = i * (TWO_PI / XRAY_RING_TAPS);
                    float2 dir = float2(cos(angle), sin(angle)) * texel * _Thickness;
                    coverage = max(coverage, SampleMask(uv + dir).g);
                    coverage = max(coverage, SampleMask(uv + dir * 0.5).g);
                }

                return half4(lerp(_OutlineColor.rgb, _XRayOutlineTint.rgb, _XRayOutlineTint.a), coverage * _OutlineColor.a * _OutlineOpacity * _XRayIntensity);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
