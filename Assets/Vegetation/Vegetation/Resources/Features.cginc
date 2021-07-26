
#ifndef VEGETATION_FEATURES
#define VEGETATION_FEATURES


#include "Assets/Vegetation/HLSLUtilities/NoiseUtils.cginc"

float ForestZoneInfluence(float2 worldPos)
{
	return saturate(simplexfbm2D(worldPos, 3, 0.001, 2, 0.5, 1));
}



#endif