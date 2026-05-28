//
//  OutlineFillSoft.shader
//  QuickOutline — Soft Glow variant
//
//  Técnica: capas concéntricas de extrusión (multi-pass layered shell).
//  Cada pass extruye el shell a una fracción diferente de _OutlineWidth
//  con alpha decreciente. Con Blend aditivo (SrcAlpha One), las capas
//  internas (más cercanas al objeto) acumulan más luz → opacas.
//  Las capas externas aportan poco → transparentes. Resultado: fade
//  correcto de opaco-cerca-del-objeto a transparente-lejos-del-objeto.
//
//  _OutlineWidth, _OutlineColor y _ZTest son seteados por Outline.cs
//  igual que en el material original. El script no requiere cambios.
//

Shader "Custom/Outline Fill Soft" {
  Properties {
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0

    _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
    _OutlineWidth("Outline Width", Range(0, 10)) = 2

    // Multiplica el RGB de cada capa para intensificar el glow.
    // Con valores > 1 interactúa con el Bloom de URP automáticamente.
    _GlowIntensity("Glow Intensity", Range(1.0, 4.0)) = 1.5
  }

  SubShader {
    Tags {
      "Queue"           = "Transparent+110"
      "RenderType"      = "Transparent"
      "DisableBatching" = "True"
    }

    // ─────────────────────────────────────────────────────────────────
    // PASS 4 — Capa exterior (extrusion × 1.0 | alpha × 0.08)
    // La más alejada del objeto: halo muy tenue, borde suave exterior.
    // ─────────────────────────────────────────────────────────────────
    Pass {
      Name "SoftFill_Outer"
      Cull Off
      ZTest [_ZTest]
      ZWrite Off
      Blend SrcAlpha One      // blending aditivo: suma luz sin oscurecer
      ColorMask RGB

      Stencil {
        Ref 1
        Comp NotEqual
      }

      CGPROGRAM
      #include "UnityCG.cginc"
      #pragma vertex vert
      #pragma fragment frag

      struct appdata {
        float4 vertex       : POSITION;
        float3 normal       : NORMAL;
        float3 smoothNormal : TEXCOORD3;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f {
        float4 position : SV_POSITION;
        fixed4 color    : COLOR;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      uniform fixed4 _OutlineColor;
      uniform float  _OutlineWidth;
      uniform float  _GlowIntensity;

      v2f vert(appdata input) {
        v2f output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        float3 normal       = any(input.smoothNormal) ? input.smoothNormal : input.normal;
        float3 viewPosition = UnityObjectToViewPos(input.vertex);
        float3 viewNormal   = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normal));

        // Extrusión al 100 % del ancho configurado
        output.position = UnityViewToClipPos(
          viewPosition + viewNormal * -viewPosition.z * _OutlineWidth / 1000.0
        );

        output.color     = _OutlineColor;
        output.color.rgb *= _GlowIntensity;
        output.color.a   *= 0.08;   // capa exterior: casi transparente
        return output;
      }

      fixed4 frag(v2f input) : SV_Target { return input.color; }
      ENDCG
    }

    // ─────────────────────────────────────────────────────────────────
    // PASS 3 — Capa media-exterior (extrusion × 0.70 | alpha × 0.15)
    // ─────────────────────────────────────────────────────────────────
    Pass {
      Name "SoftFill_MidOuter"
      Cull Off
      ZTest [_ZTest]
      ZWrite Off
      Blend SrcAlpha One
      ColorMask RGB

      Stencil {
        Ref 1
        Comp NotEqual
      }

      CGPROGRAM
      #include "UnityCG.cginc"
      #pragma vertex vert
      #pragma fragment frag

      struct appdata {
        float4 vertex       : POSITION;
        float3 normal       : NORMAL;
        float3 smoothNormal : TEXCOORD3;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f {
        float4 position : SV_POSITION;
        fixed4 color    : COLOR;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      uniform fixed4 _OutlineColor;
      uniform float  _OutlineWidth;
      uniform float  _GlowIntensity;

      v2f vert(appdata input) {
        v2f output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        float3 normal       = any(input.smoothNormal) ? input.smoothNormal : input.normal;
        float3 viewPosition = UnityObjectToViewPos(input.vertex);
        float3 viewNormal   = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normal));

        // Extrusión al 70 % — ring medio-exterior
        output.position = UnityViewToClipPos(
          viewPosition + viewNormal * -viewPosition.z * (_OutlineWidth * 0.70) / 1000.0
        );

        output.color     = _OutlineColor;
        output.color.rgb *= _GlowIntensity;
        output.color.a   *= 0.15;
        return output;
      }

      fixed4 frag(v2f input) : SV_Target { return input.color; }
      ENDCG
    }

    // ─────────────────────────────────────────────────────────────────
    // PASS 2 — Capa media-interior (extrusion × 0.40 | alpha × 0.35)
    // ─────────────────────────────────────────────────────────────────
    Pass {
      Name "SoftFill_MidInner"
      Cull Off
      ZTest [_ZTest]
      ZWrite Off
      Blend SrcAlpha One
      ColorMask RGB

      Stencil {
        Ref 1
        Comp NotEqual
      }

      CGPROGRAM
      #include "UnityCG.cginc"
      #pragma vertex vert
      #pragma fragment frag

      struct appdata {
        float4 vertex       : POSITION;
        float3 normal       : NORMAL;
        float3 smoothNormal : TEXCOORD3;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f {
        float4 position : SV_POSITION;
        fixed4 color    : COLOR;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      uniform fixed4 _OutlineColor;
      uniform float  _OutlineWidth;
      uniform float  _GlowIntensity;

      v2f vert(appdata input) {
        v2f output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        float3 normal       = any(input.smoothNormal) ? input.smoothNormal : input.normal;
        float3 viewPosition = UnityObjectToViewPos(input.vertex);
        float3 viewNormal   = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normal));

        // Extrusión al 40 % — ring medio-interior
        output.position = UnityViewToClipPos(
          viewPosition + viewNormal * -viewPosition.z * (_OutlineWidth * 0.40) / 1000.0
        );

        output.color     = _OutlineColor;
        output.color.rgb *= _GlowIntensity;
        output.color.a   *= 0.35;
        return output;
      }

      fixed4 frag(v2f input) : SV_Target { return input.color; }
      ENDCG
    }

    // ─────────────────────────────────────────────────────────────────
    // PASS 1 — Capa interior (extrusion × 0.15 | alpha × 1.0)
    // La más cercana al objeto: totalmente opaca, borde nítido interno.
    // ─────────────────────────────────────────────────────────────────
    Pass {
      Name "SoftFill_Inner"
      Cull Off
      ZTest [_ZTest]
      ZWrite Off
      Blend SrcAlpha One
      ColorMask RGB

      Stencil {
        Ref 1
        Comp NotEqual
      }

      CGPROGRAM
      #include "UnityCG.cginc"
      #pragma vertex vert
      #pragma fragment frag

      struct appdata {
        float4 vertex       : POSITION;
        float3 normal       : NORMAL;
        float3 smoothNormal : TEXCOORD3;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f {
        float4 position : SV_POSITION;
        fixed4 color    : COLOR;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      uniform fixed4 _OutlineColor;
      uniform float  _OutlineWidth;
      uniform float  _GlowIntensity;

      v2f vert(appdata input) {
        v2f output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        float3 normal       = any(input.smoothNormal) ? input.smoothNormal : input.normal;
        float3 viewPosition = UnityObjectToViewPos(input.vertex);
        float3 viewNormal   = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normal));

        // Extrusión al 15 % — ring interior, pegado al borde del objeto
        output.position = UnityViewToClipPos(
          viewPosition + viewNormal * -viewPosition.z * (_OutlineWidth * 0.15) / 1000.0
        );

        output.color     = _OutlineColor;
        output.color.rgb *= _GlowIntensity;
        output.color.a   *= 1.0;    // capa interior: completamente opaca
        return output;
      }

      fixed4 frag(v2f input) : SV_Target { return input.color; }
      ENDCG
    }
  }
}

