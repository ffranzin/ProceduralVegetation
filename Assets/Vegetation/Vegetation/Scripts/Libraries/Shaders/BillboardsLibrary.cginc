#ifndef BILLBOARDS_LIBRARY
#define BILLBOARDS_LIBRARY

#include "Assets/Vegetation/HLSLUtilities/MathUtils.cginc"

struct BillboardRect
{
	float2 min;
	float2 max;
};
StructuredBuffer<BillboardRect> _BillboardRects;


inline float2 AdjustTexCoord(float2 uv, int textureIndex)
{
	BillboardRect minMax = _BillboardRects[textureIndex];

	return lerp(minMax.min, minMax.max, uv);
}
#endif