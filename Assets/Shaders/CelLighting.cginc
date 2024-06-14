// DEFINES.
#define PI (3.1415926536)

float _ShadowStrength;
float _Steps;
fixed4 _ShadowColor;
fixed4 _Color;

inline fixed4 LightingCel(SurfaceOutput s, fixed3 lightDir, fixed atten)
{
    // Calculate the dot product of the normal and the light direction
    fixed ndotl = saturate(dot(s.Normal, lightDir));

    half3 lighting = _LightColor0 * ndotl * atten;

    float toon = max(1.0 -  _ShadowStrength, floor(lighting * _Steps) / _Steps);

    return fixed4(s.Albedo*toon*_LightColor0, s.Alpha);
}
