using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Profiling;
using Utils.Analysis;
using Utils.Cameras;
using Utils.Extensions;

namespace Vegetation.Rendering
{
    internal static partial class VegetationRenderer
    {
        private struct EncapsulatedRequestDataLOD
        {
            public int vegetationType;
            public float placementDistance;
            public Vector4 boundArea;
            public Vector4 positionAtlasPage;
            public Vector4 heightmapAtlasPage;

            public EncapsulatedRequestDataLOD(int vegetationType, float placementDistance, Vector4 boundArea, Vector4 positionAtlasPage, Vector4 heightmapAtlasPage)
            {
                this.vegetationType = vegetationType;
                this.placementDistance = placementDistance;
                this.boundArea = boundArea;
                this.positionAtlasPage = positionAtlasPage;
                this.heightmapAtlasPage = heightmapAtlasPage;
            }
        }

        private static Dictionary<int, List<EncapsulatedRequestDataLOD>> LODEncapsulatedRequestData;
        private static List<ComputeBuffer> LODEncapsulatedRequestDataOnGPU;

        private static ComputeShader computeLOD;
        private static int ComputeLODKernel = -1;
        private static int ReSetInstancesCounterOnGlobalArgsBufferKernel = -1;
        private static int SetInstancesCounterOnGlobalArgsBufferKernel = -1;
        private static uint[] ComputeLODKernelThreadGroup;
        private static uint[] ResetInstancesCounterOnGlobalArgsBufferKernelThreadGroup;
        private static uint[] SetInstancesCounterOnGlobalArgsBufferKernelThreadGroup;

        private static ComputeBuffer GeometryLODBufferDescriptorOnGPU;
        private static ComputeBuffer ShadowLODBufferDescriptorOnGPU;

        private static ComputeBuffer GeometryLODBufferDataOnGPU;
        private static ComputeBuffer ShadowLODBufferDataOnGPU;

        private static ComputeBuffer GlobalArgBuffersOnGPU;
        private static ComputeBuffer GlobalShadowArgBuffersOnGPU;

        private static int LODRequestCounter = 0;

        #region INITIALIZATION
        private static void InitializeLOD()
        {
            computeLOD = Resources.Load<ComputeShader>("ComputeVegetationLOD");

            computeLOD.GetKernelAndThreadGroupSize("VegetationComputeLOD", ref ComputeLODKernel, ref ComputeLODKernelThreadGroup);
            computeLOD.GetKernelAndThreadGroupSize("ResetInstancesCounterOnGlobalArgsBuffer", ref ReSetInstancesCounterOnGlobalArgsBufferKernel, ref ResetInstancesCounterOnGlobalArgsBufferKernelThreadGroup);
            computeLOD.GetKernelAndThreadGroupSize("SetInstancesCounterOnGlobalArgsBuffer", ref SetInstancesCounterOnGlobalArgsBufferKernel, ref SetInstancesCounterOnGlobalArgsBufferKernelThreadGroup);

            LODEncapsulatedRequestData = new Dictionary<int, List<EncapsulatedRequestDataLOD>>();
            LODEncapsulatedRequestDataOnGPU = new List<ComputeBuffer>();

            LinearizeArgsBuffer();

            InitializeLODComputeBuffers();
        }


        private static void InitializeLODComputeBuffers()
        {
            ///Armazena a quantidade estima de instancia de plantas para cada nivel de LOD.
            List<int[]> geometryInstancesCountPerPlant, shadowInstancesCountPerPlant;
            LODBufferDescriptor[] linearGeometryLODBuffer, linearShadowLODBuffer;
            int allGeometryPlantsInstanceRequiredCounter = 0, allShadowPlantsInstanceRequiredCounter = 0;

            //COUNTER PLANTS
            InitializeCounterAuxiliarBuffers();
            ComputeRequiredInstancesPerPlant();

            //LINEARIZE COUNTER PLANTS
            InitializeLinearCounters();
            LinearizeCounter();

            //SEND TO GPU
            InitializeBuffersOnGPU();


            #region ESTIMATE MAX INSTANCES PER PLANT
            /// Inicializa estruturas auxiliares para estimar a quantidade maxima de instancias de cada planta, para cada nivel de LOD.
            void InitializeCounterAuxiliarBuffers()
            {
                geometryInstancesCountPerPlant = new List<int[]>();
                shadowInstancesCountPerPlant = new List<int[]>();

                for (int i = 0; i < LibrariesManager.PlantsLibrary.Count; i++)
                {
                    geometryInstancesCountPerPlant.Add(new int[VegetationConstants.MAX_LOD_LEVELS]);
                    shadowInstancesCountPerPlant.Add(new int[VegetationConstants.MAX_LOD_LEVELS]);
                }
            }

            ///A) Cada planta possui uma cobertura vegetal que tem uma distancia maxima de vizualização. 
            ///B) Alem disso, cada planta é posta em uma área fixa, em função da cobertura vegetal que ela foi atribuida.
            ///A partir de (A) e (B) é possivel estimar aproximadamente um valor maximo de instancias de cada planta, 
            ///para da nivel de LOD desta. Atraves destes valores estimados será feito a alocação de memoria necessária
            ///para armazenar as instancias das plantas apos a computação do LOD (de maneira linearizada em um buffer unico). 
            void ComputeRequiredInstancesPerPlant()
            {
                for (int i = 0; i < LibrariesManager.PlantsLibrary.Count; i++)
                {
                    PlantDescriptor plantDescriptor = LibrariesManager.PlantsLibrary.Get(i);

                    float coveredAreaByPlant = Mathf.Pow(VegetationSettings.GetVegetationPlacementDistance(plantDescriptor.vegetationCover), 2f);

                    for (int k = 0; k < VegetationConstants.MAX_LOD_LEVELS && k < plantDescriptor.LODGroup.lodCount; k++)
                    {
                        geometryInstancesCountPerPlant[i][k] = (int)(plantDescriptor.geometryLOD.GetAreaAtLODLevel(k) / coveredAreaByPlant);
                        shadowInstancesCountPerPlant[i][k] = (int)(plantDescriptor.shadowLOD.GetAreaAtLODLevel(k) / coveredAreaByPlant);
                    }
                }
            }
            #endregion


            #region LINEARIZE COUNTER PLANTS
            void InitializeLinearCounters()
            {
                int size = LibrariesManager.PlantsLibrary.Count * VegetationConstants.MAX_LOD_LEVELS;

                linearGeometryLODBuffer = new LODBufferDescriptor[size];
                linearShadowLODBuffer = new LODBufferDescriptor[size];

                for (int i = 0; i < size; i++)
                {
                    linearGeometryLODBuffer[i] = new LODBufferDescriptor(-1, 0/*realtime setted*/);
                    linearShadowLODBuffer[i] = new LODBufferDescriptor(-1, 0/*realtime setted*/);
                }
            }

            void LinearizeCounter()
            {
                for (int i = 0, globalIndex = 0; i < LibrariesManager.PlantsLibrary.Count; i++)
                {
                    for (int j = 0; j < VegetationConstants.MAX_LOD_LEVELS; j++, globalIndex++)
                    {
                        linearGeometryLODBuffer[globalIndex].firstIndexOnLODBuffer = allGeometryPlantsInstanceRequiredCounter;
                        linearShadowLODBuffer[globalIndex].firstIndexOnLODBuffer = allShadowPlantsInstanceRequiredCounter;

                        allGeometryPlantsInstanceRequiredCounter += geometryInstancesCountPerPlant[i][j];
                        allShadowPlantsInstanceRequiredCounter += shadowInstancesCountPerPlant[i][j];
                    }
                }
            }
            #endregion


            #region SEND TO GPU
            void InitializeBuffersOnGPU()
            {
                int size = LibrariesManager.PlantsLibrary.Count * VegetationConstants.MAX_LOD_LEVELS;

                GeometryLODBufferDescriptorOnGPU?.Release();
                GeometryLODBufferDescriptorOnGPU = new ComputeBuffer(size, Marshal.SizeOf<LODBufferDescriptor>());
                GeometryLODBufferDescriptorOnGPU.SetData(linearGeometryLODBuffer);

                ShadowLODBufferDescriptorOnGPU?.Release();
                ShadowLODBufferDescriptorOnGPU = new ComputeBuffer(size, Marshal.SizeOf<LODBufferDescriptor>());
                ShadowLODBufferDescriptorOnGPU.SetData(linearShadowLODBuffer);

                GeometryLODBufferDataOnGPU?.Release();
                GeometryLODBufferDataOnGPU = new ComputeBuffer(allGeometryPlantsInstanceRequiredCounter, Marshal.SizeOf<VegetationTransform>());

                ShadowLODBufferDataOnGPU?.Release();
                ShadowLODBufferDataOnGPU = new ComputeBuffer(allShadowPlantsInstanceRequiredCounter, Marshal.SizeOf<VegetationTransform>());

                Debug.Log($"LODBuffer Memory: Geometry {GeometryLODBufferDataOnGPU.UsedMemory()} Shadow  {ShadowLODBufferDataOnGPU.UsedMemory()}");
            }
            #endregion
        }


        private static void LinearizeArgsBuffer()
        {
            List<CustomArgBuffer> allArgBuffers = new List<CustomArgBuffer>();

            for (int i = 0; i < LibrariesManager.PlantsLibrary.Count; i++)
            {
                LOD[] lods = LibrariesManager.PlantsLibrary.Get(i).LODGroup.GetLODs();
                for (int j = 0; j < lods.Length; j++)
                {
                    Renderer[] renderers = lods[j].renderers;
                    for (int k = 0; k < renderers.Length; k++)
                    {
                        Mesh mesh = renderers[k].gameObject.GetComponent<MeshFilter>().sharedMesh;

                        if (mesh == null)
                        {
                            Debug.LogError(renderers[k].gameObject.name + " missing mesh.");
                            continue;
                        }

                        for (int l = 0; l < mesh.subMeshCount; l++)
                        {
                            CustomArgBuffer args = new CustomArgBuffer();
                            unsafe
                            {
                                args.globalArgsBuffer[0] = mesh.GetIndexCount(l);
                                args.globalArgsBuffer[1] = 0; //runtime set
                                args.globalArgsBuffer[2] = mesh.GetIndexStart(l);
                                args.globalArgsBuffer[3] = mesh.GetBaseVertex(l);
                            }

                            args.plantIndexOnLibrary = i;
                            args.LODLevel = j;

                            allArgBuffers.Add(args);
                        }
                    }
                }
            }

            GlobalArgBuffersOnGPU?.Release();
            GlobalArgBuffersOnGPU = new ComputeBuffer(allArgBuffers.Count, Marshal.SizeOf<CustomArgBuffer>(), ComputeBufferType.IndirectArguments);
            GlobalArgBuffersOnGPU.SetData(allArgBuffers);

            GlobalShadowArgBuffersOnGPU?.Release();
            GlobalShadowArgBuffersOnGPU = new ComputeBuffer(allArgBuffers.Count, Marshal.SizeOf<CustomArgBuffer>(), ComputeBufferType.IndirectArguments);
            GlobalShadowArgBuffersOnGPU.SetData(allArgBuffers);
        }
        #endregion


        private static void ProceduralDistributionPlantsLOD()
        {
            computeLOD.SetTexture(ComputeLODKernel, "_PlantsPositionsBuffer", vegetationAtlas.texture);

            computeLOD.SetBuffer(ComputeLODKernel, "_LODBufferDescriptor_RW", GeometryLODBufferDescriptorOnGPU);
            computeLOD.SetBuffer(ComputeLODKernel, "_LODBufferDescriptor_RW_shadow", ShadowLODBufferDescriptorOnGPU);

            computeLOD.SetBuffer(ComputeLODKernel, "_PlantsPositionsBufferLODGeometry_RW", GeometryLODBufferDataOnGPU);
            computeLOD.SetBuffer(ComputeLODKernel, "_PlantsPositionsBufferLODShadow_RW", ShadowLODBufferDataOnGPU);

            computeLOD.SetVector("_WorldSpaceCameraPos", MainCamera.Instance.Camera.transform.position);
            computeLOD.SetVectorArray("_FrustumPlanesNormal", MainCamera.Instance.frustumPlanesNormals);

            computeLOD.SetTexture(ComputeLODKernel, "_HeightmapAtlas", DefaultVegetationInputData.DefaultHeightmapAtlasPage.atlas.texture);

            LibrariesManager.VegetationLayersLibrary.UpdateLibraryOnGPU(computeLOD, ComputeLODKernel);
            LibrariesManager.PlantsLibrary.UpdateLibraryOnGPU(computeLOD, ComputeLODKernel);

            while (LODEncapsulatedRequestData.Keys.Count >= LODEncapsulatedRequestDataOnGPU.Count)
            {
                LODEncapsulatedRequestDataOnGPU.Add(new ComputeBuffer(VegetationConstants.MAX_AREAS_RENDERED_PER_FRAME, Marshal.SizeOf<EncapsulatedRequestDataLOD>()));
            }

            for (int i = 0; i < LODEncapsulatedRequestData.Keys.Count; i++)
            {
                int resolution = LODEncapsulatedRequestData.Keys.ElementAt(i);

                int pageCounter = LODEncapsulatedRequestData[resolution].Count;

                if (pageCounter == 0)
                {
                    continue;
                }

                LODEncapsulatedRequestDataOnGPU[i].SetData(LODEncapsulatedRequestData[resolution], 0, 0, pageCounter);

                computeLOD.SetBuffer(ComputeLODKernel, "_EncapsulatedRequestDataLOD", LODEncapsulatedRequestDataOnGPU[i]);

                computeLOD.Dispatch(ComputeLODKernel, Mathf.CeilToInt(resolution / (float)ComputeLODKernelThreadGroup[0]),
                                                      Mathf.CeilToInt(resolution / (float)ComputeLODKernelThreadGroup[1]),
                                                      Mathf.CeilToInt(pageCounter / (float)ComputeLODKernelThreadGroup[2]));

                LODEncapsulatedRequestData[resolution].Clear();
            }
        }


        private static void RegisterToComputeLOD(VegetationAreaRenderer area)
        {
            if (LODRequestCounter >= VegetationConstants.MAX_AREAS_RENDERED_PER_FRAME)
            {
                return;
            }

            if (!LODEncapsulatedRequestData.ContainsKey(area.VegetationmapPage.size))
            {
                LODEncapsulatedRequestData.Add(area.VegetationmapPage.size, new List<EncapsulatedRequestDataLOD>());
            }

            LODEncapsulatedRequestData[area.VegetationmapPage.size].Add(new EncapsulatedRequestDataLOD((int)area.VegetationCover, VegetationSettings.GetVegetationPlacementDistance(area.VegetationCover), area.AdjustedBoundsMinMax,
                                area.VegetationmapPage.pageDescriptorToGPU, area.HeightmapPage.pageDescriptorToGPU));

            LODRequestCounter++;
        }


        #region INSTANCE COUNTER SET/RESET
        private static void SetInstancesCounterOnGlobalArgsBuffer()
        {
            uint[] tg = SetInstancesCounterOnGlobalArgsBufferKernelThreadGroup;

            computeLOD.SetBuffer(SetInstancesCounterOnGlobalArgsBufferKernel, "_GlobalArgsBuffer", GlobalArgBuffersOnGPU);
            computeLOD.SetBuffer(SetInstancesCounterOnGlobalArgsBufferKernel, "_LODBufferDescriptor_RW", GeometryLODBufferDescriptorOnGPU);
            computeLOD.Dispatch(SetInstancesCounterOnGlobalArgsBufferKernel, Mathf.CeilToInt(GlobalArgBuffersOnGPU.count / (float)tg[0]), (int)tg[1], (int)tg[2]);

            computeLOD.SetBuffer(SetInstancesCounterOnGlobalArgsBufferKernel, "_GlobalArgsBuffer", GlobalShadowArgBuffersOnGPU);
            computeLOD.SetBuffer(SetInstancesCounterOnGlobalArgsBufferKernel, "_LODBufferDescriptor_RW", ShadowLODBufferDescriptorOnGPU);
            computeLOD.Dispatch(SetInstancesCounterOnGlobalArgsBufferKernel, Mathf.CeilToInt(GlobalArgBuffersOnGPU.count / (float)tg[0]), (int)tg[1], (int)tg[2]);
        }


        private static void ReSetInstancesCounterOnGlobalArgsBuffer()
        {
            uint[] tg = ResetInstancesCounterOnGlobalArgsBufferKernelThreadGroup;

            computeLOD.SetBuffer(ReSetInstancesCounterOnGlobalArgsBufferKernel, "_LODBufferDescriptor_RW", GeometryLODBufferDescriptorOnGPU);
            computeLOD.Dispatch(ReSetInstancesCounterOnGlobalArgsBufferKernel, Mathf.CeilToInt(GeometryLODBufferDescriptorOnGPU.count / (float)tg[0]), (int)tg[1], (int)tg[2]);

            computeLOD.SetBuffer(ReSetInstancesCounterOnGlobalArgsBufferKernel, "_LODBufferDescriptor_RW", ShadowLODBufferDescriptorOnGPU);
            computeLOD.Dispatch(ReSetInstancesCounterOnGlobalArgsBufferKernel, Mathf.CeilToInt(ShadowLODBufferDescriptorOnGPU.count / (float)tg[0]), (int)tg[1], (int)tg[2]);
        }
        #endregion


        private static void ComputeLOD(Camera camera)
        {
            Profiler.BeginSample("Vegetation_Rendering - Reset LOD Instance Counter");
            ReSetInstancesCounterOnGlobalArgsBuffer();
            Profiler.EndSample();

            Profiler.BeginSample("Vegetation_Rendering - Compute LOD");
            ProceduralDistributionPlantsLOD();
            Profiler.EndSample();

            Profiler.BeginSample("Vegetation_Rendering - Set LOD Instance Counter");
            SetInstancesCounterOnGlobalArgsBuffer();
            Profiler.EndSample();

            LODRequestCounter = 0;
        }


        private static void ReleaseLOD()
        {
            LODEncapsulatedRequestData.Clear();
            LODEncapsulatedRequestData = null;

            for (int i = 0; LODEncapsulatedRequestDataOnGPU != null && i < LODEncapsulatedRequestDataOnGPU.Count; i++)
            {
                LODEncapsulatedRequestDataOnGPU[i]?.Release();
                LODEncapsulatedRequestDataOnGPU[i] = null;
            }

            GeometryLODBufferDescriptorOnGPU?.Release();
            ShadowLODBufferDescriptorOnGPU?.Release();
            GeometryLODBufferDataOnGPU?.Release();
            ShadowLODBufferDataOnGPU?.Release();
            GlobalArgBuffersOnGPU?.Release();
            GlobalShadowArgBuffersOnGPU?.Release();

            GeometryLODBufferDescriptorOnGPU = null;
            ShadowLODBufferDescriptorOnGPU = null;
            GeometryLODBufferDataOnGPU = null;
            ShadowLODBufferDataOnGPU = null;
            GlobalArgBuffersOnGPU = null;
            GlobalShadowArgBuffersOnGPU = null;
        }
    }
}