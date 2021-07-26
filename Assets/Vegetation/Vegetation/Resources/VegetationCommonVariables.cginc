// Upgrade NOTE: commented out 'float3 _WorldSpaceCameraPos', a built-in variable

#ifndef PLANTS_COMMON_VARIABLES
#define PLANTS_COMMON_VARIABLES

#include "VegetationStructures.cginc"

StructuredBuffer<float> _LinearAdaptabilityCurves;

RWStructuredBuffer<CustomArgBuffer> _GlobalArgsBuffer;

RWStructuredBuffer<LODBufferDescriptor> _LODBufferDescriptor_RW; // RENOMEAR _LODBUFFERDATA
StructuredBuffer<LODBufferDescriptor> _LODBufferDescriptor;

RWStructuredBuffer<LODBufferDescriptor> _LODBufferDescriptor_RW_shadow;
StructuredBuffer<LODBufferDescriptor> _LODBufferDescriptor_shadow;

RWTexture2D<float2> _PlantsPositionsBuffer_RW;
Texture2D<float2> _PlantsPositionsBuffer;

RWStructuredBuffer<VegetationTransform> _PlantsPositionsBufferLODGeometry_RW;
StructuredBuffer<VegetationTransform> _PlantsPositionsBufferLODGeometry;

RWStructuredBuffer<VegetationTransform> _PlantsPositionsBufferLODShadow_RW;
StructuredBuffer<VegetationTransform> _PlantsPositionsBufferLODShadow;

Texture2D<float> _HeightmapAtlas;

StructuredBuffer<float4> _ElementsPositionsAtlasPages;//encoding -- XY:min Z:size //REMOVER
StructuredBuffer<float4> _QuadTreeBoundNodes;//encoding -- XY:min ZW:max // REMOVER
StructuredBuffer<float4> _HeightmapAtlasPages;//encoding -- XY:min Z:size // REMOVER

int _TexturesCount;


////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////// COLLECT POSITIONS //////////////////////////////////
AppendStructuredBuffer<float2> _CollectPositionsBuffer;
float4 _AreaMinMax;
float4 _AreaMinMaxOriginal;
float _PlacementDistance;
int _VegetationCoverLimit;
////////////////////////////////////////////////////////////////////////////////////

float4 _FrustumPlanesNormal[6];
bool _ActiveFrustumCulling;

int _InstanceModelIndex;
int _InstanceLODLevel;

#endif