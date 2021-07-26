
using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace Vegetation
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct VegetationTransform
    {
        public Vector3 position;
        public Vector3 rotation;
        public float scale;

        public VegetationTransform(Vector3 position, Vector3 rotation, float scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }
}



namespace Vegetation.Rendering
{
    internal class RendererComponents
    {
        private PlantDescriptor PlantDescriptor;
        public RendererComponents()
        {
            PlantDescriptor = LibrariesManager.PlantsLibrary.Get(0);
        }

        public int PlantModelIndex;
        public int LODLevel;
        public Mesh Mesh;
        public int SubmeshIndex;
        public Renderer Renderer;
        public Material Material;
        public Material DebugMaterial;
        public MaterialPropertyBlock GeometryMPB;
        public MaterialPropertyBlock ShadowMPB;

        public bool DebugLOD => PlantDescriptor.descriptor.debugLOD == 1;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct LODBufferDescriptor
    {
        public int firstIndexOnLODBuffer;
        public int instanceCounterOnLODBuffer;

        public LODBufferDescriptor(int firstIndexOnLODBuffer, int instanceCounterOnLODBuffer)
        {
            this.firstIndexOnLODBuffer = firstIndexOnLODBuffer;
            this.instanceCounterOnLODBuffer = instanceCounterOnLODBuffer;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct CustomArgBuffer
    {
        public unsafe fixed uint globalArgsBuffer[5];

        public int plantIndexOnLibrary;
        public int LODLevel;
    }

    [Serializable]
    internal class TexturePBRMaps
    {
        public Texture2D Albedo;
        public Texture2D Normal;
        public Texture2D Specular;
        public Texture2D Opacity;
        public Texture2D AmbientOcclusion;

        public TexturePBRMaps(Texture2D albedo, Texture2D normal, Texture2D specular, Texture2D opacity, Texture2D ambientOcclusion)
        {
            Albedo = albedo;
            Normal = normal;
            Specular = specular;
            Opacity = opacity;
            AmbientOcclusion = ambientOcclusion;
        }

        public static bool operator ==(TexturePBRMaps c1, TexturePBRMaps c2)
        {
            return  c1.Albedo == c2.Albedo &&
                    c1.Normal == c2.Normal &&
                    c1.Specular == c2.Specular &&
                    c1.Specular == c2.Specular &&
                    c1.AmbientOcclusion == c2.AmbientOcclusion;
        }

        public static bool operator !=(TexturePBRMaps c1, TexturePBRMaps c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object texturePBRMaps)
        {
            if (!(texturePBRMaps is TexturePBRMaps))
            { 
                return false;
            }

            return (TexturePBRMaps)texturePBRMaps == this;
        }
    }

    internal struct BillboardRect
    {
        public Vector2 min;
        public Vector2 max;

        public BillboardRect(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }
    }
}