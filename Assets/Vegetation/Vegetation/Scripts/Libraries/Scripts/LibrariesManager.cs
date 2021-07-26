using UnityEngine;
using Vegetation.InternalInterfaces;
using Vegetation.Rendering;

namespace Vegetation
{
    /// <summary>
    /// Classe que mantem referencia as Libraries necessarias para a 
    /// Distribuição e Renderização da vegetação.
    /// </summary>
    internal static class LibrariesManager
    {
        private static PlantsLibrary m_PlantsLibrary;
        private static BillboardsLibrary m_BillboardsLibrary;
        private static LayersLibrary m_LayersLibrary;

        /// <summary>
        /// Biblioteca de Plantas. 
        /// </summary>
        public static IGPULibrary<PlantDescriptor> PlantsLibrary => m_PlantsLibrary;
        /// <summary>
        /// Biblioteca de billboards para renderização de vegetação rasteira. 
        /// </summary>
        public static IGPULibrary<TexturePBRMaps> BillboardsLibrary => m_BillboardsLibrary;
        /// <summary>
        /// Biblioteca de Layers da vegetação.
        /// </summary>
        public static IGPULibrary<VegetationLayerDescriptor> VegetationLayersLibrary => m_LayersLibrary;

        public static void Initialize()
        {
            //This order cannot be changed
            m_PlantsLibrary = Resources.Load<PlantsLibrary>("PlantsLibrary");
            m_PlantsLibrary.Initialize();
            
            m_BillboardsLibrary = Resources.Load<BillboardsLibrary>("BillboardsLibrary");
            m_BillboardsLibrary.Initialize();

            m_LayersLibrary = Resources.Load<LayersLibrary>("LayersLibrary");
            m_LayersLibrary.Initialize();

            Debug.Log("----------------------------  VEGETATION LIBRARIES  -----------------------------" +
                      $"\n    => PlantsLibrary initialized with {m_PlantsLibrary.Count} elements." +
                      $"\n    => BillboardsLibrary initialized with {m_BillboardsLibrary.Count} elements." +
                      $"\n    => LayersLibrary initialized with {m_LayersLibrary.Count} elements.");
        }

        public static void Release()
        {
            m_PlantsLibrary.Release();
            m_BillboardsLibrary.Release();
            m_LayersLibrary.Release();
        }
    }
}