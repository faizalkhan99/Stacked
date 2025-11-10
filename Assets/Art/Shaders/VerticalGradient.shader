Shader "Unlit/VerticalGradient"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.5, 0.8, 1.0, 1)
        _BottomColor ("Bottom Color", Color) = (0.1, 0.2, 0.5, 1)
        _GradientHeight ("Gradient Height", Range(0, 1)) = 0.5
        _BlendStrength ("Blend Strength", Range(0.01, 5)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Uniforms (must match the script PropertyToID)
            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _GradientHeight;
            float _BlendStrength;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Direction from camera center to skybox surface
                float3 dir = normalize(i.worldPos);

                // Base vertical factor (0 = bottom, 1 = top)
                float t = saturate(dir.y * 0.5 + 0.5);

                // Apply height and blending controls
                t = saturate((t - _GradientHeight) * _BlendStrength + _GradientHeight);

                // Interpolate between bottom and top colors
                return lerp(_BottomColor, _TopColor, t);
            }
            ENDCG
        }
    }

    Fallback Off
}
