using System.Collections.Generic;
using UnityEngine;
using Vegetation.Rendering;

namespace Vegetation
{
    [CreateAssetMenu(menuName = "Vegetation/Vegetation Scene")]
    internal class VegetationScene : ScriptableObject
    {
        public List<VegetationLayerDescriptor> VegetationLayers;

        public LayersLibrary layersLibrary;
        public BillboardsLibrary billboardsLibrary;
        public PlantsLibrary plantsLibrary;

        public void ExtractLayersLibrary()
        {
            layersLibrary?.Clear();

            layersLibrary = layersLibrary ?? CreateInstance<LayersLibrary>();

            for (int i = 0; i < VegetationLayers.Count; i++)
            {
                if (VegetationLayers[i] != null)
                {
                    layersLibrary.Add(VegetationLayers[i]);
                }
            }
        }

        public void ExtractBillboardLibrary()
        {
            billboardsLibrary?.Clear();

            billboardsLibrary = billboardsLibrary ?? CreateInstance<BillboardsLibrary>();

            for (int i = 0; i < VegetationLayers.Count; i++)
            {
                if (VegetationLayers[i] == null)
                {
                    continue;
                }

                for (int j = 0; j < VegetationLayers[i].PlantsRenderer.Count; j++)
                {
                    if (VegetationLayers[i].PlantsRenderer[j] == null)
                    {
                        continue;
                    }

                    GroundVegetationTexturing groundVegetationTexturing = VegetationLayers[i].PlantsRenderer[j].GetComponent<GroundVegetationTexturing>();

                    for (int k = 0; groundVegetationTexturing != null && k < groundVegetationTexturing.TextureCount; k++)
                    {
                        TexturePBRMaps texturePBRMaps = new TexturePBRMaps(groundVegetationTexturing.AlbedosMaps[k],
                                                                            groundVegetationTexturing.NormalMaps[k],
                                                                            groundVegetationTexturing.SpecularMaps[k],
                                                                            groundVegetationTexturing.OpacityMaps[k],
                                                                            groundVegetationTexturing.AmbientOcclusion[k]);

                        billboardsLibrary.Add(texturePBRMaps);
                    }
                }
            }
        }

        public void ExtractPlantsLibrary()
        {
            plantsLibrary?.Clear();

            plantsLibrary = plantsLibrary ?? CreateInstance<PlantsLibrary>();

            for (int i = 0; i < VegetationLayers.Count; i++)
            {
                if (VegetationLayers[i] == null)
                {
                    continue;
                }

                for (int j = 0; j < VegetationLayers[i].PlantsRenderer.Count; j++)
                {
                    if (VegetationLayers[i].PlantsRenderer[j] != null)
                    {
                        plantsLibrary.Add(VegetationLayers[i].PlantsRenderer[j]);
                    }
                }
            }
        }
    }
}