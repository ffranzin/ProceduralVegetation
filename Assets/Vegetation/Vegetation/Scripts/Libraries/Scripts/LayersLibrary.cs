using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Vegetation.InternalInterfaces;
using Vegetation.Utilities;

namespace Vegetation
{
    /// <summary>
    /// Estrutura todos os arquivos de configuração dos Layers de vegetação.
    /// </summary>
    /// <remarks>
    /// Para acessar está Library em GPU é necessário fazer uso do LayersLibrary.cginc.
    /// </remarks>
    internal class LayersLibrary : GenericLibrary<VegetationLayerDescriptor>, IGPULibrary<VegetationLayerDescriptor>
    {
        public ReadOnlyCollection<VegetationLayerDescriptor> VegetationLayers => library.AsReadOnly();

        private ComputeBuffer VegetationLayerDescriptorOnGPU;

        public override void Add(VegetationLayerDescriptor item)
        {
            base.Add(item);

            library.OrderBy(a => (int)a.VegetationCover);
        }

        public override void Initialize()
        {
            library.ForEach(a => a.InitializeVegetationLayer());

            if (Count > 0)
            {
                VegetationLayerDescriptorOnGPU?.Release();
                VegetationLayerDescriptorOnGPU = new ComputeBuffer(Count, Marshal.SizeOf<VegetationLayerDescriptor.LayerDescriptor>());
                VegetationLayerDescriptorOnGPU.SetData(library.Select(a => a.layerDescriptor).ToArray());
            }
        }

        public void UpdateLibraryOnGPU(ComputeShader compute, int kernel)
        {
            compute.SetBuffer(kernel, "_VegetationLayersLibrary", VegetationLayerDescriptorOnGPU);
            compute.SetInt("_VegetationLayersLibraryCount", library.Count);
        }


        public void UpdateLibraryOnGPU(Material material)
        {
            material.SetBuffer("_VegetationLayersLibrary", VegetationLayerDescriptorOnGPU);
            material.SetInt("_VegetationLayersLibraryCount", library.Count);
        }

        public override void Release()
        {
            VegetationLayerDescriptorOnGPU.Release();
            base.Release();
        }
    }

}