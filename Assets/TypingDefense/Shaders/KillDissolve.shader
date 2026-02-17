Shader "TypingDefense/KillDissolve"
{
    Properties
    {
        _Color ("Main Color", Color) = (0.3, 1.0, 0.3, 1.0)
        _EdgeColor ("Edge Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Cutoff ("Dissolve Cutoff", Range(0, 1)) = 0.0
        _EdgeWidth ("Edge Width", Range(0.01, 0.2)) = 0.05
        _NoiseScale ("Noise Scale", Float) = 8.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            Blend One One
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            fixed4 _EdgeColor;
            float _Cutoff;
            float _EdgeWidth;
            float _NoiseScale;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Procedural hash noise (no texture needed)
            float2 hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float valueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);

                float a = frac(sin(dot(i, float2(127.1, 311.7))) * 43758.5453);
                float b = frac(sin(dot(i + float2(1, 0), float2(127.1, 311.7))) * 43758.5453);
                float c = frac(sin(dot(i + float2(0, 1), float2(127.1, 311.7))) * 43758.5453);
                float d = frac(sin(dot(i + float2(1, 1), float2(127.1, 311.7))) * 43758.5453);

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float noise = valueNoise(i.uv * _NoiseScale);

                // Discard pixels below cutoff
                float dissolve = noise - _Cutoff;
                clip(dissolve);

                // Edge glow: bright band at the dissolve frontier
                float edge = 1.0 - smoothstep(0.0, _EdgeWidth, dissolve);

                fixed4 col = lerp(_Color, _EdgeColor, edge);

                // Intensify edge brightness
                col.rgb *= 1.0 + edge * 2.0;

                // Fade out fully dissolved areas
                col.a = smoothstep(0.0, _EdgeWidth * 0.5, dissolve);

                // Modulate by edge for additive punch
                col.rgb *= col.a + edge;

                return col;
            }
            ENDCG
        }
    }

    FallBack Off
}
