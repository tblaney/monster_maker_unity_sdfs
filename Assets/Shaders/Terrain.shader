Shader "Custom/Terrain"
{
    Properties
    {
        _ColorRTop ("Top Color R", Color) = (1,1,1,1)
        _ColorGTop ("Top Color G", Color) = (1,1,1,1)
        _ColorBTop ("Top Color B", Color) = (1,1,1,1)
        _ColorBaseTop ("Top Color Base", Color) = (1,1,1,1)
        _ColorTopAlt ("Top Color Alt", Range(0.0,2.0)) = 1.2

        [Space(10)]
        _ColorSide ("Side Color", Color) = (1,1,1,1)
        _ColorSideAlt ("Side Color Alt", Range(0.0,2.0)) = 1.2
        _SideThreshold ("Side Threshold", Range(0.0,1.0)) = 0.6
        _SideNoiseThreshold ("Side Noise Threshold", Range(0.0,1.0)) = 0.6
        _SideNoiseScale ("Side Noise Scale", Range(0.0,10.0)) = 1.0
        
        [Space(10)]
        _TopNoiseThreshold ("Top Noise Threshold", Range(0.0,1.0)) = 0.6
        _TopNoiseScale ("Top Noise Scale", Range(0.0,10.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert addshadow

        #include "Noise/ClassicNoise3D.cginc"

        struct Input
        {
            float4 color : COLOR;
            float3 worldPos;
            INTERNAL_DATA
            float3 localPos;
            float3 worldNormal;
        };

        fixed4 _ColorBTop;
        fixed4 _ColorGTop;
        fixed4 _ColorRTop;
        fixed4 _ColorBaseTop;
        float _ColorTopAlt;

        fixed4 _ColorSide;
        float _ColorSideAlt;

        float _SideThreshold;
        float _SideNoiseThreshold;
        float _SideNoiseScale;
        
        float _TopNoiseThreshold;
        float _TopNoiseScale;

        // Vertex modification function
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            // Compute world position
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            // Pass local position
            o.localPos = v.vertex.xyz;

            o.color = v.color; // Pass vertex color to fragment shader.

            o.worldNormal = mul(unity_ObjectToWorld, v.normal);
        }

        // Surface shading function
        void surf(Input IN, inout SurfaceOutput o)
        {
            float dotVal = dot(IN.worldNormal, float3(0,1,0));
            float3 color = _ColorBaseTop.rgb;  

            if (IN.color.r > 0.9)
            {
                color = _ColorRTop.rgb;
            } else if (IN.color.g > 0.9)
            {
                color = _ColorGTop.rgb;
            } else if (IN.color.b > 0.9)
            {
                color = _ColorBTop.rgb;
            }

            if (dotVal < _SideThreshold)
            {
                color = _ColorSide.rgb;

                float noiseVal = snoise(IN.worldPos*_SideNoiseScale);
                noiseVal = abs(noiseVal);

                if (noiseVal > _SideNoiseThreshold)
                {
                    color = lerp(color, _ColorSide*_ColorSideAlt, noiseVal);
                }
            } else
            {
                float noiseVal = cnoise(IN.worldPos*_TopNoiseScale);
                noiseVal = abs(noiseVal);

                if (noiseVal > _TopNoiseThreshold)
                {
                    color = lerp(color, color*_ColorTopAlt, noiseVal);
                }
            }

            o.Albedo.rgb = color;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
