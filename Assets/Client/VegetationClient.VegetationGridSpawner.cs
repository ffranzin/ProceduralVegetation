using System.Collections.Generic;
using UnityEngine;
using Vegetation.Rendering;

namespace Vegetation.Tests
{
    internal class VetetationCell
    {
        public int lastAccess { get; private set; }

        private readonly IVegetationAreaRenderable vegetationArea;

        public Bounds boundsWorld { get; private set; }

        public VetetationCell(Bounds boundsWorld)
        {
            this.boundsWorld = boundsWorld;

            vegetationArea = VegetationFacade.VegetationAreaRenderer(boundsWorld);
        }

        public void RenderVegetation()
        {
            lastAccess = Time.frameCount;
            vegetationArea.Render();
        }

        public void Release()
        {
            vegetationArea.Release();
        }
    }


    internal partial class VegetationClient
    {
        private const int OLD_CELL_INTERVAL = 100;
        private const int COLLECT_OLD_CELL_INTERVAL = 100;
        private const float TERRAIN_SIZE = 5000;

        [Header("Plants Grid Spawner")]

        public bool drawCells = false;
        [Range(1, 500)]
        [SerializeField] private float cellSize = 64f;

        [Range(1, 5000)]
        [SerializeField] private float SelectAroundDistance = 200;

        [SerializeField] private Material groundMaterial;


        private GameObject gridDebbuger;

        private VetetationCell[,] hash;

        private readonly List<Vector2Int> instancedCells = new List<Vector2Int>();

        private int cellsHCounter;
        private int cellsVCounter;

        private void InitializeGrid()
        {
            cellsHCounter = Mathf.FloorToInt((TERRAIN_SIZE / cellSize));
            cellsVCounter = Mathf.FloorToInt((TERRAIN_SIZE / cellSize));

            hash = new VetetationCell[cellsHCounter, cellsVCounter];

            gridDebbuger = GameObject.CreatePrimitive(PrimitiveType.Quad);
            gridDebbuger.name = "Vegetation Grid Debbuger";
            gridDebbuger.transform.position = new Vector3(0.5f, 0, 0.5f) * TERRAIN_SIZE;
            gridDebbuger.transform.localScale = new Vector3(1, 1, 1) * TERRAIN_SIZE;
            gridDebbuger.transform.Rotate(new Vector3(90, 0, 0));

            gridDebbuger.GetComponent<MeshRenderer>().material = groundMaterial;
        }

        private void InstanciateCellAroundPos()
        {
            List<VetetationCell> cells = GetCellsAroundPos(Camera.main.transform.position, SelectAroundDistance);

            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

            for (int i = 0; i < cells.Count; i++)
            {
                if (GeometryUtility.TestPlanesAABB(frustumPlanes, cells[i].boundsWorld))
                {
                    cells[i].RenderVegetation();
                }
            }
        }


        private List<VetetationCell> GetCellsAroundPos(Vector3 position, float radius)
        {
            List<VetetationCell> cells = new List<VetetationCell>();

            int l = (int)(position.x / cellSize);//floorToInt
            int c = (int)(position.z / cellSize);

            int offSetX = 1 + (int)(radius / cellSize);//ceilToInt
            int offSetY = 1 + (int)(radius / cellSize);

            //No cells outside of the terrain
            int iniX = Mathf.Clamp(l - offSetX, 0, cellsHCounter);
            int endX = Mathf.Clamp(l + offSetX, 0, cellsHCounter);

            int iniY = Mathf.Clamp(c - offSetY, 0, cellsVCounter);
            int endY = Mathf.Clamp(c + offSetY, 0, cellsVCounter);

            for (int i = iniX; i < endX; i++)
            {
                for (int j = iniY; j < endY; j++)
                {
                    hash[i, j] = hash[i, j] ?? CreateCell(i, j);

                    if (Mathf.Sqrt(hash[i, j].boundsWorld.SqrDistance(position)) < radius)
                    {
                        cells.Add(hash[i, j]);
                    }
                }
            }

            return cells;
        }


        private void CollectOldCells(bool force = false)
        {
            if (Time.frameCount % COLLECT_OLD_CELL_INTERVAL != 0)
            {
                return;
            }

            for (int i = 0; i < instancedCells.Count; i++)
            {
                if ((Time.frameCount - hash[instancedCells[i].x, instancedCells[i].y].lastAccess) > OLD_CELL_INTERVAL)
                {
                    hash[instancedCells[i].x, instancedCells[i].y].Release();
                    hash[instancedCells[i].x, instancedCells[i].y] = null;
                    instancedCells.RemoveAt(i);
                    i--;
                }
            }
        }


        private void DestroyAllCells()
        {
            for (int i = 0; i < instancedCells.Count; i++)
            {
                hash[instancedCells[i].x, instancedCells[i].y].Release();
                hash[instancedCells[i].x, instancedCells[i].y] = null;
                instancedCells.RemoveAt(i);
                i--;
            }
        }


        private void ReleaseGrid()
        {
            for (int i = 0; i < cellsHCounter; i++)
            {
                for (int j = 0; j < cellsVCounter; j++)
                {
                    hash[i, j] = null;
                }
            }

            hash = null;
        }

        private VetetationCell CreateCell(int hashPosI, int hashPosJ)
        {
            Vector3 center = new Vector3(cellSize * (hashPosI + 0.5f), 0, cellSize * (hashPosJ + 0.5f));
            Vector3 size = new Vector3(cellSize, 100, cellSize);

            VetetationCell cell = new VetetationCell(new Bounds(center, size));

            instancedCells.Add(new Vector2Int(hashPosI, hashPosJ));

            return cell;
        }

        #region DEBUG
        private void DrawCells()
        {
            if (drawCells && Application.isPlaying)
            {
                ShowGrid();
                ShowUsedCell();
            }
        }

        private void ShowGrid()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, TERRAIN_SIZE));
            Gizmos.DrawLine(Vector3.zero, new Vector3(TERRAIN_SIZE, 0, 0));
            Gizmos.DrawLine(new Vector3(TERRAIN_SIZE, 0, TERRAIN_SIZE), new Vector3(0, 0, TERRAIN_SIZE));
            Gizmos.DrawLine(new Vector3(TERRAIN_SIZE, 0, TERRAIN_SIZE), new Vector3(TERRAIN_SIZE, 0, 0));

            Gizmos.color = Color.green;
            for (float i = 0; i < TERRAIN_SIZE; i += cellSize)
            {
                Gizmos.DrawLine(new Vector3(i, 0, TERRAIN_SIZE), new Vector3(i, 0, 0));
                Gizmos.DrawLine(new Vector3(TERRAIN_SIZE, 0, i), new Vector3(0, 0, i));
            }
        }

        private void ShowUsedCell()
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < instancedCells.Count; i++)
            {
                VetetationCell cell = hash[instancedCells[i].x, instancedCells[i].y];

                if (cell.lastAccess == Time.frameCount)
                {
                    Gizmos.DrawWireCube(cell.boundsWorld.center, cell.boundsWorld.size);
                }
            }
        }
        #endregion

    }
}