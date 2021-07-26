using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Vegetation
{
    /// <summary>
    /// Classe que define Layer de Vegetação (agrupamento de plantas) e o comportamento do mesmo 
    /// durante a distribuição. Uma cobertura vegetal, quando existente, é formado por um ou 
    /// mais Layer de vegetação.
    /// </summary>
    internal abstract class VegetationLayerDescriptor : ScriptableObject
    {
        /// <summary>
        /// Cobertura vegetal na qual está planta está inserida.
        /// </summary>
        public VegetationCover VegetationCover => (VegetationCover)layerDescriptor.vegetationCover;

        #region Structs
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct LayerDescriptor
        {
            /////////////////////// GLOBAL PARAMETERS //////////////////////
            public int vegetationCover;
            public float placementDistance;
            public float selfDistance;
            public float worldObstacleRadius;
            ////////////////////////////////////////////////////////////////

            //////////////////// DISTRIBUTION PARAMETERS ////////////////////
            public int octaves;
            public float frequency;
            public float amplitude;
            public float gain;
            public float lacunarity;
            ////////////////////////////////////////////////////////////////

            public unsafe fixed float adaptability[FeaturesIndex.FEATURES_COUNT];
            public unsafe fixed int discretizedPlantsIndex[VegetationConstants.BUFFER_SIZE_RAM_VRAM];
        }
        #endregion

        public LayerDescriptor layerDescriptor;

        [SerializeField]protected List<PlantDescriptor> m_Plants = new List<PlantDescriptor>();
        [SerializeField]protected List<int> m_PlantsPlacementProbability = new List<int>();

        public ReadOnlyCollection<PlantDescriptor> PlantsRenderer => m_Plants.AsReadOnly();


        public virtual void InitializeVegetationLayer()
        {
            layerDescriptor.placementDistance = VegetationSettings.GetVegetationPlacementDistance(VegetationCover);

            DiscretizeModelsIndex();
        }
        

        //public bool HasPlantReference(PlantDescriptor plant)
        //{
        //    return m_Plants.IndexOf(plant) >= 0;
        //}


        private void DiscretizeModelsIndex()
        {
            for (int i = 0, k = 0; i < m_Plants.Count; i++)
            {
                int plantIndex = LibrariesManager.PlantsLibrary.IndexOf(m_Plants[i]);

                for (int j = k; j < m_PlantsPlacementProbability[i] + k; j++)
                {
                    unsafe
                    {
                        layerDescriptor.discretizedPlantsIndex[j] = plantIndex;
                    }
                }
                k += m_PlantsPlacementProbability[i];
            }
        }


        public void NormalizeModelsPlacementPercentage()
        {
            float sum = m_PlantsPlacementProbability.Sum();

            for (int i = 0; i < m_PlantsPlacementProbability.Count; i++)
            {
                m_PlantsPlacementProbability[i] = Mathf.RoundToInt((float)m_PlantsPlacementProbability[i] / sum * 100f);
            }
        }


        private void OnValidate()
        {
            if(Application.isPlaying)
            {
                InitializeVegetationLayer();
            }
        }
    }
}