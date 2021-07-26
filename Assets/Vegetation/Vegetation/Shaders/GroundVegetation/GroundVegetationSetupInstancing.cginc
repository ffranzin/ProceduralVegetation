#ifndef GROUND_VEGETATION_SETUP_INSTANCING
#define GROUND_VEGETATION_SETUP_INSTANCING

#include "Assets/Vegetation/Vegetation/Resources/VegetationCommonVariables.cginc"
#include "Assets/Vegetation/Vegetation/Resources/VegetationCommonFunctions.cginc"
#include "Assets/Vegetation/HLSLUtilities/NoiseUtils.cginc"

#include "Assets/Vegetation/HLSLUtilities/QuaternionUtils.cginc"
#include "GroundVegetationWindWave.cginc"
#include "Assets/Vegetation/Vegetation/Scripts/Libraries/Shaders/PlantsLibrary.cginc"
#include "Assets/Vegetation/Vegetation/Scripts/Libraries/Shaders/BillboardsLibrary.cginc"

StructuredBuffer<int> _TexturesIndexOnBillboardsLibrary;
int _BillboardsCount;
int billboardIndex;


void setup()
{
	#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED 
		VegetationTransform transform = GetVegetationInstanceTransform(unity_InstanceID);

		float distToCamera = length(_WorldSpaceCameraPos - transform.position);
		float distToCameraNormalized = distToCamera / _PlantsLibrary[_InstanceModelIndex].geometryCullDistance;

		//////////////////WIND PARAMETERS/////////////////////////
		float2 wind_uv = (transform.position % 1024) / 1024 * 0.5;
		_windAmount *= (1 - distToCameraNormalized) * max(0.5, tex2Dlod(_windWave, float4(wind_uv, 1, 1)).r);
		_windSpeed *= _Time;
		//////////////////////////////////////////////////////////

		unity_ObjectToWorld._11_21_31_41 = float4(transform.scale, 0, 0, 0);
		unity_ObjectToWorld._12_22_32_42 = float4(0, transform.scale, 0, 0);
		unity_ObjectToWorld._13_23_33_43 = float4(0, 0, transform.scale, 0);
		unity_ObjectToWorld._14_24_34_44 = float4(transform.position, 1);

		unity_ObjectToWorld = mul(unity_ObjectToWorld, GetRotationMatrixYAxis(transform.position.x * transform.position.z));

		unity_WorldToObject = unity_ObjectToWorld;
		unity_WorldToObject._14_24_34 *= -1;
		unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;

		UpdateWind(transform.position.xz, (1 - distToCameraNormalized));
		billboardIndex = CustomRand(transform.position.xz, 0, _BillboardsCount);
	#endif
}

#endif


