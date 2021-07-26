
Shader "Custom/SpeedTreeIndirectBillboard"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _HueVariation ("Hue Variation", Color) = (1.0,0.5,0.0,0.1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        [MaterialEnum(None,0,Fastest,1)] _WindQuality ("Wind Quality", Range(0,1)) = 0
    }

    // targeting SM3.0+
    SubShader
    {
        Tags
        {
            "Queue"="AlphaTest"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
        }
        LOD 400

        CGPROGRAM
			#pragma surface surf Lambert vertex:SpeedTreeBillboardVert nodirlightmap nodynlightmap addshadow dithercrossfade
			#pragma target 5.0
			#pragma multi_compile __ BILLBOARD_FACE_CAMERA_POS
			#pragma shader_feature EFFECT_BUMP
			//#pragma shader_feature EFFECT_HUE_VARIATION

			#define EFFECT_HUE_VARIATION 1
			#define ENABLE_WIND
            #include "SpeedTreeBillboardCommon.cginc"
			#include "SpeedTreeSetupInstancing.cginc"
					#pragma multi_compile_vertex  LOD_FADE_PERCENTAGE LOD_FADE_CROSSFADE
					#pragma multi_compile_fragment __ LOD_FADE_CROSSFADE


			#pragma multi_compile_instancing
			#pragma instancing_options procedural:setup

            void surf(Input IN, inout SurfaceOutput OUT)
            {
					#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
						unity_LODFade.x = 0.1f;
						unity_LODFade.y = 0.1f;
					#endif
                SpeedTreeFragOut o;
                SpeedTreeFrag(IN, o);
                SPEEDTREE_COPY_FRAG(OUT, o)
            }

        ENDCG

    }
    FallBack "Transparent/Cutout/VertexLit"
}
