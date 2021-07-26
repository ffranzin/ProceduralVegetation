#ifndef MATH_UTILS
#define MATH_UTILS

#define INFINITY 9999999 
#define NaN 0.0/0.0


inline int CustomRandInt(float2 seed, int min, int max)//max isnt inclusive
{
	seed = (seed % 1000);

	return (int)lerp(min, max, frac(seed.x * seed.y * 1.79123));
}


inline float CustomRand(float2 seed, float min, float max)
{
	seed = (seed % 1000) * float2(1.989, 1.233);

	return (frac(seed.x * seed.y) * (max - min) + min);
}


bool RandomBoolean(float2 seed)
{
	return frac(seed.x * seed.y * 1.0231) > 0.5;
}


inline float Remap(float x, float in_min, float in_max, float out_min, float out_max)
{
    return lerp(out_min, out_max, /*ratio*/saturate((x - in_min) / (in_max - in_min)));
}


inline float sqr(float x)
{
    return x * x;
}

inline float lengthSquared(const float2 point1Seg, const float2 point2Seg)
{
    return sqr(point1Seg.x - point2Seg.x) + sqr(point1Seg.y - point2Seg.y);
}


inline float PerpDot(float2 a, float2 b)
{
    return (a.y * b.x) - (a.x * b.y);
}


////////////////////EM TEORIA AMBAS FAZEM A MESMA COISA -- VERIFICAR E REMOVER A MENOS EFICIENTE
inline float DistancePointToLineSegment(float2 lineP0, float2 lineP1, float2 p, out float t)
{
    float2 pa = p - lineP0;
    float2 ba = lineP1 - lineP0;
    t = saturate(dot(pa, ba) / dot(ba, ba));
    return length(pa - ba * t);
}


//http://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
inline float DistancePointSegment(const float2 refPoint, const float2 point1Seg, const float2 point2Seg, out float tt)
{
    tt = 1;

    const float l2 = lengthSquared(point1Seg, point2Seg);
    if (l2 == 0.0f) 
        return distance(refPoint, point1Seg);

    const float t = max(0.0f, min(1.0f, dot(refPoint - point1Seg, point2Seg - point1Seg) / l2));
    tt = t;
    const float2 projection = point1Seg + t * (point2Seg - point1Seg);
    return distance(refPoint, projection);
}
//////////////////////////////////////////////////////////////////////////////////////////////////////////////


inline bool IsPointInLeftSize(float2 lineP0, float2 lineP1, float2 p)
{
    return ((lineP1.x - lineP0.x) * (p.y - lineP0.y) - (lineP1.y - lineP0.y) * (p.x - lineP0.x)) > 0;
}


inline bool IsPointInsideRect(float2 boundMin, float2 boundMax, float2 p)
{
	return p.x >= boundMin.x && p.x <= boundMax.x && p.y >= boundMin.y && p.y <= boundMax.y;
}


inline float RayPlaneIntersect(float3 rayOrigin, float3 rayDir, float3 planeOrigin, float3 planeNormal)
{
	float d = dot(rayDir, planeNormal);

	if (abs(d) > 0.0001)
	{
		float t = ( dot(planeNormal, planeOrigin - rayOrigin)) / dot(planeNormal, rayDir);

        if (t >= 0)
		{
			return t;
		}
    }
        
    return INFINITY;
}



inline float RaySphereIntersect(float3 rayOrigin, float3 rayDir, float3 sphereCenter, float sphereRadius)
{
	if (dot(rayDir, normalize(sphereCenter - rayOrigin)) < 0.00001)
	{
		return INFINITY;
	}
		
	float3 dir = rayOrigin - sphereCenter;
	float b = dot(rayDir, dir);
	float c = dot(dir, dir) - sphereRadius * sphereRadius;

	float delta = b * b - c;

	if (delta < 0.00001)
	{
		return INFINITY;
	}
		
	delta = sqrt(delta);

	float r1 = -b - delta;
	float r2 = -b + delta;

	if (r1 < 0 && r2 > 0) return r2;
	if (r1 > 0 && r2 < 0) return r1;
        
	return min(r1, r2);
}



inline bool PointInsideArbitraryQuad(float3 p, float3 vertA, float3 vertB, float3 vertC, float3 vertD)
{
    float3 qAresta1 = normalize(vertB - vertA);
    float3 qAresta2 = normalize(vertC - vertB);
    float3 qAresta3 = normalize(vertD - vertC);
    float3 qAresta4 = normalize(vertA - vertD);

    float3 d1 = normalize(p - vertA);
    float3 d2 = normalize(p - vertB);
    float3 d3 = normalize(p - vertC);
    float3 d4 = normalize(p - vertD);
        
    return dot(qAresta1, d1) > 0 && dot(qAresta2, d2) > 0 && dot(qAresta3, d3) > 0 && dot(qAresta4, d4) > 0;
}


#endif