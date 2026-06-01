// ============================================================
//  HolographicVisor.shader
//  Sci-Fi Robot HUD System — Holographic UI Shader Stub
//  ============================================================
//  Place in: Assets/Shaders/HolographicVisor.shader
//  Assign to the VisorFrame Image material.
//
//  EFFECT FEATURES:
//    • Scanline overlay (animated offset)
//    • Fresnel-style edge glow
//    • Additive-friendly blending for holographic look
//    • CRT curvature distortion (subtle)
//    • Configurable glow color and scanline density
// ============================================================

Shader "RoboticHUD/HolographicVisor"
{
    Properties
    {
        // Standard UI texture
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        // ── Holographic controls ──
        _Color          ("Tint Color",        Color)  = (0, 0.92, 1, 0.18)
        _GlowColor      ("Glow Color",        Color)  = (0, 0.92, 1, 1.0)
        _GlowIntensity  ("Glow Intensity",    Range(0,3))    = 1.2
        _FresnelPow     ("Edge Fresnel Power",Range(0.1,6))  = 2.5

        // ── Scanlines ──
        _ScanlineColor  ("Scanline Color",    Color)  = (0,0.92,1,0.08)
        _ScanlineDensity("Scanline Density",  Float)  = 240.0
        _ScanlineOffset ("Scanline Offset",   Range(0,1)) = 0.0

        // ── Distortion ──
        _CurvatureX     ("CRT Curvature X",   Range(0,0.2)) = 0.04
        _CurvatureY     ("CRT Curvature Y",   Range(0,0.2)) = 0.04

        // ── Unity UI required ──
        _StencilComp    ("Stencil Comparison", Float) = 8
        _Stencil        ("Stencil ID",         Float) = 0
        _StencilOp      ("Stencil Operation",  Float) = 0
        _StencilWriteMask ("Stencil Write Mask",Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask      ("Color Mask",         Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType"      = "Transparent"
            "PreviewType"     = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref   [_Stencil]
            Comp  [_StencilComp]
            Pass  [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "HOLOGRAPHIC_VISOR"

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            fixed4    _Color;
            fixed4    _GlowColor;
            float     _GlowIntensity;
            float     _FresnelPow;
            fixed4    _ScanlineColor;
            float     _ScanlineDensity;
            float     _ScanlineOffset;
            float     _CurvatureX;
            float     _CurvatureY;
            float4    _ClipRect;

            // ── CRT lens distortion ──────────────────────────
            float2 ApplyCurvature(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / float2(_CurvatureX > 0 ? 1.0/_CurvatureX : 9999,
                                                     _CurvatureY > 0 ? 1.0/_CurvatureY : 9999);
                uv = uv + uv * offset * offset;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            // ── Edge fresnel ──────────────────────────────────
            float EdgeGlow(float2 uv)
            {
                float2 d = abs(uv - 0.5) * 2.0;           // 0 at center, 1 at edge
                float edge = max(d.x, d.y);
                return pow(edge, _FresnelPow);
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPos = v.vertex;
                OUT.vertex   = UnityObjectToClipPos(OUT.worldPos);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color    = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Curvature distortion
                float2 uv = ApplyCurvature(IN.texcoord);

                // Discard outside lens
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                    discard;

                fixed4 col = tex2D(_MainTex, uv) * IN.color;

                // Edge glow (fresnel)
                float glow = EdgeGlow(uv) * _GlowIntensity;
                col.rgb += _GlowColor.rgb * glow * col.a;

                // Scanlines
                float scanline = sin((uv.y + _ScanlineOffset) * _ScanlineDensity * 3.14159) * 0.5 + 0.5;
                col.rgb += _ScanlineColor.rgb * _ScanlineColor.a * scanline;

                // Unity UI clip rect
                col.a *= UnityGet2DClipping(IN.worldPos.xy, _ClipRect);

                return col;
            }
            ENDCG
        }
    }

    // Fallback to standard UI default if shader unsupported
    Fallback "UI/Default"
}
