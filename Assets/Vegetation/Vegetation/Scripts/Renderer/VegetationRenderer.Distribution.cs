using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Utils.Analysis;
using Utils.Atlas;
using Utils.Extensions;


namespace Vegetation.Rendering
{
    /// <remarks>
    /// Distribui a vegetação para cada area renderizavel.
    /// 
    /// Por questoes de performance, cada solicitação de distribuição é recebida 
    /// e mantida em um dicionario para que a distribuição seja invocada em conjunto
    /// para varias Areas (previamente a renderização). Assim, menos dispatchs são 
    /// feitos a GPU, minimizando as chances de sobrecarrega.
    /// 
    /// Como os buffers de escrita é um atlas, ou seja, um buffer unico, ao dispachar 
    /// varios processos haverá o enfileiramento destes ao inves de uma execução em 
    /// paralelo. Isso ocorre porque buffers RW são bloqueados durante a execução de
    /// um compute shader. 
    /// </remarks>
    internal static partial class VegetationRenderer
    {
        private static int vegetationDistributionKernel = -1;
        private static uint[] vegetationDistributionKernelThreadGroup;

        private struct EncapsulatedRequestDataDistribution
        {
            public int vegetationType;
            public float placementDistance;
            public Vector4 adjustedBound;
            public Vector4 originalBound;
            public Vector4 positionAtlasPage;

            public EncapsulatedRequestDataDistribution(int vegetationType, float placementDistance, Vector4 boundArea, Vector4 boundAreaOriginal, Vector4 positionAtlasPage)
            {
                this.vegetationType = vegetationType;
                this.placementDistance = placementDistance;
                this.adjustedBound = boundArea;
                this.originalBound = boundAreaOriginal;
                this.positionAtlasPage = positionAtlasPage;
            }
        }

        private static Dictionary<int, List<EncapsulatedRequestDataDistribution>> distributionEncapsulatedRequestData;
        private static List<ComputeBuffer> distributionEncapsulatedRequestDataOnGPU;

        private static int distributionRequestCounter = 0;

        private static void InitializeDistribution()
        {
            computeVegetation.GetKernelAndThreadGroupSize("GeneratePlantsPositions", ref vegetationDistributionKernel, ref vegetationDistributionKernelThreadGroup);

            distributionEncapsulatedRequestData = new Dictionary<int, List<EncapsulatedRequestDataDistribution>>();
            distributionEncapsulatedRequestDataOnGPU = new List<ComputeBuffer>();
        }


        private static void DispatchDistribution()
        {
            computeVegetation.SetTexture(vegetationDistributionKernel, "_PlantsPositionsBuffer_RW", vegetationAtlas.texture);

            uint[] tg = vegetationDistributionKernelThreadGroup;

            List<int> allResolutionsKeys = distributionEncapsulatedRequestData.Keys.ToList();

            while (allResolutionsKeys.Count >= distributionEncapsulatedRequestDataOnGPU.Count)
            {
                distributionEncapsulatedRequestDataOnGPU.Add(new ComputeBuffer(VegetationConstants.MAX_AREAS_RENDERED_PER_FRAME, Marshal.SizeOf<EncapsulatedRequestDataDistribution>()));
            }

            int freeBufferIndex = 0;

            LibrariesManager.VegetationLayersLibrary.UpdateLibraryOnGPU(computeVegetation, vegetationDistributionKernel);

            for (int i = 0; i < allResolutionsKeys.Count; i++)
            {
                int resolution = allResolutionsKeys[i];
                int pageCounter = distributionEncapsulatedRequestData[resolution].Count;

                distributionEncapsulatedRequestDataOnGPU[freeBufferIndex].SetData(distributionEncapsulatedRequestData[resolution], 0, 0, pageCounter);

                computeVegetation.SetBuffer(vegetationDistributionKernel, "_EncapsulatedRequestDataDistribution", distributionEncapsulatedRequestDataOnGPU[freeBufferIndex]);

                computeVegetation.Dispatch(vegetationDistributionKernel, Mathf.CeilToInt(resolution / (float)tg[0]),
                                                                         Mathf.CeilToInt(resolution / (float)tg[1]),
                                                                         Mathf.CeilToInt(pageCounter / (float)tg[2]));

                distributionEncapsulatedRequestData[resolution].Clear();
                freeBufferIndex++;
            }

            distributionEncapsulatedRequestData.Clear();
        }


        private static void RegisterAreaToReceiveVegetation(VegetationAreaRenderer area, AtlasPageDescriptor vegetationPage)
        {
            if (!distributionEncapsulatedRequestData.ContainsKey(vegetationPage.size))
            {
                distributionEncapsulatedRequestData.Add(vegetationPage.size, new List<EncapsulatedRequestDataDistribution>());
            }

            EncapsulatedRequestDataDistribution request = new EncapsulatedRequestDataDistribution(
                                                                                        (int)area.VegetationCover,
                                                                                        VegetationSettings.GetVegetationPlacementDistance(area.VegetationCover),
                                                                                        area.AdjustedBoundsMinMax,
                                                                                        area.BoundsMinMax,
                                                                                        vegetationPage.pageDescriptorToGPU);

            distributionEncapsulatedRequestData[vegetationPage.size].Add(request);

            distributionRequestCounter++;
        }


        private static void ComputeDistribution(Camera camera)
        {
            if (distributionRequestCounter > 0)
            {
                DispatchDistribution();

                distributionRequestCounter = 0;
            }
        }


        private static void ReleaseDistribution()
        {
            distributionEncapsulatedRequestData.Clear();
            distributionEncapsulatedRequestData = null;

            for (int i = 0; distributionEncapsulatedRequestDataOnGPU != null && i < distributionEncapsulatedRequestDataOnGPU.Count; i++)
            {
                distributionEncapsulatedRequestDataOnGPU[i]?.Release();
                distributionEncapsulatedRequestDataOnGPU[i] = null;
            }
        }
    }
}