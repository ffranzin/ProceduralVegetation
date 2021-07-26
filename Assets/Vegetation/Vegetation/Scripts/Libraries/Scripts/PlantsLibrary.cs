using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Vegetation.InternalInterfaces;
using Vegetation.Utilities;

namespace Vegetation
{
    /// <summary>
    /// Referencia todas as plantas que são renderizadas.
    /// </summary>
    /// <remarks>   
    /// Está Library é fundamental para garantir uma unica draw call por nivel de LOD de planta.
    /// Sem está Library, uma mesma planta referenciada em diferentes Layers é tratada de
    /// maneira distinta e causa multiplas draw calls. Logo, seu proposito é reduzir a quantidade
    /// de draw call para minimizar os danos, causados pela vegetação, na performance.
    /// 
    /// Para acessar está Library em GPU é necessário fazer uso do PlantsLibrary.cginc.
    /// </remarks>
    internal class PlantsLibrary : GenericLibrary<PlantDescriptor>, IGPULibrary<PlantDescriptor>
    {
        public ReadOnlyCollection<PlantDescriptor> Plants => library.AsReadOnly();

        private ComputeBuffer libraryOnGPU;

        public override void Initialize()
        {
            library.ForEach(a => a.Initialize());

            libraryOnGPU?.Release();
            libraryOnGPU = new ComputeBuffer(Count, Marshal.SizeOf<PlantDescriptor.Descriptor>());
            libraryOnGPU.SetData(library.Select(a => a.descriptor).ToArray());

            base.Initialize();
        }

        public void UpdateLibraryOnGPU(Material material)
        {
            material.SetBuffer("_PlantsLibrary", libraryOnGPU);
        }

        public void UpdateLibraryOnGPU(ComputeShader computeShader, int kernel)
        {
            computeShader.SetBuffer(kernel, "_PlantsLibrary", libraryOnGPU);
        }

        public override void Release()
        {
            libraryOnGPU?.Release();

            base.Release();
        }
    }
}