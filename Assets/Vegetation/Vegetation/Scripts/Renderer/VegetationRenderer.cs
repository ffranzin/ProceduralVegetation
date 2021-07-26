using UnityEngine;
using Utils.Atlas;

namespace Vegetation.Rendering
{
    internal static partial class VegetationRenderer
    {
        private static AdvancedAtlasMultiResolution vegetationAtlas;

        private static ComputeShader computeVegetation;

        public static void Initialize()
        {
            vegetationAtlas = new AdvancedAtlasMultiResolution(RenderTextureFormat.RGFloat, FilterMode.Point, RenderTextureReadWrite.Linear, "VegetationPositionBuffer");

            computeVegetation = Resources.Load<ComputeShader>("ComputeVegetationDistribution");

            InitializeDistribution();
            InitializeLOD();
            InitializeRenderer();
        }


        public static AtlasPageDescriptor InitializeVegetationPage(VegetationAreaRenderer vegetationArea)
        {
            float placementDistance = VegetationSettings.GetVegetationPlacementDistance(vegetationArea.VegetationCover);

            int pageResolutionX = Mathf.CeilToInt((vegetationArea.AdjustedBoundsMinMax.z - vegetationArea.AdjustedBoundsMinMax.x) / placementDistance);
            int pageResolutionZ = Mathf.CeilToInt((vegetationArea.AdjustedBoundsMinMax.w - vegetationArea.AdjustedBoundsMinMax.y) / placementDistance);

            int pageResolution = Mathf.Max(pageResolutionX, pageResolutionZ);

            //pageResolution = (pageResolution - (pageResolution % 16)) + 16;

            AtlasPageDescriptor page = vegetationAtlas.GetPageResolution(pageResolution);

            RegisterAreaToReceiveVegetation(vegetationArea, page);

            return page;
        }


        public static void RenderArea(VegetationAreaRenderer area)
        {
            RegisterToComputeLOD(area);
        }


        public static void Release()
        {
            ReleaseLOD();
            ReleaseRenderer();
            ReleaseDistribution();
        }
    }
}
