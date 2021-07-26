using UnityEngine;

namespace Vegetation
{

    /// <summary>
    /// Area de vegetação que pode ser regatado as posições das plantas.
    /// </summary>
    /// <remarks>
    /// Toda a estruturação de renderização, onde as plantas são distribuidas e 
    /// renderizadas em função de distancias para a camera não afetam este tipo 
    /// de Area. Ou seja, mesmo que uma Area não esteja renderizando as plantas 
    /// é possivel ter acesso as posições das plantas postas dentro desta area.
    /// </remarks>
    internal partial class VegetationAreaCollector : VegetationArea, IVegetationAreaCollectable
    {
        /// <summary>
        /// Quantidade de plantas dentro da area, conforme a cobertura vegetal definida. 
        /// </summary>
        public int Count
        {
            get
            {
                if (m_Count == -1)
                {
                    if(m_Positions != null)
                    {
                        m_Count = m_Positions.Length;
                    }
                    else
                    {
                        m_Count = VegetationGPUExtractor.GetPlantsPositionsCounter(this);
                    }
                }

                return m_Count;
            }
        }
        private int m_Count = -1;


        /// <summary>
        /// Posição das plantas dentro da area, conforme a cobertura vegetal definida. 
        /// </summary>
        public Vector2[] Positions
        {
            get
            {
                if (m_Positions == null)
                {
                    m_Positions = VegetationGPUExtractor.GetPlantsPositions(this);
                }

                LastAccess = Time.frameCount;
                return m_Positions;
            }
        }
        private Vector2[] m_Positions = null;


        /// <summary>
        /// Posição das plantas dentro da area, conforme a cobertura vegetal definida. 
        /// </summary>
        public Vector3[] PositionsWithHeight
        {
            get
            {
                if (m_PositionsWithHeight == null)
                {
                    Vector2[] positions = Positions; //force the distribution if needed

                    m_PositionsWithHeight = new Vector3[positions.Length];

                    for (int i = 0; i < positions.Length; i++)
                    {
                        float height = VegetationFacade.SampleHeight(positions[i]);
                        m_PositionsWithHeight[i] = new Vector3(positions[i].x, height, positions[i].y);
                    }
                }

                LastAccess = Time.frameCount;
                return m_PositionsWithHeight;
            }
        }
        private Vector3[] m_PositionsWithHeight = null;


        public VegetationAreaCollector(Bounds bounds, VegetationCover vegetationCover) : base(bounds)
        {
            VegetationCover = vegetationCover;
        }


        public override void Release()
        {
            m_Positions = null;
            m_PositionsWithHeight = null;

            base.Release();
        }


        ~VegetationAreaCollector()
        {
            Release();
        }
    }
}