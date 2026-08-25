Shader "TheLastEngineer/PostProcess/ToonPosterize"
{
    Properties
    {
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
            Name "ToonPosterize"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // Aporta: Varyings, Vert, _BlitTexture, _BlitMipLevel, sampler_LinearClamp, sampler_PointClamp
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            // Aporta: _ToonDepthTexture (descarte de skybox) y _ToonOutlineMask (proteccion del outline)
            #include "ToonEdges.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float  _Bands;
                float  _BandStrength;
                float  _BandGamma;
                float  _BandMode;
            CBUFFER_END

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

            half4 Frag(Varyings input) : SV_Target0
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                half4 source = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);

                // Skybox / fondo: se devuelve el color original intacto, sin banding.
                // Ojo: como _ToonDepthTexture solo tiene opacos, un transparente recortado
                // contra el cielo tampoco se posteriza.
                if (IsBackground(SampleToonDepth(uv)))
                    return source;

                // El outline ya se compuso en BeforeRenderingTransparents. Cuantizar sus pixeles
                // snapearia los valores intermedios del borde y las lineas quedarian aliaseadas,
                // asi que el banding se apaga proporcionalmente a la cobertura de la linea.
                float coverage = SampleToonOutlineMask(uv);

                half3 banded = Posterize(source.rgb, _Bands, _BandGamma, _BandMode);
                half3 color  = lerp(source.rgb, banded, _BandStrength * (1.0 - coverage));

                return half4(color, source.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
