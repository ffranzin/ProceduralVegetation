Shader "Custom/Ground"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTexGround("Ground (RGB)", 2D) = "white" {}
        _MainTexForest("Forest (RGB)", 2D) = "white" {}

        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #include "Assets/Vegetation/Vegetation/Resources/Features.cginc"

        sampler2D _MainTexGround;
        sampler2D _MainTexForest;

        float _Glossiness;
        float _Metallic;

        struct Input
        {
            float2 uv_MainTex;
			float3 worldPos;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float forestInfluence = ForestZoneInfluence(IN.worldPos.xz);

            float2 uv = IN.worldPos.xz / 8.0;

            fixed4 ground = tex2D (_MainTexGround, uv);
            fixed4 forest = tex2D (_MainTexForest, uv);

            o.Albedo = lerp(ground, forest, forestInfluence);

            o.Smoothness = _Glossiness;
            o.Metallic = _Metallic;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
