Shader "TypingDefense/BlackHoleDistortion"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Strength ("Distortion Strength", Range(0, 1)) = 0.4
        _InnerRadius ("Black Core Radius", Range(0.01, 0.5)) = 0.25
        _OuterRadius ("Distortion Radius", Range(0.1, 1.0)) = 0.5
        _RotSpeed ("Rotation Speed", Range(0, 10)) = 2.0
        _EdgeGlow ("Edge Glow Intensity", Range(0, 3)) = 1.0
        _GlowColor ("Glow Color", Color) = (0.5, 0.2, 1.0, 1.0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" }

        GrabPass { "_GrabTexture" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _GrabTexture;
            float _Strength;
            float _InnerRadius;
            float _OuterRadius;
            float _RotSpeed;
            float _EdgeGlow;
            fixed4 _GlowColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 grabPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.uv = v.uv - 0.5;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = length(i.uv);

                // --- SOLID BLACK CORE ---
                // Inside inner radius: opaque black circle
                float coreMask = 1.0 - smoothstep(_InnerRadius * 0.85, _InnerRadius, dist);

                // --- DISTORTION RING (between inner and outer radius) ---
                float ringMask = smoothstep(_InnerRadius * 0.7, _InnerRadius, dist)
                               * (1.0 - smoothstep(_OuterRadius * 0.8, _OuterRadius, dist));

                float distortionFalloff = saturate(1.0 - (dist - _InnerRadius) / (_OuterRadius - _InnerRadius));
                distortionFalloff *= distortionFalloff;

                // Spiral rotation
                float angle = _RotSpeed * _Time.y + distortionFalloff * 6.2831;
                float s = sin(angle);
                float c = cos(angle);

                float2 rotated;
                rotated.x = i.uv.x * c - i.uv.y * s;
                rotated.y = i.uv.x * s + i.uv.y * c;

                float2 distortion = (rotated - i.uv) * _Strength * distortionFalloff;

                // Pull toward center
                float2 pullDir = -normalize(i.uv + 0.0001);
                distortion += pullDir * _Strength * distortionFalloff * 0.15;

                // Sample distorted background
                float2 grabUV = i.grabPos.xy / i.grabPos.w;
                grabUV += distortion * ringMask;
                fixed4 grabColor = tex2D(_GrabTexture, grabUV);

                // --- EDGE GLOW at the border of the black core ---
                float glowRing = smoothstep(_InnerRadius * 0.6, _InnerRadius, dist)
                               * smoothstep(_InnerRadius * 1.8, _InnerRadius * 1.1, dist);
                fixed3 glow = _GlowColor.rgb * glowRing * _EdgeGlow * _Strength;

                // --- COMPOSITE ---
                fixed4 result;

                // Core: solid black
                // Ring: distorted background + glow
                // Blend between them
                result.rgb = lerp(grabColor.rgb + glow, fixed3(0, 0, 0), coreMask);

                // Alpha: visible in core + ring area, fade at outer edge
                float outerFade = smoothstep(_OuterRadius, _OuterRadius * 0.7, dist);
                result.a = max(coreMask, outerFade * _Strength) * i.color.a;

                return result;
            }
            ENDCG
        }
    }

    FallBack Off
}
