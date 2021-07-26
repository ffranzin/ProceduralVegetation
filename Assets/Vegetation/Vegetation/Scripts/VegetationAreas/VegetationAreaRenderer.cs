using UnityEngine;
using Utils.Atlas;
using Utils.Cameras;

namespace Vegetation.Rendering
{
    /// <summary>
    /// Area de vegetação voltada a renderização das plantas.
    /// </summary>
    internal class VegetationAreaRenderer : VegetationArea, IVegetationAreaRenderable
    {
        /// <summary>
        /// Mapa que estrutura as plantas distribuidas à está areas.
        /// </summary>
        public AtlasPageDescriptor VegetationmapPage { get; private set; }

        /// <summary>
        /// Mapa de altura que cobre toda a area, utilizado para posicionar as 
        /// plantas sobre o terreno.
        /// </summary>
        public Atlas.AtlasPageDescriptor HeightmapPage { get; private set; }


        public VegetationAreaRenderer(Bounds bounds) : base(bounds)
        {
            VegetationCover = VegetationCover.NO_VEGETATION;
        }

        /// <summary>
        /// Invoca a renderização das plantas desta area.
        /// </summary>
        /// <remarks>
        /// A cobertura vegetal de uma determinada Area é definda atraves da 
        /// distancia da mesma para a Camera. Com isso, se a distancia:
        /// 
        ///     1. AUMENTAR: pode haver a re-distribuição das plantas, de modo  
        /// que menos coberturas vegetais sejam associadas a area; dessa forma,
        /// prevalece plantas de maior porte. 
        ///
        ///     2. DIMINUIR: pode haver a re-distribuição das plantas, de modo  
        /// que mais coberturas vegetais sejam associadas a area; a partir disso, 
        /// as plantas de menor porte sao distribuidas.
        /// </remarks>
        public void Render(Atlas.AtlasPageDescriptor heightmapPage = null)
        {
            float distance = Mathf.Sqrt(bounds.SqrDistance(MainCamera.Instance.Camera.transform.position));

            VegetationCover vegetationCovertmp = VegetationSettings.GetVegetationCoverBasedOnDistance(distance);

            if (vegetationCovertmp == VegetationCover.NO_VEGETATION)
            {
                VegetationCover = VegetationCover.NO_VEGETATION;
                VegetationmapPage?.Release();
                return;
            }

            if (vegetationCovertmp != VegetationCover)
            {
                VegetationCover = vegetationCovertmp;
                VegetationmapPage?.Release();
                VegetationmapPage = VegetationRenderer.InitializeVegetationPage(this);
            }

            HeightmapPage = heightmapPage ?? DefaultVegetationInputData.DefaultHeightmapAtlasPage;

            VegetationRenderer.RenderArea(this);
            LastAccess = Time.frameCount;
        }


        ~VegetationAreaRenderer()
        {
            VegetationmapPage?.Release();
            VegetationmapPage = null;
        }
    }
}
