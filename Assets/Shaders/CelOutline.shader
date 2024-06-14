// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/CelOutline"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Steps ("Shading Steps", Float) = 3.0 // Defines the number of discrete steps in lighting
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
		_OutlineWidth("Outline Width", Range(0.0, 1.0)) = 0.02
        _ShadowStrength("Shadow Strength", Range(0.0, 1.0)) = 0.8
    }
    SubShader
    {
        //First outline
		Pass{
			Tags{ "Queue" = "Geometry" }
			Cull Front
			CGPROGRAM

            struct appdata 
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };

			struct v2f {
				float4 pos : SV_POSITION;
			};

			#pragma vertex vert
			#pragma fragment frag

            float _OutlineColor;
            float _OutlineWidth;

			v2f vert(appdata v)
            {
				appdata original = v;

				float3 scaleDir = normalize(v.vertex.xyz - float4(0,0,0,1));
                v.vertex.xyz += scaleDir * _OutlineWidth;

                float3 worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;

				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			half4 frag(v2f i) : COLOR
            {
				float4 color = _OutlineColor;
				color.a = 1;
				return color;
			}

			ENDCG
		}

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

            sampler2D _MainTex;
            fixed4 _Color;
            float _Steps;
            float _ShadowStrength;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                //fixed shadow = SHADOW_ATTENUATION(i);

                float3 norm = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float nDotL = max(0, dot(norm, lightDir));

                float lighting = nDotL * atten * _LightColor0;
                lighting += i.ambient;

                float baseLight = 1.0 - _ShadowStrength;
                float toon = max(baseLight, floor(lighting * _Steps) / _Steps);

                col.rgb *= _Color.rgb * toon * _LightColor0; 
                return col;
            }
            ENDCG
        }

        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
