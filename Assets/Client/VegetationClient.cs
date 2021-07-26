using UnityEngine;

namespace Vegetation.Tests
{

    internal partial class VegetationClient : MonoBehaviour
    {
        void Awake()
        {
            VegetationFacade.Initialize("asd");
            InitializeGrid();
        }

        private void FixedUpdate()
        {
            CollectOldCells();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DestroyAllCells();
                VegetationFacade.Initialize("asd");
            }

            InstanciateCellAroundPos();

            CollectPlantsAroundCamera();

            VegetationFacade.Render();
        }

        private void OnDestroy()
        {
            ReleaseGrid();

            VegetationFacade.Release();
        }

        private void OnDrawGizmos()
        {
            DrawCells();
            DrawCollectedPlants();
        }

    }
}