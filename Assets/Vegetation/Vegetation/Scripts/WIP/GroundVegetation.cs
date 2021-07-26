using System;
using UnityEngine;

namespace Vegetation.Rendering
{
    [CreateAssetMenu(menuName = "Vegetation/Ground Vegetation")]
    [Serializable]
    internal class GroundVegetation : VegetationLayerDescriptor
    {
        public override void InitializeVegetationLayer()
        {
            base.InitializeVegetationLayer();

            InitializeMaterials();
        }


        private void InitializeMaterials()
        {
            for (int i = 0; i < m_Plants.Count; i++)
            {
                Renderer[] renderers = m_Plants[i].GetComponentsInChildren<Renderer>();

                for (int j = 0; j < renderers.Length; j++)
                {
                    for (int k = 0; k < renderers[j].sharedMaterials.Length; k++)
                    {
                        LibrariesManager.BillboardsLibrary.UpdateLibraryOnGPU(renderers[j].sharedMaterials[k]);
                    }
                }
            }
        }
    }
}