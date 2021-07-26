
#ifndef VEGETATION_STRUCTURES
#define VEGETATION_STRUCTURES


#include "Assets/Vegetation/Vegetation/Resources/VegetationGlobalDefines.cginc"

struct CustomArgBuffer
{
	uint argsBuffer[5];

	int plantIndexOnLibrary;
	int LODLevel;
};


struct VegetationTransform
{
	float3 position;
	float3 rotation;
	float scale;
};


struct LODBufferDescriptor
{
	int firstIndexOnLODBuffer;
	int instancesCounter;
};

#endif