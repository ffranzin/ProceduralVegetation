using UnityEngine;

namespace Vegetation.Tests
{
    internal partial class VegetationClient
    {
        private IVegetationAreaCollectable vegetation;

        [Header("Plants Collector")]
        [SerializeField] private bool collectPlants = false;
        [SerializeField] private bool ShowCollectedPlants = false;
        [SerializeField] private Vector3 size = new Vector3(100, 1, 100);
        [SerializeField] private CollectableVegetationCover CollectableVegetationCover = CollectableVegetationCover.MEDIUM_TREE;
        private Bounds bounds = new Bounds();


        private void CollectPlantsAroundCamera()
        {
            if (!collectPlants)
            {
                ShowCollectedPlants = false;
            }

            if (collectPlants && Time.frameCount % 100 == 0)
            {
                bounds = new Bounds(Camera.main.transform.position, size);

                vegetation?.Release();
                vegetation = VegetationFacade.VegetationAreaCollector(bounds, CollectableVegetationCover);
            }
        }

        private void DrawCollectedPlants()
        {
            if (ShowCollectedPlants && vegetation != null)
            {
                Vector3[] positions = vegetation.PositionsWithHeight;
                Gizmos.color = new Color(0, 1, 0, 0.1f);

                float centerheight = positions.Length > 0 ? positions[0].y : 0;

                Gizmos.DrawCube(new Vector3(bounds.center.x, centerheight, bounds.center.z), bounds.size);

                Gizmos.color = Color.green;
                for (int i = 0; i < positions.Length; i++)
                {
                    Gizmos.DrawRay(positions[i], Vector3.up * 10);
                    Gizmos.DrawSphere(positions[i] + Vector3.up * 10, 0.5f);
                }
            }
        }
    }
}