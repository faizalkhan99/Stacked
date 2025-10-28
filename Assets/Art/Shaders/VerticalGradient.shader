Shader "Unlit/VerticalGradient"
{
    Properties
    {
        // These names MUST match the PropertyToID calls in ColorManager
        _TopColor ("Top Color", Color) = (0.5, 0.8, 1.0, 1) // Default Light Blue
        _BottomColor ("Bottom Color", Color) = (0.1, 0.2, 0.5, 1) // Default Dark Blue
    }
    SubShader
    {
        // Skybox specific settings
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off // Don't cull backfaces, don't write to depth buffer

        Pass
        {
            CGPROGRAM
            #pragma vertex vert // Specify vertex shader function name
            #pragma fragment frag // Specify fragment shader function name
            #include "UnityCG.cginc" // Include standard Unity shader functions

            // Make properties accessible in CGPROGRAM
            fixed4 _TopColor;
            fixed4 _BottomColor;

            // Input to the vertex shader
            struct appdata
            {
                float4 vertex : POSITION; // Vertex position in object space
            };

            // Output from vertex shader, input to fragment shader
            struct v2f
            {
                float4 vertex : SV_POSITION; // Vertex position in clip space (required)
                float3 worldPos : TEXCOORD0; // Pass world position to fragment shader
            };

            // Vertex Shader: Calculates world position
            v2f vert (appdata v)
            {
                v2f o;
                // Calculate the final screen position
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Calculate the world position of this point on the skybox sphere
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // Fragment Shader: Determines the color based on world Y position
            fixed4 frag (v2f i) : SV_Target // SV_Target specifies this is the final color output
            {
                // Get the direction from the camera (center) to this point on the skybox
                float3 viewDir = normalize(i.worldPos);

                // Use the Y component of the direction. Ranges from -1 (bottom) to +1 (top).
                // Remap this [-1, 1] range to [0, 1] for Lerp.
                float t = viewDir.y * 0.5 + 0.5;

                // Linearly interpolate between the bottom and top colors based on 't'
                fixed4 col = lerp(_BottomColor, _TopColor, t);
                return col; // Output the final color
            }
            ENDCG
        }
    }
    Fallback Off // No fallback needed for a simple skybox
}
