using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Profiling;

namespace Vegetation.Rendering
{
    internal static partial class VegetationRenderer
    {
        private static readonly int modelIndexPropertyID = Shader.PropertyToID("_InstanceModelIndex");
        private static readonly int LODLevelPropertyID = Shader.PropertyToID("_InstanceLODLevel");
        private static readonly int LODBufferDescriptorPropertyID = Shader.PropertyToID("_LODBufferDescriptor");
        private static readonly int elementsPositionsBufferLODPropertyID = Shader.PropertyToID("_PlantsPositionsBufferLODGeometry");

        private static List<RendererComponents> renderersComponents;

        private static Bounds renderBounds;


        #region INITIALIZATION
        private static void InitializeRenderer()
        {
            LinearizeRenderersComponents();

            renderBounds = new Bounds(Vector3.zero, Vector3.one * VegetationSettings.GetVegetationViewDistance(VegetationCover.BIG_TREE));
        }


        private static void LinearizeRenderersComponents()
        {
            renderersComponents?.Clear();
            renderersComponents = new List<RendererComponents>();

            for (int i = 0; i < LibrariesManager.PlantsLibrary.Count; i++)
            {
                LOD[] lods = LibrariesManager.PlantsLibrary.Get(i).LODGroup.GetLODs();

                for (int j = 0; j < lods.Length; j++)
                {
                    Renderer[] renderers = lods[j].renderers;
                    for (int k = 0; k < renderers.Length; k++)
                    {
                        Mesh mesh = renderers[k].GetComponent<MeshFilter>().sharedMesh;
                        Material[] materials = renderers[k].sharedMaterials;

                        for (int l = 0; l < mesh.subMeshCount; l++)
                        {
                            RendererComponents rc = new RendererComponents
                            {
                                PlantModelIndex = i,
                                LODLevel = j,
                                Mesh = mesh,
                                Renderer = renderers[k],
                                SubmeshIndex = l,
                                Material = materials[l],
                                DebugMaterial = new Material(materials[l])
                            };
                            rc.DebugMaterial.SetColor("_Color", new Color(j == 0 ? 0 : 1, j == 1 ? 0 : 1, j == 1 ? 0 : 1, 1));
                            rc.GeometryMPB = new MaterialPropertyBlock();
                            rc.ShadowMPB = new MaterialPropertyBlock();

                            renderersComponents.Add(rc);
                        }
                    }
                }
            }
        }
        #endregion


        public static void Render(Camera camera)
        {
            ComputeDistribution(Camera.main);
            ComputeLOD(Camera.main);

            renderBounds.center = camera.transform.position;

#if UNITY_EDITOR
            Profiler.BeginSample("Vegetation_Rendering - GetCounter Debug");
            GetPlantsInstanceCounter();
            Profiler.EndSample();
#endif

            Profiler.BeginSample("Vegetation_Rendering - Render");
            for (int i = 0; i < renderersComponents.Count; i++)
            {
                Material material = renderersComponents[i].Material;

#if UNITY_EDITOR
                //material = renderersComponents[i].DebugLOD ? renderersComponents[i].DebugMaterial : material;
#endif

                renderersComponents[i].Renderer.GetPropertyBlock(renderersComponents[i].GeometryMPB);
                renderersComponents[i].GeometryMPB.SetInt(modelIndexPropertyID, renderersComponents[i].PlantModelIndex);
                renderersComponents[i].GeometryMPB.SetInt(LODLevelPropertyID, renderersComponents[i].LODLevel);
                renderersComponents[i].GeometryMPB.SetBuffer(LODBufferDescriptorPropertyID, GeometryLODBufferDescriptorOnGPU);
                renderersComponents[i].GeometryMPB.SetBuffer(elementsPositionsBufferLODPropertyID, GeometryLODBufferDataOnGPU);
                Graphics.DrawMeshInstancedIndirect(renderersComponents[i].Mesh, renderersComponents[i].SubmeshIndex, material, renderBounds, GlobalArgBuffersOnGPU, Marshal.SizeOf<CustomArgBuffer>() * i, renderersComponents[i].GeometryMPB, UnityEngine.Rendering.ShadowCastingMode.Off, true);

                renderersComponents[i].Renderer.GetPropertyBlock(renderersComponents[i].ShadowMPB);
                renderersComponents[i].ShadowMPB.SetInt(modelIndexPropertyID, renderersComponents[i].PlantModelIndex);
                renderersComponents[i].ShadowMPB.SetInt(LODLevelPropertyID, renderersComponents[i].LODLevel);
                renderersComponents[i].ShadowMPB.SetBuffer(LODBufferDescriptorPropertyID, ShadowLODBufferDescriptorOnGPU);
                renderersComponents[i].ShadowMPB.SetBuffer(elementsPositionsBufferLODPropertyID, ShadowLODBufferDataOnGPU);
                Graphics.DrawMeshInstancedIndirect(renderersComponents[i].Mesh, renderersComponents[i].SubmeshIndex, material, renderBounds, GlobalShadowArgBuffersOnGPU, Marshal.SizeOf<CustomArgBuffer>() * i, renderersComponents[i].ShadowMPB, UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly, false);
            }
            Profiler.EndSample();
        }


        private static void ReleaseRenderer()
        {
            renderersComponents.Clear();
            renderersComponents = null;
        }
    }
}
