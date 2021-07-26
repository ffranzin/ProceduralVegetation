using UnityEngine;

namespace Vegetation
{
    /// <summary>
    /// Classe base que define uma Area de Vegetação.
    /// </summary>
    internal class VegetationArea
    {
        /// <summary>
        /// Ultima acesso efetuado a Area.
        /// </summary>
        public int LastAccess { get; protected set; }

        protected readonly Bounds bounds = new Bounds();

        public Vector4 AdjustedBoundsMinMax { get; private set; } = Vector4.zero;

        public Vector2 AreaSize => new Vector2(AdjustedBoundsMinMax.z - AdjustedBoundsMinMax.x, AdjustedBoundsMinMax.w - AdjustedBoundsMinMax.y);

        public Vector4 BoundsMinMax { get; private set; } = Vector4.zero;

        
        /// <summary>
        /// Cobertura vegetal que compoe está area. 
        /// </summary>
        public VegetationCover VegetationCover
        {
            get
            {
                return m_VegetationCover;
            }
            protected set
            {
                m_VegetationCover = value;

                if (m_VegetationCover != VegetationCover.NO_VEGETATION)
                {
                    UpdateMinMax();
                }
            }
        }
        private VegetationCover m_VegetationCover;


        public VegetationArea(Bounds bounds)
        {
            this.bounds = bounds;

            BoundsMinMax = new Vector4(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
        }


        /// <summary>
        /// Ajusta as bounds para uma distribuição correta das plantas.
        /// </summary>
        /// <remarks>
        /// Cada cobertura vegetal é distribuida em funcao de uma grid que possui 
        /// uma resolução especifica. Para garantir uma distribuicao correta, é 
        /// obrigatorio que o minimo e maximo da bound sejam multiplos da resolução 
        /// estipulada a cobertura vegetal desta area.
        /// 
        /// Por exemplo, supordo que uma area seja receba plantas distribuidas a cada 2x2m.
        /// Uma area delimitada por uma bound de minimo (5,6) e maximo (15, 16) gera uma distribuição 
        /// errada. Nesse caso, é necessario ajustara a bounds para que seu minimo seja (4,6) e o 
        /// maximo (16, 16). Observe que o minimo é ajustado para um valor inferior, e o maximo para 
        /// um valor superior para garantir que nenhuma planta que pertence a area seja descartada.
        /// </remarks>
        private void UpdateMinMax()
        {
            float placementDistance = VegetationSettings.GetVegetationPlacementDistance(m_VegetationCover);

            Vector2 min = new Vector2(bounds.min.x - (bounds.min.x % placementDistance), bounds.min.z - (bounds.min.z % placementDistance));
            Vector2 max = new Vector2((min.x + bounds.size.x), (min.y + bounds.size.z));
            max.x -= (max.x % placementDistance);
            max.y -= (max.y % placementDistance);

            AdjustedBoundsMinMax = new Vector4(Mathf.FloorToInt(bounds.min.x / placementDistance) * placementDistance,
                                     Mathf.FloorToInt(bounds.min.z / placementDistance) * placementDistance,
                                     Mathf.CeilToInt(bounds.max.x / placementDistance) * placementDistance,
                                     Mathf.CeilToInt(bounds.max.z / placementDistance) * placementDistance);
        }


        public virtual void Release()
        {
        }
    }
}
