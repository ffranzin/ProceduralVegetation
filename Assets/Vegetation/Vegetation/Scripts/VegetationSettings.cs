
namespace Vegetation
{
    /// <summary>
    /// Configuração global utilizado para distribuir e renderizar a vegetação.
    /// </summary>
    internal static class VegetationSettings
    {
        private static readonly float[] vegetationCoverDistance = new float[6] { 3000, 1000, 500, 250, 120, 50 };
        private static readonly float[] vegetationCoverDensity = new float[6] { 8, 4, 2, 1, 0.5f, 0.25f };

        /// <summary>
        /// Retorna a distacia de visualização das plantas de uma determinada cobertura vegetal.
        /// </summary>
        /// <remarks>
        /// Está é uma distancia importante para gerenciar as area que será distribuido plantas, 
        /// a fim de minimizar o consumo de memoria. Para isso, cada cobertura vegetal é limitada 
        /// a ser distribuida somente no entorno da camera, de modo que a distancia varia de acordo 
        /// com o tamanho das plantas que compoe tal cobertura vegetal. Por exemplo, plantas de  
        /// grande porte sao visiveis a maiores distancias que plantas rasteiras.
        /// </remarks>
        public static float GetVegetationViewDistance(VegetationCover cover)
        {
            return (cover == VegetationCover.NO_VEGETATION) ? 0 : vegetationCoverDistance[(int)cover];
        }

        /// <summary>
        /// Retorna a distacia para a distribuição das plantas de uma determinada cobertura vegetal.
        /// Quando menor o valor retornado, maior a densidade de plantas por m^2.
        /// </summary>
        public static float GetVegetationPlacementDistance(VegetationCover cover)
        {
            return (cover == VegetationCover.NO_VEGETATION) ? -1 : vegetationCoverDensity[(int)cover];
        }


        /// <summary>
        /// Retorna a cobertura vegetal baseado em uma distancia.
        /// </summary>
        public static VegetationCover GetVegetationCoverBasedOnDistance(float distance)
        {
            VegetationCover vegetationCover = VegetationCover.NO_VEGETATION;

            for (int i = vegetationCoverDistance.Length - 1; i >= 0; i--)
            {
                if (vegetationCoverDistance[i] > distance)
                {
                    vegetationCover = (VegetationCover)i;

                    break;
                }
            }

            while (vegetationCover >= 0)
            {
                if (ExistAtLeastOneVegetationCover(vegetationCover))
                {
                    return vegetationCover;
                }

                vegetationCover--;
            }

            return vegetationCover;
        }


        private static bool ExistAtLeastOneVegetationCover(VegetationCover vegetationCover)
        {
            for (int i = 0; i < LibrariesManager.VegetationLayersLibrary.Count; i++)
            {
                if (LibrariesManager.VegetationLayersLibrary.Get(i).VegetationCover == vegetationCover)
                {
                    return true;
                }
            }

            return false;
        }
    }
}