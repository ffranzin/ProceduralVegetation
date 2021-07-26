using UnityEngine;

namespace Vegetation.Rendering
{
    /// <summary>
    /// Classe auxiliar para substituir algumas informacoes, para fins de debug,
    /// que sao fornecidos à vegetação por outros modulos.
    /// </summary>
    internal static class DefaultVegetationInputData
    {
        public static Atlas.AtlasPageDescriptor DefaultHeightmapAtlasPage;

        private static Atlas HeightmapAtlas;

        public static void Initialize()
        {
            HeightmapAtlas = new Atlas(RenderTextureFormat.R8, FilterMode.Point, 32, 32, true);
            DefaultHeightmapAtlasPage = HeightmapAtlas.GetPage();
        }

        public static void Release()
        {
            HeightmapAtlas?.Release();

            DefaultHeightmapAtlasPage = null;
        }
    }
}