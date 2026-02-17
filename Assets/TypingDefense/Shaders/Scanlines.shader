Shader "TypingDefense/Scanlines"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScanlineCount ("Scanline Count", Float) = 300
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.15
        _ScanlineSpeed ("Scanline Scroll Speed", Float) = 0.5
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.4
        _VignetteRadius ("Vignette Radius", Range(0, 2)) = 0.8
        _FlickerIntensity ("Flicker Intensity", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZTest Always Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _ScanlineCount;
            float _ScanlineIntensity;
            float _ScanlineSpeed;
            float _VignetteIntensity;
            float _VignetteRadius;
            float _FlickerIntensity;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Scanlines - scrolling horizontal lines
                float scanline = sin((i.uv.y + _Time.x * _ScanlineSpeed) * _ScanlineCount * 3.14159) * 0.5 + 0.5;
                scanline = 1.0 - scanline * _ScanlineIntensity;
                col.rgb *= scanline;

                // Subtle flicker
                float flicker = 1.0 - _FlickerIntensity * sin(_Time.z * 7.0 + i.uv.y * 2.0);
                col.rgb *= flicker;

                // Vignette
                float2 center = i.uv - 0.5;
                float dist = length(center);
                float vignette = 1.0 - smoothstep(_VignetteRadius, _VignetteRadius + 0.5, dist) * _VignetteIntensity;
                col.rgb *= vignette;

                return col;
            }
            ENDCG
        }
    }
}
