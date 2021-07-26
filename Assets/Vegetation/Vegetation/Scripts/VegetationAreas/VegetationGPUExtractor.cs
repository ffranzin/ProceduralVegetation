using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Profiling;
using Utils.Extensions;

namespace Vegetation
{
    /// <summary>
    /// Classe que invoca a distribuição das plantas na GPU e recupera as posições 
    /// para serem utilizadas na CPU.
    /// </summary>
    /// <remarks>
    /// Está classe é destinada a reduzir a quantidade de acesso e transferencias de 
    /// dados entre CPU e GPU. Os algoritmos utilizado nesse processo são os mesmos 
    /// utilizados para distribuir as plantas para renderização. O retorno desta classe 
    /// é sempre deterministico, logo, se necessario é possivel destruir as posições 
    /// geradas para controle de memoria.
    /// </remarks>
    internal static class VegetationGPUExtractor
    {
        private static ComputeShader computeCollectPlantsPosition;
        private static int kernelCollectPlantsPosition;
        private static uint[] kernelCollectPlantsPositionThreadGroup;

        private static ComputeBuffer outputBuffer;
        private static ComputeBuffer outputBufferCounter;

        public static void Initialize()
        {
            computeCollectPlantsPosition = Resources.Load<ComputeShader>("ComputeVegetationDistribution");
            computeCollectPlantsPosition.GetKernelAndThreadGroupSize("CollectPlantsPosition", ref kernelCollectPlantsPosition, ref kernelCollectPlantsPositionThreadGroup);

            outputBuffer = new ComputeBuffer(8192, Marshal.SizeOf<Vector2>(), ComputeBufferType.Append);
            outputBufferCounter = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        }


        private static void InitializeOutputBufferOnGPU(VegetationArea vegetationArea, float placementDistance)
        {
            int instanceCountX = Mathf.CeilToInt(vegetationArea.AreaSize.x / placementDistance);
            int instanceCountY = Mathf.CeilToInt(vegetationArea.AreaSize.y / placementDistance);

            int totalInstanceNeeded = instanceCountX * instanceCountY;

            if (totalInstanceNeeded > outputBuffer.count)
            {
                outputBuffer.Release();
                outputBuffer = new ComputeBuffer(totalInstanceNeeded, Marshal.SizeOf<Vector2>(), ComputeBufferType.Append);
            }

            outputBuffer.SetCounterValue(0);
        }


        private static int GetOutputBufferCounter()
        {
            Profiler.BeginSample("Vegetation_Rendering - Collect Positions GetCounter");

            ComputeBuffer.CopyCount(outputBuffer, outputBufferCounter, 0);

            int[] bufferCounterOutput = new int[1];
            outputBufferCounter.GetData(bufferCounterOutput);

            Profiler.EndSample();

            return bufferCounterOutput[0];
        }


        private static Vector2[] RetrievePlantsPositionsFromGPU()
        {
            Profiler.BeginSample("Vegetation_Navegation - Collect Positions GetData");

            int plantsCounter = GetOutputBufferCounter();

            Vector2[] plantsPositions = new Vector2[plantsCounter];

            outputBuffer.GetData(plantsPositions, 0, 0, plantsCounter);

            Profiler.EndSample();
            return plantsPositions;
        }


        private static void ExtractPlantsPositionsFromGPU(VegetationArea vegetationArea, float placementDistance)
        {
            LibrariesManager.VegetationLayersLibrary.UpdateLibraryOnGPU(computeCollectPlantsPosition, kernelCollectPlantsPosition);

            computeCollectPlantsPosition.SetBuffer(kernelCollectPlantsPosition, "_CollectPositionsBuffer", outputBuffer);

            computeCollectPlantsPosition.SetVector("_AreaMinMax", vegetationArea.AdjustedBoundsMinMax);
            computeCollectPlantsPosition.SetVector("_AreaMinMaxOriginal", vegetationArea.BoundsMinMax);

            computeCollectPlantsPosition.SetFloat("_PlacementDistance", placementDistance);
            computeCollectPlantsPosition.SetInt("_VegetationCoverLimit", (int)vegetationArea.VegetationCover);

            int tgX = Mathf.CeilToInt(vegetationArea.AreaSize.x / placementDistance);
            int tgY = Mathf.CeilToInt(vegetationArea.AreaSize.y / placementDistance);

            tgX = Mathf.CeilToInt(tgX / (float)kernelCollectPlantsPositionThreadGroup[0]);
            tgY = Mathf.CeilToInt(tgY / (float)kernelCollectPlantsPositionThreadGroup[1]);

            computeCollectPlantsPosition.Dispatch(kernelCollectPlantsPosition, tgX, tgY, (int)kernelCollectPlantsPositionThreadGroup[2]);
        }

        /// <summary>
        /// Distribui e recupera as posicoes das plantas de uma determinada area.
        /// </summary>
        public static Vector2[] GetPlantsPositions(VegetationArea vegetationArea)
        {
            float placementDistance = VegetationSettings.GetVegetationPlacementDistance(vegetationArea.VegetationCover);

            InitializeOutputBufferOnGPU(vegetationArea, placementDistance);

            ExtractPlantsPositionsFromGPU(vegetationArea, placementDistance);

            Vector2[] plantsPositions = RetrievePlantsPositionsFromGPU();

            return plantsPositions;
        }

        /// <summary>
        /// Distribui e recupera a quantidade de plantas de uma determinada area.
        /// </summary>
        public static int GetPlantsPositionsCounter(VegetationArea vegetationArea)
        {
            float placementDistance = VegetationSettings.GetVegetationPlacementDistance(vegetationArea.VegetationCover);

            InitializeOutputBufferOnGPU(vegetationArea, placementDistance);

            ExtractPlantsPositionsFromGPU(vegetationArea, placementDistance);

            int counter = GetOutputBufferCounter();

            return counter;
        }


        public static void Release()
        {
            outputBuffer?.Release();
            outputBuffer = null;

            outputBufferCounter?.Release();
            outputBufferCounter = null;
        }
    }
}