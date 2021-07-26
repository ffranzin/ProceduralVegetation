
#ifndef VEGETATION_COMMON_FUNCTIONS
#define VEGETATION_COMMON_FUNCTIONS

#include "VegetationCommonVariables.cginc"
#include "Assets/Vegetation/HLSLUtilities/MathUtils.cginc"

float4x4 GetRotationMatrixYAxis(float radAngle)
{
	float4x4 rotationMatrix;

	float cosAngle = cos(radAngle);
	float sinAngle = sin(radAngle);

	rotationMatrix._11_21_31_41 = float4(cosAngle, 0, -sinAngle, 0);
	rotationMatrix._12_22_32_42 = float4(0, 1, 0, 0);
	rotationMatrix._13_23_33_43 = float4(sinAngle, 0, cosAngle, 0);
	rotationMatrix._14_24_34_44 = float4(0,0,0,1);
	
	return rotationMatrix;
}


float4x4 GetRotationMatrixXAxis(float angle)
{
	float4x4 rotationMatrix;

	angle = radians(angle);

	float cosAngle = cos(angle);
	float sinAngle = sin(angle);

	rotationMatrix._11_21_31_41 = float4(1, 0, 0, 0);
	rotationMatrix._12_22_32_42 = float4(0, cosAngle, -sinAngle, 0);
	rotationMatrix._13_23_33_43 = float4(0, sinAngle, cosAngle, 0);
	rotationMatrix._14_24_34_44 = float4(0,0,0,1);
	
	return rotationMatrix;
}



inline int IndexOnLODBufferDescriptor(int modelIndex, int LODLevel)///the same for geometrys and shadows buffer
{
	return modelIndex * MAX_LOD_LEVELS + LODLevel;
}


VegetationTransform GetVegetationInstanceTransform(int instanceID) 
{
	int index = IndexOnLODBufferDescriptor(_InstanceModelIndex, _InstanceLODLevel);

	return _PlantsPositionsBufferLODGeometry[instanceID + _LODBufferDescriptor[index].firstIndexOnLODBuffer];
}


inline bool IsInvalidPosition(float2 pos)
{
	return pos.x == INFINITY || pos.y == INFINITY;
}


inline float EncodeEnvironmentElementData(int environmentElementClass, float normalizedScale)
{
	normalizedScale = clamp(normalizedScale, 0, 0.999);

	return environmentElementClass + normalizedScale;
}

inline float DecodeEnvironmentElementScale(float encodedData)
{
	return frac(encodedData);
}

inline float Decode_EnvironmentElementIndex(float encodedData)
{
	return (int)(encodedData);
}

#endif