#ifndef SPEEDTREE_SETUP_INSTANCING
#define SPEEDTREE_SETUP_INSTANCING

	#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED 
		
	#include "Assets/Vegetation/Vegetation/Resources/VegetationCommonVariables.cginc"
	#include "Assets/Vegetation/Vegetation/Resources/VegetationCommonFunctions.cginc"

	void setup()
	{
		VegetationTransform transform = GetVegetationInstanceTransform(unity_InstanceID);

		unity_ObjectToWorld._11_21_31_41 = float4(transform.scale, 0, 0, 0);
		unity_ObjectToWorld._12_22_32_42 = float4(0, transform.scale, 0, 0);
		unity_ObjectToWorld._13_23_33_43 = float4(0, 0, transform.scale, 0);
		unity_ObjectToWorld._14_24_34_44 = float4(transform.position, 1);

		unity_ObjectToWorld = mul(unity_ObjectToWorld, GetRotationMatrixYAxis(transform.position.x * transform.position.z));

		unity_WorldToObject = unity_ObjectToWorld;
		unity_WorldToObject._14_24_34 *= -1;
		unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
	}
	#endif
#endif
