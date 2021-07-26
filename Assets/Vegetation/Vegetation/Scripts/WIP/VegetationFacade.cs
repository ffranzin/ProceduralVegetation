using UnityEngine;
using Vegetation.Rendering;

namespace Vegetation
{
    public static class VegetationFacade
    {
        public static void Initialize(string bundlePath)
        {
            DefaultVegetationInputData.Initialize();

            LibrariesManager.Initialize();

            VegetationRenderer.Initialize();

            VegetationGPUExtractor.Initialize();
        }

        public static IVegetationAreaCollectable VegetationAreaCollector(Bounds bounds, CollectableVegetationCover vegetationCover)
        {
            return new VegetationAreaCollector(bounds, (VegetationCover)vegetationCover);
        }


        public static IVegetationAreaRenderable VegetationAreaRenderer(Bounds bounds)
        {
            return new VegetationAreaRenderer(bounds);
        }

        public static float SampleHeight(Vector3 position)
        {
            return 1;
        }

        public static float SampleHeight(Vector2 position)
        {
            return 1;
        }

        public static void Render()
        {
            VegetationRenderer.Render(Camera.main);
        }

        public static void Release()
        {
            VegetationRenderer.Release();
            LibrariesManager.Release();
            DefaultVegetationInputData.Release();
            VegetationGPUExtractor.Release();
        }
    }
}