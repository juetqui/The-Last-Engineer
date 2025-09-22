Shader "Custom/S_PortalEffect"
{
    Properties
    {
        [IntRange] _StencilID ("Stencil ID", Range(0,255)) = 1
        _MainTex ("Tunnel Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _ScrollSpeed ("Scroll Speed", float) = 1.0
        _FresnelColor ("Fresnel Color", Color) = (0,1,1,1)
        _FresnelPower ("Fresnel Power", float) = 3.0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Geometry+1"
        }

        Pass
        {
            Name "PortalEmbudo"
            Ztest Always
            // stencil test: solo donde la máscara pintó
            Stencil
            {
                Ref [_StencilID]
                Comp Equal
                Pass Keep
               // Fail Keep
            }

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos   : TEXCOORD1;
                float3 worldNormal: TEXCOORD2;
                float2 uvTunnel   : TEXCOORD0;
            };

            // Declaración correcta en URP
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            float4 _MainTex_ST;
            float _ScrollSpeed;
            float4 _FresnelColor;
            float _FresnelPower;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS).xyz;
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uvTunnel = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Scroll en UV
                float2 uv = IN.uvTunnel;
                uv.y += _Time.y * _ScrollSpeed;
                uv.y = frac(uv.y);

                // texturas
                half4 colTunnel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                half4 noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv * 5.0);

                // agregar distorsión con ruido
                colTunnel.rgb += (noise.rgb - 0.5) * 0.2;

                // fresnel para bordes
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);
                float fres = pow(1.0 - saturate(dot(viewDir, normalize(IN.worldNormal))), _FresnelPower);

                half4 fresCol = _FresnelColor * fres;

                // mezclar túnel + fresnel
                half4 final = colTunnel;
                final.rgb = lerp(final.rgb, fresCol.rgb, fres);
                final.a = 1.0;

                return final;
            }
            ENDHLSL
        }
    }
}


