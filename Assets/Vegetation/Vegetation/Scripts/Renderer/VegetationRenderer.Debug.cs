using UnityEngine;
using Utils.Analysis;

namespace Vegetation.Rendering
{
    /// <remarks>
    /// Classe auxiliar de debug da renderização vegetação.
    /// </remarks>
    internal static partial class VegetationRenderer
    {
        private static bool toggleShowInstanceCount = false;
        
        private static void GetPlantsInstanceCounter()
        {
            if (Input.GetKey(KeyCode.I) && Input.GetKeyDown(KeyCode.C))
            {
                toggleShowInstanceCount = !toggleShowInstanceCount;
                Debug.Log("ToggleShowInstanceCount: " + toggleShowInstanceCount);
            }

            if (toggleShowInstanceCount)
            {
                string output = "";

                LODBufferDescriptor[] result = new LODBufferDescriptor[GeometryLODBufferDescriptorOnGPU.count];

                GeometryLODBufferDescriptorOnGPU.GetData(result);

                unsafe
                {
                    for (int i = 0; i < LibrariesManager.PlantsLibrary.Count; i++)
                    {
                        int sum = 0;
                        for (int j = 0; j < VegetationConstants.MAX_LOD_LEVELS; j++)
                        {
                            if (true || j < LibrariesManager.PlantsLibrary.Get(i).LODGroup.lodCount)
                            {
                                int index = i * VegetationConstants.MAX_LOD_LEVELS + j;
                                output += $"LOD{j}: {result[index].instanceCounterOnLODBuffer}    ";
                                sum += result[index].instanceCounterOnLODBuffer;
                            }
                            else
                            {
                                output += $"LOD{j}: --    ";
                            }
                        }
                        output += $"    ---> Sum: {sum}\n";
                    }

                    Debug.Log(output);
                }
            }
        }
    }
}