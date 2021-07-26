#ifndef LAYERS_LIBRARY
#define LAYERS_LIBRARY

#include "Assets/Vegetation/Vegetation/Resources/VegetationGlobalDefines.cginc"

struct VegetationLayerDescriptor
{
	/////////////////////// GLOBAL PARAMETERS //////////////////////
	int vegetationCover;
	float placementDistance;
	float selfDistance;
	float worldObstacleRadius;
	////////////////////////////////////////////////////////////////

	//////////////////// DISTRIBUTION PARAMETERS ////////////////////
	int octaves;
	float frequency;
	float amplitude;
	float gain;
	float lacunarity;
	////////////////////////////////////////////////////////////////

	float adaptability[FEATURES_COUNT];
	int discretizedPlantsIndex[BUFFER_SIZE_RAM_VRAM];
};

StructuredBuffer<VegetationLayerDescriptor> _VegetationLayersLibrary;
int _VegetationLayersLibraryCount;

#endif