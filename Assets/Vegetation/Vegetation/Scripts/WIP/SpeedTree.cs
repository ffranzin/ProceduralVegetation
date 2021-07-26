using System;
using UnityEngine;


namespace Vegetation.Rendering
{
    [CreateAssetMenu(menuName = "Vegetation/SpeedTree")]
    [Serializable]
    internal class SpeedTree : VegetationLayerDescriptor
    {
        public override void InitializeVegetationLayer()
        {
            base.InitializeVegetationLayer();
        }


        private static void UpdateSpeedtreeMaterial()
        {
            Debug.LogError("POSICIONAR");

            //Shader shader = Shader.Find("Custom/SpeedTreeIndirect");

            //MeshFilter[] meshes = plant.GetComponentsInChildren<MeshFilter>();
            //for (int j = 0; j < meshes.Length; j++)
            //{
            //    meshes[j].mesh.bounds = new Bounds(meshes[j].mesh.bounds.center, Vector3.one * 1000000); // resize the bounding box to avoid GPU culling -- if culled, wind cannot be updated
            //}

            //plant.GetComponent<LODGroup>().enabled = false; // should be disable because of the wind animation -- if enabled only one LOD will be updated

            //MeshRenderer[] renderers = plant.GetComponentsInChildren<MeshRenderer>();
            //for (int j = 0; j < renderers.Length; j++)
            //{
            //    for (int k = 0; k < renderers[j].materials.Length; k++)
            //    {
            //        renderers[j].materials[k].shader = shader;
            //        renderers[j].materials[k].EnableKeyword("EFFECT_HUE_VARIATION");
            //        renderers[j].materials[k].EnableKeyword("EFFECT_BUMP");
            //        renderers[j].materials[k].DisableKeyword("EFFECT_SUBSURFACE"); // not supported in defered rendering yet
            //        renderers[j].materials[k].SetFloat("_Glossiness", 0); // not supported in defered rendering yet

            //        if (j == renderers.Length - 1)
            //        {
            //            renderers[j].materials[k].EnableKeyword("EFFECT_BILLBOARD");
            //        }
            //        else
            //        {
            //            renderers[j].materials[k].DisableKeyword("EFFECT_BILLBOARD");
            //        }
            //    }
            //}
        }
    }
}
