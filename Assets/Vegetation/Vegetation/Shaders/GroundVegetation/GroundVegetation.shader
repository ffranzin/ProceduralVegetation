// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Vegetation/GroundVegetationDrawInstanced" 
{
	Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		_MainTex("Albedo", 2D) = "white" {}
		_BumpMap("Normal", 2D) = "white" {}
		_Opacity("Opacity", 2D) = "white" {}
		_SpecGlossMap("Specular", 2D) = "white" {}
        _BumpScale("Normal Scale", Range(0.0, 3.0)) = 1.0
		
		_windWave ("Wind Wave", 2D) = "white" {}
		
        _windSpeed("Wind Speed", Range(0.01, 2.0)) = 0.12
        _windWaveSize("Wind Wave Size", Range(0.0, 20)) = 2.0
        _windAmount("Wind Amount", Range(0.0, 10.0)) = 1.0
    }
	

	SubShader
	{
		Tags 
		{ 
			"Queue"="AlphaTest"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
		}

		// ----------------------------------------------------------------------//
		// Shadow Caster
		// ----------------------------------------------------------------------//
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual Cull off
			
			CGPROGRAM
			#pragma target 3.0
			 
			// -------------------------------------

			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			
			#pragma multi_compile_shadowcaster
			#pragma multi_compile_instancing
		    #pragma instancing_options procedural:setup

			#pragma vertex vertShadowCasterCustom
			#pragma fragment fragShadowCaster

			#include "UnityCG.cginc"

			#include "GroundVegetationSetupInstancing.cginc"
			#include "GroundVegetationShadowCustom.cginc"
			
			ENDCG
		}
		

		// ----------------------------------------------------------------------
		// Deferred
		// ----------------------------------------------------------------------
		Pass
		{
			Name "DEFERRED"
			Tags { "LightMode" = "Deferred" "Queue"="Geometry" "RenderType"="Opaque" }

			ZWrite On ZTest LEqual Cull off
			CGPROGRAM
			#pragma target 5.0
			#pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 nomrt
            #pragma only_renderers d3d11

			#define _NORMALMAP 1
			#define _ALPHATEST_ON 1
			#define UNITY_PACK_WORLDPOS_WITH_TANGENT 1

			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON

			#pragma multi_compile_prepassfinal
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
		    #pragma instancing_options procedural:setup

			#include "UnityStandardCore.cginc"
			
			#include "GroundVegetationSetupInstancing.cginc"
			#include "Assets/Vegetation/Vegetation/Scripts/Libraries/Shaders/PlantsLibrary.cginc"
			#include "Assets/Vegetation/Vegetation/Scripts/Libraries/Shaders/BillboardsLibrary.cginc"

			#pragma vertex MyVertDeferred
			#pragma fragment MyFragmentDeferred

			sampler2D _Opacity;
			float _AlphaCutoff;

			VertexOutputDeferred MyVertDeferred(appdata_full v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				VertexOutputDeferred o;
				UNITY_INITIALIZE_OUTPUT(VertexOutputDeferred, o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				//v.vertex.xyz = VertexRotation(v.vertex);
				v.normal.xyz = float3(0,1,0);

				#if WIND
					ApplyWindToVertex(v, unity_ObjectToWorld._14_24_34_44);
				#endif

				float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
				#if UNITY_REQUIRE_FRAG_WORLDPOS
					#if UNITY_PACK_WORLDPOS_WITH_TANGENT
						o.tangentToWorldAndPackedData[0].w = posWorld.x;
						o.tangentToWorldAndPackedData[1].w = posWorld.y;
						o.tangentToWorldAndPackedData[2].w = posWorld.z;
					#else
						o.posWorld = posWorld.xyz;
					#endif
				#endif
				o.pos = UnityObjectToClipPos(v.vertex);

				o.tex.xy = AdjustTexCoord(v.texcoord.xy, billboardIndex);

				o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
				float3 normalWorld = UnityObjectToWorldNormal(v.normal);

				#ifdef _TANGENT_TO_WORLD
					float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

					float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
					o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
					o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
					o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
				#else
					o.tangentToWorldAndPackedData[0].xyz = 0;
					o.tangentToWorldAndPackedData[1].xyz = 0;
					o.tangentToWorldAndPackedData[2].xyz = normalWorld;
				#endif

				o.ambientOrLightmapUV = 0;
				#ifdef LIGHTMAP_ON
					o.ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#elif UNITY_SHOULD_SAMPLE_SH
					o.ambientOrLightmapUV.rgb = ShadeSHPerVertex(normalWorld, o.ambientOrLightmapUV.rgb);
				#endif
				#ifdef DYNAMICLIGHTMAP_ON
					o.ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				#ifdef _PARALLAXMAP
					TANGENT_SPACE_ROTATION;
					half3 viewDirForParallax = mul(rotation, ObjSpaceViewDir(v.vertex));
					o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
					o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
					o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
				#endif

				return o;
			}

			void MyFragmentDeferred (
				VertexOutputDeferred i,
				out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1,
				out half4 outGBuffer2 : SV_Target2, out half4 outEmission : SV_Target3          // RT3: emission (rgb), --unused-- (a)
				#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
					,out half4 outShadowMask : SV_Target4       // RT4: shadowmask (rgba)
				#endif
				)
			{
				UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

				

				FRAGMENT_SETUP(s)
				UNITY_SETUP_INSTANCE_ID(i);

				clip(tex2D(_Opacity, i.tex.xy).r - _AlphaCutoff);
				clip(tex2D(_MainTex, i.tex.xy).a - _AlphaCutoff);

				if (_PlantsLibrary[_InstanceModelIndex].debugLOD > 0.5)
				{
					_Color = float4(_InstanceLODLevel == 0, _InstanceLODLevel == 1, _InstanceLODLevel == 2, 1);
				}

				// no analytic lights in this pass
				UnityLight dummyLight = DummyLight ();
				half atten = 1;

				// only GI
				half occlusion = Occlusion(i.tex.xy);
				#if UNITY_ENABLE_REFLECTION_BUFFERS
					bool sampleReflectionsInDeferred = false;
				#else
					bool sampleReflectionsInDeferred = true;
				#endif
				
				UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, dummyLight, sampleReflectionsInDeferred);

				half3 emissiveColor = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect).rgb;

				#ifdef _EMISSION
					emissiveColor += Emission (i.tex.xy);
				#endif

				#ifndef UNITY_HDR_ON
					emissiveColor.rgb = exp2(-emissiveColor.rgb);
				#endif


				UnityStandardData data;
				data.diffuseColor   = s.diffColor * _Color;
				data.occlusion      = occlusion;
				data.specularColor  = s.specColor;
				data.smoothness     = s.smoothness;
				data.normalWorld    = s.normalWorld;

				UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

				// Emissive lighting buffer
				outEmission = half4(emissiveColor, 1);

				// Baked direct lighting occlusion if any
				#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
					outShadowMask = UnityGetRawBakedOcclusions(i.ambientOrLightmapUV.xy, IN_WORLDPOS(i));
				#endif
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
