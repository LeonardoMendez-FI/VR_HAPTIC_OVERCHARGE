Shader "RoboticHUD/HolographicTarget"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineIntensity ("Outline Intensity", Range(0,2)) = 1.0
        _ScanProgress ("Scan Progress", Range(0,1)) = 0.0
        _DrainIntensity ("Drain Intensity", Range(0,1)) = 0.0
        _NoiseScale ("Noise Scale", Float) = 10.0
        _FresnelPower ("Fresnel Power", Range(0.1,5)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Front
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineIntensity;
            float _ScanProgress;
            float _DrainIntensity;
            float _NoiseScale;
            float _FresnelPower;

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Fresnel outline
                float fresnel = 1.0 - abs(dot(i.worldNormal, i.viewDir));
                fresnel = pow(fresnel, _FresnelPower);
                float outlineAlpha = fresnel * _OutlineIntensity;

                // Scanline effect
                float scanLine = 1.0 - abs(i.uv.y - _ScanProgress) * 20.0;
                scanLine = saturate(scanLine);

                // Electricity effect when draining
                float electric = 0.0;
                if (_DrainIntensity > 0.01)
                {
                    float n = noise(i.uv * _NoiseScale + _Time.y * 0.5);
                    electric = step(0.7, n) * _DrainIntensity;
                }

                // Combine
                float alpha = outlineAlpha * (0.5 + scanLine * 0.5) + electric * 0.8;
                alpha = saturate(alpha);

                fixed4 col = _OutlineColor;
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}