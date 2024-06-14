// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/CelTerrain"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _ColorTop ("Top Color", Color) = (1,1,1,1)
        _ColorSide ("Side Color", Color) = (1,1,1,1)
        _Threshold ("Threshold", Range(0.01, 1.0)) = 0.6
        _Steps ("Shading Steps", Float) = 3.0 // Defines the number of discrete steps in lighting
        _ShadowStrength("Shadow Strength", Range(0.0, 1.0)) = 0.8
    }
    SubShader
    {
        Pass
        {
            Tags {"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            // compile shader into multiple variants, with and without shadows
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            // shadow helper functions and macros
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                SHADOW_COORDS(1) // put shadows data into TEXCOORD1
                float3 worldNormal : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                fixed3 diff : COLOR0;
                fixed3 ambient : COLOR1;
                float4 pos : SV_POSITION;
            };


            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal,1));
                o.worldNormal = normalize(mul(v.normal, (float3x3)unity_ObjectToWorld));
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // compute shadows data
                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 _ColorTop;
            fixed4 _ColorSide;
            float _Steps;
            float _ShadowStrength;
            float _Threshold;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _ColorTop;

                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                //fixed shadow = SHADOW_ATTENUATION(i);

                float3 norm = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float nDotL = max(0, dot(norm, lightDir));

                float lighting = nDotL * atten * _LightColor0;
                lighting += i.ambient;

                float baseLight = 1.0 - _ShadowStrength;
                float toon = max(baseLight, floor(lighting * _Steps) / _Steps);

                float dotVal = dot(i.worldNormal, float3(0,1,0));
                if (dotVal < _Threshold )
                {
                    col.rgb = _ColorSide.rgb;
                }
                col.rgb *= col.rgb * toon * _LightColor0; 

                return col;
            }
            ENDCG
        }

        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
