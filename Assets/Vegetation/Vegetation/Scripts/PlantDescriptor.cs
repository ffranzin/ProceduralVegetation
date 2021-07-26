using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vegetation.Rendering;

namespace Vegetation
{
    /// <summary>
    /// Descritor de uma planta.
    /// </summary>
    /// <remarks>
    /// Está classe é responsavel por fazer o gerenciamento de parametros utilizados em GPU para renderização de uma planta.
    /// Dentre estes parametros destaca-se o nivel de LOD, nas qual cada planta possui dois destes niveis. 
    /// O primeiro deles é destinado a resolução da geometria que passa pelo estagio de Vertex e Fragment shader. Já o segundo 
    /// LOD é voltado a resolução da sombra, onde é definido um refinamento para a geometria que irá passar pelos estagio de 
    /// Vertex e ShadowCaster.
    /// 
    /// Ambos os niveis de LOD sao discretizados em um vetor de n posições e enviados a GPU. Atraves da distancia entre 
    /// camera e uma instancia desta planta, normalizado pela diastancia de culling, é gerado um indice de acesso a este 
    /// buffer. O retorno deste acesso define o nivel de LOD da geometria e da sombra. 
    /// </remarks>
    internal class PlantDescriptor : MonoBehaviour
    {
        [Serializable]
        public struct Descriptor
        {
            [HideInInspector] public float geometryCullDistance;
            [HideInInspector] public float shadowCullDistance;
            public float frustumCullingDistance;
            public float minScale;
            public float maxScale;

            [HideInInspector] public unsafe fixed int discreteLOD[VegetationConstants.BUFFER_SIZE_RAM_VRAM];
            [HideInInspector] public unsafe fixed int discreteLODShadow[VegetationConstants.BUFFER_SIZE_RAM_VRAM];

            [Range(0, 1)] public int enableRender;
            [Range(0, 1)] public int enableShadow;
            [Range(0, 1)] public int debugLOD;
        }

        public VegetationCover vegetationCover = VegetationCover.NO_VEGETATION;
        public CustomLODSettings geometryLOD;
        public CustomLODSettings shadowLOD;
        public LODGroup LODGroup { get; private set; }

        public Descriptor descriptor;//TODO set as private
        public Descriptor GetDescriptor()
        {
            return descriptor;
        }


        private void DiscretizeLODLevels()
        {
            List<int> curveSamplesGeometry = geometryLOD.DiscretizeLODIntoSamples(VegetationConstants.BUFFER_SIZE_RAM_VRAM);
            List<int> curveSamplesShadow = shadowLOD.DiscretizeLODIntoSamples(VegetationConstants.BUFFER_SIZE_RAM_VRAM);

            unsafe
            {
                for (int i = 0; i < curveSamplesGeometry.Count; i++)
                {
                    descriptor.discreteLOD[i] = curveSamplesGeometry[i];
                    descriptor.discreteLODShadow[i] = curveSamplesShadow[i];
                }

                descriptor.discreteLOD[VegetationConstants.BUFFER_SIZE_RAM_VRAM - 1] = -1;
                descriptor.discreteLODShadow[VegetationConstants.BUFFER_SIZE_RAM_VRAM - 1] = -1;
            }
        }


        private void InitializeDescriptor()
        {
            DiscretizeLODLevels();
            descriptor.geometryCullDistance = geometryLOD.cullDistance;

#if !UNITY_EDITOR
            ///isto é uma pequena otimização que requer a correta inicialização de alguns buffers.
            ///Para evitar a agregação de complexidade ao codigo (onde seria necessario a reinicialização dos buffers, 
            ///caso shadowDistance seja modificado), está otimização é aplicada somente fora do editor.
            descriptor.shadowCullDistance = Mathf.Min(shadowLOD.cullDistance, QualitySettings.shadowDistance);
#else
            descriptor.shadowCullDistance = shadowLOD.cullDistance;
#endif
        }


        public void Initialize()
        {
            if (vegetationCover == VegetationCover.NO_VEGETATION)
            {
                Debug.LogError($"The plant {name} must have a vegetation cover.");
            }
            
            if (GetComponentsInChildren<Tree>().ToList().Exists(a => a.hasSpeedTreeWind))
            {
                //as animações de vento do speedtree requerem que haja uma instancia auxiliar da planta
                //na cena sendo afetada por uma WindZone, e que possa servir de base para copiar 
                //informações enviadas aos materiais.
                if(LODGroup == null)
                { 
                    LODGroup = Instantiate(gameObject).GetComponent<LODGroup>();

                    DisableCastShadow(LODGroup);

                    DisableFrustumCulling(LODGroup);
                }
            }
            else
            {
                LODGroup = GetComponent<LODGroup>();
            }

            InitializeDescriptor();
        }


        /// <summary>
        /// As animações de vento são aplicadas somente se a instancia auxiliar estiver visivel.
        /// por isso, deve-se forçar que a instancia não sofra Frustum Culling.
        /// </summary>
        private static void DisableFrustumCulling(LODGroup LODGroup)
        {
            foreach (MeshFilter meshFilter in LODGroup.GetComponentsInChildren<MeshFilter>())
            {
                meshFilter.mesh.bounds = new Bounds(meshFilter.mesh.bounds.center, Vector3.one * float.MaxValue);
            }

            LODGroup.enabled = false;
        }

        /// <summary>
        /// como é necessário instanciar um objeto, mais draw calls são necessarias.
        /// para minimizar isto, a sombra da planta é desabilidata, uma vez que não agrega nada visualmente.
        /// </summary>
        private static void DisableCastShadow(LODGroup LODGroup)
        {
             LODGroup.GetComponentsInChildren<MeshRenderer>().ToList().ForEach(a => a.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off);
        }
    }
}