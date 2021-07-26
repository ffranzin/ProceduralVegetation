#ifndef PLANTS_LIBRARY
#define PLANTS_LIBRARY

struct PlantDescriptor
{
	float geometryCullDistance;
	float shadowCullDistance;
	float frustumCullingDistance;
	float minScale;
	float maxScale;

	int discreteLOD[BUFFER_SIZE_RAM_VRAM];
	int discreteLODShadow[BUFFER_SIZE_RAM_VRAM];

	int enableRender;
	int enableShadow;
	int debugLOD;
};

StructuredBuffer<PlantDescriptor> _PlantsLibrary;

#endif