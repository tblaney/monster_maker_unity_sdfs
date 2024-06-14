#include "UnityCG.cginc"

#include "Packages/com.quizandpuzzle.shaderlib/Runtime/sdf.cginc"
#include "Packages/com.quizandpuzzle.shaderlib/Runtime/math.cginc"

#define SURF_DIST 0.001
#define MAX_STEPS 40
#define MAX_DIST 100.0

bool _Outline;
float _OutlineWidth;
bool _Blend;
half3 _LightColor0;

struct Surface
{
    float distanceToSurface;
    float3 diffuse;
    float outline;
};

struct Shape
{
    float3 position;
    float3 rotation;
    float3 scale;
    float3 diffuse;
    float radius;
    int shapeType;
    int blendMode;
    float blendStrength;
};

StructuredBuffer<Shape> shapes;
StructuredBuffer<float4> operationValues;
int numShapes;

float3x3 rotateX(float theta)
{
    float c = cos(theta);
    float s = sin(theta);
    return float3x3(
        float3(1, 0, 0),
        float3(0, c, -s),
        float3(0, s, c)
    );
}

float3x3 rotateY(float theta)
{
    float c = cos(theta);
    float s = sin(theta);
    return float3x3(
        float3(c, 0, s),
        float3(0, 1, 0),
        float3(-s, 0, c)
    );
}

float3x3 rotateZ(float theta)
{
    float c = cos(theta);
    float s = sin(theta);
    return float3x3(
        float3(c, -s, 0),
        float3(s, c, 0),
        float3(0, 0, 1)
    );
}

float sdCylinder(float3 p, float h, float r)
{
    float2 d = abs(float2(length(p.xz), p.y)) - float2(r, h);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

float sdAnisotropicCylinder(float3 p, float3 scale, float h, float r)
{
    // Apply scaling to the position vector
    p.xz = p.xz / scale.xz; // Scale radially
    p.y = p.y / scale.y;    // Scale axially

    // Compute the distance from the scaled point to the cylinder surface
    float2 d = abs(float2(length(p.xz), p.y)) - float2(r, h);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

#define MAXMANDELBROTDIST 1.5
#define MANDELBROTSTEPS 64

float2 DE(float3 pos)
{
    float Power = 7.0 + 20.0 * (sin(_Time / 10.0) + 1.0);
    float3 z = pos;
    float dr = 1.0;
    float r = 0.0;
    for (int i = 0; i < MANDELBROTSTEPS; i++)
    {
        r = length(z);
        if (r > MAXMANDELBROTDIST) break;

        // convert to polar coordinates
        float theta = acos(z.z / r);
        float phi = atan2(z.y, z.x);
        dr = pow(r, Power - 1.0) * Power * dr + 1.0;

        // scale and rotate the point
        float zr = pow(r, Power);
        theta = theta * Power;
        phi = phi * Power;

        // convert back to cartesian coordinates
        z = zr * float3(sin(theta) * cos(phi), sin(phi) * sin(theta), cos(theta));
        z += pos;
    }
    return float2(0.9 * log(r) * r / dr, 50.0 * pow(dr, 0.128 / float(MAX_STEPS)));
}

float3 noise(in float3 x)
{
    float3 p = floor(x);
    float3 f = frac(x);
    f = f * f * (3.0 - 2.0 * f);

    return lerp(lerp(lerp(hash33(p + float3(0, 0, 0)),
                          hash33(p + float3(1, 0, 0)), f.x),
                     lerp(hash33(p + float3(0, 1, 0)),
                          hash33(p + float3(1, 1, 0)), f.x), f.y),
                lerp(lerp(hash33(p + float3(0, 0, 1)),
                          hash33(p + float3(1, 0, 1)), f.x),
                     lerp(hash33(p + float3(0, 1, 1)),
                          hash33(p + float3(1, 1, 1)), f.x), f.y), f.z);
}

float3x3 m = float3x3(0.00, 0.80, 0.60,
                      -0.80, 0.36, -0.48,
                      -0.60, -0.48, 0.64);


float3 fbm(float3 q)
{
    float3 f = 0.5000 * noise(q);
    q = mul(m, q) * 2.01;
    f += 0.2500 * noise(q);
    q = mul(m, q) * 2.02;
    f += 0.1250 * noise(q);
    q = mul(m, q) * 2.03;
    f += 0.0625 * noise(q);
    q = mul(m, q) * 2.04;
    #if 0
    f += 0.03125 * noise(q);
    q = mul(m, q) * 2.05;
    f += 0.015625 * noise(q);
    q = mul(m, q) * 2.06;
    f += 0.0078125 * noise(q);
    q = mul(m, q) * 2.07;
    f += 0.00390625 * noise(q);
    q = mul(m, q) * 2.08;
    #endif
    return float3(f);
}

float sdRoundedBox(float3 p, float3 b, float r) {
    float3 q = abs(p) - b;
    return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0) - r;
}

float sdAnisotropicSphere(float3 p, float3 scale, float s)
{
    p = p / scale; // Scale the point p
    return length(p) - s;
}

float GetShapeDistance(float3 p, Shape shape)
{
    if (shape.shapeType == 0)
    {
        float d1 = sdAnisotropicSphere(p, float3(shape.scale.x,shape.scale.y,shape.scale.z), shape.radius);
        return d1;
    }
    else if (shape.shapeType == 1)
    {
        //return sdBox(p);
        //return sdRoundedBox(p, shape.scale, shape.corners);
        return sdRoundBox(p, shape.scale, shape.radius);
        //return sdBox(p, shape.scale);
    }
    else if (shape.shapeType == 2)
    {
        return sdTorus(p, shape.scale);
    }
    else if (shape.shapeType == 3)
    {
        return sdPlane(p, float3(0, 1, 0), 0.1);
    }
    else if (shape.shapeType == 4)
    {
        return DE(p * .2).x;
    }
    else if (shape.shapeType == 5)
    {
        return fbm(p);
    }
    else if (shape.shapeType == 6)
    {
        return sdCylinder(p, shape.scale.y, shape.radius);
    }

    return MAX_DIST;
}

float3 Blend(float a, float b, float3 colA, float3 colB, float k)
{
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    float3 blendCol = lerp(colB, colA, h);
    return float3(blendCol);
}

Surface Union(Surface obj1, Surface obj2)
{
    Surface surface = obj1;
    if (obj2.distanceToSurface < obj1.distanceToSurface)
    {
        surface = obj2;
    }
    surface.distanceToSurface = opUnion(obj2.distanceToSurface, obj1.distanceToSurface);
    return surface;
}

//float smoothUnion(float d1, float d2, float k) {
//    float res = exp(-k * d1) + exp(-k * d2);
//    return -log(res) / k;
//}

float smoothUnion(float d1, float d2, float k)
{
    float h = clamp(0.5 + 0.5*(d2-d1)/k, 0.0, 1.0);
    return lerp(d2, d1, h) - k*h*(1.0-h);
}

Surface SmoothUnion(Surface obj1, Surface obj2, float k)
{
    float d1 = obj1.distanceToSurface;
    float d2 = obj2.distanceToSurface;
    float smoothDistance = smoothUnion(d1, d2, k);
    
    // Calculate interpolation factor
    float t = (smoothDistance - d2) / (d1 - d2);
    t = clamp(t, 0.0, 1.0); // Ensuring t remains within [0,1]

    Surface surface;
    surface.distanceToSurface = smoothDistance;
    
    // Smoother color interpolation
    float h = 0.5 + 0.5 * (d1 - d2) / k; // Adjusting blend factor based on the smooth step
    h = clamp(h, 0.0, 1.0);
    
    if (_Blend)
    {
        //surface.diffuse = lerp(obj1.diffuse, obj2.diffuse, h); // Using mix for color blending
        surface.diffuse = Blend(d1,d2,obj1.diffuse,obj2.diffuse,k);
    }
    else
    {
        if (obj2.distanceToSurface < obj1.distanceToSurface)
        {
            surface.diffuse = obj2.diffuse;
        } else
        {
            surface.diffuse = obj1.diffuse;
        }
    }

    return surface;
}


Surface Intersection(Surface obj1, Surface obj2)
{
    Surface surface = obj1;
    if (obj2.distanceToSurface > obj1.distanceToSurface)
    {
        surface = obj2;
    }
    surface.distanceToSurface = opIntersection(obj2.distanceToSurface, obj1.distanceToSurface);
    return surface;
}

Surface SmoothIntersection(Surface obj1, Surface obj2, float k)
{
    Surface surface = obj1;
    if (obj2.distanceToSurface > obj1.distanceToSurface)
    {
        surface = obj2;
    }
    surface.distanceToSurface = opSmoothIntersection(obj2.distanceToSurface, obj1.distanceToSurface, k);
    return surface;
}

Surface Subtraction(Surface obj1, Surface obj2)
{
    Surface surface = obj2;
    if (-obj2.distanceToSurface > obj1.distanceToSurface)
    {
        surface = obj1;
    }
    surface.distanceToSurface = opSubtraction(obj1.distanceToSurface, obj2.distanceToSurface);
    return surface;
}

Surface SmoothSubtraction(Surface obj1, Surface obj2, float k)
{
    Surface surface = obj2;
    if (-obj2.distanceToSurface > obj1.distanceToSurface)
    {
        surface = obj1;
    }
    surface.distanceToSurface = opSmoothSubtraction(obj1.distanceToSurface, obj2.distanceToSurface, k);
    return surface;
}

Surface Blend(Surface s1, Surface s2, float blendStrength, int blendMode)
{
    if (blendMode == 0)
    {
        return Union(s1, s2);
    }
    if (blendMode == 1)
    {
        return SmoothUnion(s1, s2, blendStrength);
    }
    if (blendMode == 2)
    {
        return Subtraction(s1, s2);
    }
    if (blendMode == 3)
    {
        return SmoothSubtraction(s1, s2, blendStrength);
    }
    if (blendMode == 4)
    {
        return Union(SmoothSubtraction(s1, s2, blendStrength), s1);
    }
    if (blendMode == 5)
    {
        return Intersection(s1, s2);
    }
    if (blendMode == 6)
    {
        return SmoothIntersection(s1, s2, blendStrength);
    }

    return s1;
}

Surface Scene(float3 p)
{
    Surface surface;
    surface.distanceToSurface = MAX_DIST;
    surface.diffuse = 1.;
    int valueIndex = 0;
    for (int i = 0; i < numShapes; i++)
    {
        Shape shape = shapes[i];
        Surface shapeSurf;
        float3x3 rotation = mul(mul(rotateX(-shape.rotation.x), rotateY(-shape.rotation.y)),
                                rotateZ(-shape.rotation.z));

        float3 position = p;
        position = mul(rotation, (position - shape.position));

        //position = ApplyPositionOperations(position, rotation, shape, valueIndex);
        shapeSurf.distanceToSurface = GetShapeDistance(position, shape);
        //shapeSurf.distanceToSurface = ApplyDistanceOperations(position, shapeSurf.distanceToSurface, shape, valueIndex);

        shapeSurf.diffuse = shape.diffuse;
        surface = Blend(surface, shapeSurf, shape.blendStrength, shape.blendMode);
        
        valueIndex = i;
    }

    return surface;
}

float3 GetNormal(float3 surfPoint)
{
    float epsilon = 0.0001;
    float centerDistance = Scene(surfPoint).distanceToSurface;
    float xDistance = Scene(surfPoint + float3(epsilon, 0, 0)).distanceToSurface;
    float yDistance = Scene(surfPoint + float3(0, epsilon, 0)).distanceToSurface;
    float zDistance = Scene(surfPoint + float3(0, 0, epsilon)).distanceToSurface;
    //float3 normal = normalize(float3(xDistance, yDistance, zDistance) - centerDistance);
    //return normal;

    float3 normal = normalize(float3(
        Scene(surfPoint + float3(epsilon, 0.0, 0.0)).distanceToSurface - Scene(surfPoint).distanceToSurface,
        Scene(surfPoint + float3(0.0, epsilon, 0.0)).distanceToSurface - Scene(surfPoint).distanceToSurface,
        Scene(surfPoint + float3(0.0, 0.0, epsilon)).distanceToSurface - Scene(surfPoint).distanceToSurface
    ));

    return normal;
}

float RayMarch(float3 rayOrigin, float3 rayDirection, out Surface surface)
{
    float distanceToScene = 0;
    float nearest = MAX_DIST;
    Surface closestSurface;
    float lastSDF = MAX_DIST;
    float edge = 0.0;
    for (int i = 0; i < MAX_STEPS; i++)
    {
        float3 step = rayOrigin + rayDirection * distanceToScene;
        closestSurface = Scene(step);

        nearest = min(closestSurface.distanceToSurface, nearest);

        if ((lastSDF < _OutlineWidth) && (closestSurface.distanceToSurface > lastSDF))
        {
            edge = 1.0;
        }

        if (closestSurface.distanceToSurface < SURF_DIST || distanceToScene > MAX_DIST) break;
        distanceToScene += closestSurface.distanceToSurface;
        lastSDF = closestSurface.distanceToSurface;

        if (edge > 0.9)
            break;
    }

    closestSurface.distanceToSurface = distanceToScene;
    closestSurface.outline = edge;

    surface = closestSurface;

    float distanceOut = distanceToScene;
    if (distanceOut >= MAX_DIST && edge < 0.9)
    {
        return -1.0;
    }

    return distanceOut;
}
