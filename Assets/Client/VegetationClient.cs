using UnityEngine;

namespace Vegetation.Tests
{

    internal partial class VegetationClient : MonoBehaviour
    {
        private void Awake()
        {
            VegetationFacade.Initialize();
            InitializeGrid();
        }

        private void FixedUpdate()
        {
            CollectOldCells();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DestroyAllCells();
                VegetationFacade.Initialize();
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