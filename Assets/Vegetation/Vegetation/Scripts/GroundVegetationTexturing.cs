using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

namespace Vegetation.Rendering
{
    [Serializable]
    internal class GroundVegetationTexturing : MonoBehaviour
    {
        [SerializeField] private List<Texture2D> m_AlbedoMaps = new List<Texture2D>();
        [SerializeField] private List<Texture2D> m_NormalMaps = new List<Texture2D>();
        [SerializeField] private List<Texture2D> m_OpacityMaps = new List<Texture2D>();
        [SerializeField] private List<Texture2D> m_SpecularMaps = new List<Texture2D>();
        [SerializeField] private List<Texture2D> m_AmbientOcclusion = new List<Texture2D>();

        public ReadOnlyCollection<Texture2D> AlbedosMaps => m_AlbedoMaps.AsReadOnly();
        public ReadOnlyCollection<Texture2D> NormalMaps => m_NormalMaps.AsReadOnly();
        public ReadOnlyCollection<Texture2D> OpacityMaps => m_OpacityMaps.AsReadOnly();
        public ReadOnlyCollection<Texture2D> SpecularMaps => m_SpecularMaps.AsReadOnly();
        public ReadOnlyCollection<Texture2D> AmbientOcclusion => m_AmbientOcclusion.AsReadOnly();

        public int TextureCount => m_AlbedoMaps.Count;

        private ComputeBuffer BillboardsIndex;

        private List<Material> Materials;

        private void Start()
        {
            ExtractTextureRectsBillboardsLibrary();

            ExtractMateriaisFromRenderers();

            InitializeMaterials();
        }

#if UNITY_EDITOR
        private void Update()
        {
            InitializeMaterials();
        }
#endif

        private void ExtractMateriaisFromRenderers()
        {
            Materials = new List<Material>();

            foreach (MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                Materials.AddRange(meshRenderer.sharedMaterials);
            }
        }


        private void ExtractTextureRectsBillboardsLibrary()
        {
            int[] billboardsIndexOnLibrary = new int[TextureCount];

            for (int i = 0; i < TextureCount; i++)
            {
                TexturePBRMaps texturePBRMaps = new TexturePBRMaps(AlbedosMaps[i], NormalMaps[i], SpecularMaps[i], OpacityMaps[i], AmbientOcclusion[i]);

                billboardsIndexOnLibrary[i] = LibrariesManager.BillboardsLibrary.IndexOf(texturePBRMaps);
            }

            BillboardsIndex?.Release();
            BillboardsIndex = new ComputeBuffer(TextureCount, sizeof(int));
            BillboardsIndex.SetData(billboardsIndexOnLibrary);
        }


        private void InitializeMaterials()
        {
            for (int i = 0; i < Materials.Count; i++)
            {
                LibrariesManager.BillboardsLibrary.UpdateLibraryOnGPU(Materials[i]);

                Materials[i].SetBuffer("_TexturesIndexOnBillboardsLibrary", BillboardsIndex);
                Materials[i].SetInt("_BillboardsCount", BillboardsIndex.count);
            }
        }
    }
}