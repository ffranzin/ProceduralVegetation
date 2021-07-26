// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_STANDARD_SHADOW_INCLUDED
#define UNITY_STANDARD_SHADOW_INCLUDED

// NOTE: had to split shadow functions into separate file,
// otherwise compiler gives trouble with LIGHTING_COORDS macro (in UnityStandardCore.cginc)


#include "Assets/Vegetation/Vegetation/Resources/VegetationCommonFunctions.cginc"
#include "Assets/Vegetation/Vegetation/Resources/VegetationCommonVariables.cginc"
#include "UnityStandardInput.cginc"

#define _ALPHATEST_ON 1


#if (defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)) && defined(UNITY_USE_DITHER_MASK_FOR_ALPHABLENDED_SHADOWS)
#define UNITY_STANDARD_USE_DITHER_MASK 1
#endif

// Need to output UVs in shadow caster, since we need to sample texture and do clip/dithering based on it
#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
#define UNITY_STANDARD_USE_SHADOW_UVS 1
#endif

// Has a non-empty shadow caster output struct (it's an error to have empty structs on some platforms...)
#if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
#define UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT 1
#endif

#ifdef UNITY_STEREO_INSTANCING_ENABLED
#define UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT 1
#endif


// SHADOW_ONEMINUSREFLECTIVITY(): workaround to get one minus reflectivity based on UNITY_SETUP_BRDF_INPUT
#define SHADOW_JOIN2(a, b) a##b
#define SHADOW_JOIN(a, b) SHADOW_JOIN2(a,b)
#define SHADOW_ONEMINUSREFLECTIVITY SHADOW_JOIN(UNITY_SETUP_BRDF_INPUT, _ShadowGetOneMinusReflectivity)



//#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
struct VertexOutputShadowCaster
{
    V2F_SHADOW_CASTER_NOPOS
    UNITY_POSITION(pos);

    float2 tex : TEXCOORD0;
};
//#endif

// We have to do these dances of outputting SV_POSITION separately from the vertex shader,
// and inputting VPOS in the pixel shader, since they both map to "POSITION" semantic on
// some platforms, and then things don't go well.

VertexOutputShadowCaster vertShadowCasterCustom(appdata_full v, uint unity_InstanceID : SV_InstanceID)
{
     UNITY_SETUP_INSTANCE_ID(v);
    VertexOutputShadowCaster o;
    
	//VertexRotation(v.vertex);
	
	#if WIND
		ApplyWindToVertex(v, unity_ObjectToWorld._14_24_34_44);
	#endif

    UNITY_INITIALIZE_OUTPUT(VertexOutputShadowCaster, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.tex = float4(AdjustTexCoord(v.texcoord, billboardIndex), 0, 0);
    
    o.pos = UnityObjectToClipPos(v.vertex);
    
    TRANSFER_SHADOW_CASTER_NOPOS(o, o.pos)

    return o;
}


half4 fragShadowCaster (
	#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
	VertexOutputShadowCaster i
	#endif
	#ifdef UNITY_STANDARD_USE_DITHER_MASK
	, UNITY_VPOS_TYPE vpos : VPOS
	#endif
	) : SV_Target
{
	#if defined(UNITY_STANDARD_USE_SHADOW_UVS)
		half alpha = tex2D(_MainTex, i.tex).a * _Color.a * 0.5;

		#if defined(_ALPHATEST_ON)
			clip (alpha - _Cutoff);
		#endif

		#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
			#if defined(UNITY_STANDARD_USE_DITHER_MASK)
				// Use dither mask for alpha blended shadows, based on pixel position xy
				// and alpha level. Our dither texture is 4x4x16.
				half alphaRef = tex3D(_DitherMaskLOD, float3(vpos.xy*0.25,alpha*0.9375)).a;
				clip (alphaRef - 0.01);
			#else
				clip (alpha - _Cutoff);
			#endif
		#endif
	#endif // #if defined(UNITY_STANDARD_USE_SHADOW_UVS)

	SHADOW_CASTER_FRAGMENT(i)
}	

#endif 
