Shader "TypingDefense/PulseGlow"
{
    Properties
    {
        _Color ("Glow Color", Color) = (1.0, 0.2, 0.0, 1.0)
        _PulseSpeed ("Pulse Speed", Range(0.1, 20)) = 3.0
        _PulseMin ("Pulse Min", Range(0, 1)) = 0.3
        _PulseMax ("Pulse Max", Range(0, 3)) = 1.0
        _SoftEdge ("Soft Edge", Range(0.01, 1)) = 0.5
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
            float _PulseSpeed;
            float _PulseMin;
            float _PulseMax;
            float _SoftEdge;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv - 0.5;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = length(i.uv) * 2.0;

                // Radial gradient falloff
                float radial = 1.0 - smoothstep(0.0, _SoftEdge, dist);
                radial *= radial;

                // Sinusoidal pulse
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float intensity = lerp(_PulseMin, _PulseMax, pulse);

                fixed4 col = _Color * radial * intensity;

                return col;
            }
            ENDCG
        }
    }

    FallBack Off
}
