Shader "Custom/SDFMesh"
{
    Properties
    {
        _OutlineWidth ("Outline Width", Range(0.0001, 0.1)) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" }
        LOD 100

        Cull Back

        CGINCLUDE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "SDFRaymarching.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 rayDir : TEXCOORD0;
                float3 rayOrigin : TEXCOORD1;
            };

            v2f vertBase (appdata v)
            {
                v2f o;

                // get world position of vertex
                // using float4(v.vertex.xyz, 1.0) instead of v.vertex to match Unity's code
                
                float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));

                float3 worldSpaceViewPos = UNITY_MATRIX_I_V._m03_m13_m23;
                float3 worldSpaceViewForward = -UNITY_MATRIX_I_V._m02_m12_m22;
                // originally the perspective ray dir
                float3 worldCameraToPos = worldPos - worldSpaceViewPos;
                // orthographic ray dir
                float3 worldRayDir = worldSpaceViewForward * dot(worldCameraToPos, worldSpaceViewForward);
                // orthographic ray origin
                float3 worldRayOrigin = worldPos - worldRayDir;

                // calculate and world space ray direction and origin for interpolation
                o.rayDir = worldRayDir;
                o.rayOrigin = worldRayOrigin;

                o.pos = UnityWorldToClipPos(worldPos);

                return o;
            }
        ENDCG

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            
            CGPROGRAM
            #pragma vertex vertBase
            #pragma fragment fragBase

            // needed for conservative depth and sample modifier
            #pragma target 5.0

            #pragma multi_compile_fwdbase
            // skip support for any kind of baked lighting
            #pragma skip_variants LIGHTMAP_ON DYNAMICLIGHTMAP_ON DIRLIGHTMAP_COMBINED SHADOWS_SHADOWMASK
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma shader_feature_local _ _MAPPING_CUBEMAP
            #pragma shader_feature_local _ _MSAABEHAVIOUR_ALPHATOCOVERAGE _MSAABEHAVIOUR_SUPERSAMPLE

            // this shouldn't be needed as this should be handled by the multi_compile_fwdbase
            // but I couldn't get it to use this variant without this line
            // might be because we're doing vertex lights in the fragment instead of vertex shader
            #pragma multi_compile _ VERTEXLIGHT_ON

            struct shadowInput {
                SHADOW_COORDS(0)
            };

            half4 fragBase (v2f i, out float outDepth : SV_Depth) : SV_Target
            {
                // ray origin
                float3 rayOrigin = i.rayOrigin;

                // normalize ray vector
                float3 rayDir = normalize(i.rayDir);

                // ray box intersection
                Surface surface;
                float rayHit = RayMarch(rayOrigin, rayDir, surface);

                // above function returns -1 if there's no intersection
                clip(rayHit);

                if (!_Outline && surface.outline > 0.9)
                    discard;

                // calculate world space position from ray, front hit ray length, and ray origin
                float3 worldPos = rayDir * rayHit + rayOrigin;

                // world space surface normal
                float3 worldNormal = GetNormal(worldPos);

                // output modified depth
                float4 clipPos = UnityWorldToClipPos(worldPos);
                outDepth = clipPos.z / clipPos.w;

                #if !defined(UNITY_REVERSED_Z)
                    // openGL platforms need the clip space to be rescaled
                    outDepth = outDepth * 0.5 + 0.5;
                #endif

                #if defined (SHADOWS_SCREEN)
                    // setup shadow struct for screen space shadows
                    shadowInput shadowIN;
                #if defined(UNITY_NO_SCREENSPACE_SHADOWS)
                    // mobile directional shadow
                    shadowIN._ShadowCoord = mul(unity_WorldToShadow[0], float4(worldPos, 1.0));
                #else
                    // screen space directional shadow
                    shadowIN._ShadowCoord = ComputeScreenPos(clipPos);
                #endif // UNITY_NO_SCREENSPACE_SHADOWS
                #else
                    // no shadow, or no directional shadow
                    float shadowIN = 0;
                #endif // SHADOWS_SCREEN

                    // basic lighting
                    half3 worldLightDir = UnityWorldSpaceLightDir(worldPos);
                    half ndotl = saturate(dot(worldNormal, worldLightDir));

                    // get shadow, attenuation, and cookie
                    UNITY_LIGHT_ATTENUATION(atten, shadowIN, worldPos);

                    // per pixel lighting
                    half3 lighting = _LightColor0 * ndotl * atten;

                #if defined(UNITY_SHOULD_SAMPLE_SH)
                    // ambient lighting
                    half3 ambient = ShadeSH9(float4(worldNormal, 1));
                    lighting += ambient;

                #if defined(VERTEXLIGHT_ON)
                    // "per vertex" non-important lights
                    half3 vertexLighting = Shade4PointLights(
                    unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                    unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                    unity_4LightAtten0, worldPos, worldNormal);

                    lighting += vertexLighting;
                #endif // VERTEXLIGHT_ON
                #endif // UNITY_SHOULD_SAMPLE_SH

                //fixed shadow = UNITY_SHADOW_ATTENUATION(shadowIN, worldPos);

                // toonify
                int steps = 4;
                float baseLight = 0.2; // This ensures no value goes to absolute black
                float toon = max(baseLight, floor(lighting * steps) / steps);

                //float3 color = surface.diffuse*toon*max(baseLight, atten);
                float3 color = surface.diffuse*toon*_LightColor0;

                if (_Outline)
                    color = lerp(color, float3(0, 0, 0), surface.outline);

                return half4(color, 1.0);
            }
            
            ENDCG
        }
        
        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual
            CGPROGRAM
            #pragma vertex vertBase
            #pragma fragment fragShadow
            #pragma multi_compile_shadowcaster

            fixed4 fragShadow (v2f i,
            out float outDepth : SV_Depth
            ) : SV_Target
            {
            // ray origin
            float3 rayOrigin = i.rayOrigin;
            // normalize ray vector
            float3 rayDir = normalize(i.rayDir);

            // ray box intersection
            Surface surface;
            float rayHit = RayMarch(rayOrigin, rayDir, surface);

            // above function returns -1 if there's no intersection
            clip(rayHit);

            if (!_Outline && surface.outline > 0.9)
                discard;

            // calculate object space position
            float3 worldPos = rayDir * rayHit + rayOrigin;
            float3 objectSpacePos = mul(unity_WorldToObject, float4(worldPos, 1.0));
            // output modified depth
            // yes, we pass in objectSpacePos as both arguments
            // second one is for the object space normal, which in this case
            // is the normalized position, but the function transforms it 
            // into world space and normalizes it so we don't have to
            float4 clipPos = UnityClipSpaceShadowCasterPos(objectSpacePos, objectSpacePos);
            clipPos = UnityApplyLinearShadowBias(clipPos);
            outDepth = clipPos.z / clipPos.w;
            return 0;
            }
            ENDCG
        }
    }
}