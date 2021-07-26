using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Utils;
using Vegetation.InternalInterfaces;
using Vegetation.Utilities;

namespace Vegetation.Rendering
{
    /// <summary>
    /// Estrutura todas as texturas utilizadas para compor as billboards da vegetação rasteira.
    /// </summary>
    /// <remarks>
    /// Está classe tem como proposito reduzir a duplicidade de texturas em GPU, minimizando o consumo de memoria.
    /// 
    /// Para acessar está Library em GPU é necessário fazer uso do BillboardsLibrary.cginc.
    /// </remarks>
    internal class BillboardsLibrary : GenericLibrary<TexturePBRMaps>, IGPULibrary<TexturePBRMaps>
    {
        public float MipmapBias = -0.5f;
        public int MaxTextureSize = 8192;

        private List<TexturePBRMaps> TexturesOnAtas => library;
        private List<BillboardRect> TexturesRects;
        private ComputeBuffer TexturesRectsOnGPU;

        private Texture2D AlbedoAtlas; //TODO use RenderTexture (VRAM only) instead of Texture2D (RAM + VRAM)
        private Texture2D NormalAtlas;
        private Texture2D SpecularAtlas;
        private Texture2D AmbientOcclusionAtlas;
        
        public override void Initialize()
        {
            if(TexturesOnAtas == null || TexturesOnAtas.Count == 0)
            {
                return;
            }

            TexturesRects = new List<BillboardRect>();

            CreateTextureAtlas();

            SetTextureRectsOnGPU();
        }


        private void SetTextureRectsOnGPU()
        {
            TexturesRectsOnGPU?.Release();
            TexturesRectsOnGPU = new ComputeBuffer(Count, Marshal.SizeOf<BillboardRect>());
            TexturesRectsOnGPU.SetData(TexturesRects);
        }


        private void CreateTextureAtlas()
        {
            if(AlbedoAtlas != null && NormalAtlas != null && SpecularAtlas != null && AmbientOcclusionAtlas != null)
            {
                //return;
            }

            Release();

            AlbedoAtlas = new Texture2D(1, 1);
            NormalAtlas = new Texture2D(1, 1);
            SpecularAtlas = new Texture2D(1, 1);
            AmbientOcclusionAtlas = new Texture2D(1, 1);
            Texture2D opacityAtlas = new Texture2D(1, 1);

            Rect[] rectAlbedos = AlbedoAtlas.PackTextures(TexturesOnAtas.Select(a => CombineRGB_R(a.Albedo, a.Opacity, TextureFormat.RGBA32)).ToArray(), 0, MaxTextureSize);
            NormalAtlas.PackTextures(TexturesOnAtas.Select(a => a.Normal).ToArray(), 0, MaxTextureSize);
            SpecularAtlas.PackTextures(TexturesOnAtas.Select(a => a.Specular).ToArray(), 0, MaxTextureSize);
            AmbientOcclusionAtlas.PackTextures(TexturesOnAtas.Select(a => a.AmbientOcclusion).ToArray(), 0, MaxTextureSize);
            opacityAtlas.PackTextures(TexturesOnAtas.Select(a => a.Opacity).ToArray(), 0, MaxTextureSize);

            AlbedoAtlas.mipMapBias = MipmapBias;
            NormalAtlas.mipMapBias = MipmapBias;
            SpecularAtlas.mipMapBias = MipmapBias;
            AmbientOcclusionAtlas.mipMapBias = MipmapBias;

            AlbedoAtlas.Apply();
            NormalAtlas.Apply();
            SpecularAtlas.Apply();
            AmbientOcclusionAtlas.Apply();

            TexturesRects.Clear();
            TexturesRects.AddRange(rectAlbedos.Select(a => new BillboardRect(a.min, a.max)));
        }


        private Texture2D CombineRGB_R(Texture2D albedo, Texture2D opacity, TextureFormat format, bool useMipmap = true)
        {
            Color32[] albedoColors = albedo.GetPixels32();
            Color32[] opacityColors = opacity.GetPixels32();

            if (albedoColors.Length != opacityColors.Length)
            {
                Debug.LogError("Textures must be the same size.");
                return null;
            }

            for (int i = 0; i < albedoColors.Length; i++)
            {
                albedoColors[i].a = opacityColors[i].r;
            }

            Texture2D texture = new Texture2D(albedo.width, albedo.height, format, useMipmap);
            texture.SetPixels32(albedoColors);
            texture.Apply();

            return texture;
        }

        void IGPULibrary<TexturePBRMaps>.UpdateLibraryOnGPU(Material material)
        {
            if (TexturesOnAtas == null || TexturesOnAtas.Count == 0)
            {
                return;
            }

            material.SetTexture("_MainTex", AlbedoAtlas);
            material.SetTexture("_BumpMap", NormalAtlas);
            material.SetTexture("_SpecularAtlas", SpecularAtlas);//TODO fix name (check the unity names)
            material.SetTexture("_AmbientOcclusionAtlas", AmbientOcclusionAtlas);//TODO fix name (check the unity names)

            material.SetBuffer("_BillboardRects", TexturesRectsOnGPU);
        }

        void IGPULibrary<TexturePBRMaps>.UpdateLibraryOnGPU(ComputeShader computeShader, int kernel)
        {
            throw new System.Exception("Not implemented. Why Compute Shaders are using PBR Maps?");
        }

        public override void Release()
        {
            DestroyImmediate(AlbedoAtlas);
            DestroyImmediate(NormalAtlas);
            DestroyImmediate(SpecularAtlas);
            DestroyImmediate(AmbientOcclusionAtlas);

            AlbedoAtlas = null;
            NormalAtlas = null;
            SpecularAtlas = null;
            AmbientOcclusionAtlas = null;

            base.Release();
        }
    }
}