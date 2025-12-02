using System.Collections.Generic;
using UnityEngine;
using GridUtilities;
using Sirenix.OdinInspector;
using System;

namespace GridTool
{
    [CreateAssetMenu(fileName = "NewGridData", menuName = "MapTool/Grid Data")]
    public class GridDataSO : ScriptableObject
    {
        [SerializeField] private List<CellData> cellDatas;
        [SerializeField] private float cellSize = 1f;
        
        public List<CellData> CellDatas
        {
            get
            {
                cellDatas ??= new List<CellData>();
                return cellDatas;
            }
        }
        public float CellSize => cellSize;

        public void ClearCellDatas()
        {
            CellDatas.Clear();
        }

        public void AddCellData(CellData cellData)
        {
            CellDatas.Add(cellData);
        }

        public void SortCellDatas()
        {
            CellDatas.Sort((a, b) =>
            {
                int compareY = b.CellPosition.y.CompareTo(a.CellPosition.y);
                return compareY != 0 ? compareY : a.CellPosition.x.CompareTo(b.CellPosition.x);
            });
        }
        
        public void SetCellSize(float size)
        {
            cellSize = size;
        }
        
        public CellData GetCellAt(Vector2Int cellPosition)
        {
            return CellDatas?.Find(c => c.CellPosition == cellPosition);
        }

        public Grid<CellData> CreateRuntimeGrid()
        {
            if (CellDatas == null || CellDatas.Count == 0)
            {
                Debug.LogWarning("GridDataSO has no CellDatas defined.");
                return null;
            }

            Vector3 origin = Vector3.zero;
            Grid<CellData> grid = new Grid<CellData>(cellSize, origin);
            
            // IMPORTANT: Add all cells to the grid!
            foreach (var cellData in cellDatas)
            {
                grid.SetCellGridObject(cellData.CellPosition, cellData);
            }
            
            Debug.Log($"Grid created with {grid.GetCellCount()} cells");
            
            return grid;
        }

        public CellData[,] GeneratePreviewMatrix()
        {
            if (cellDatas == null || cellDatas.Count == 0) return null;

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (CellData cellData in cellDatas)
            {
                minX = Mathf.Min(minX, cellData.CellPosition.x);
                minY = Mathf.Min(minY, cellData.CellPosition.y);
                maxX = Mathf.Max(maxX, cellData.CellPosition.x);
                maxY = Mathf.Max(maxY, cellData.CellPosition.y);
            }

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            
            CellData[,] matrix = new CellData[width, height];
            
            foreach (CellData cellData in cellDatas)
            {
                if (cellData == null) return null;
                
                int x = cellData.CellPosition.x - minX;
                int y = cellData.CellPosition.y - minY;
                
                matrix[x, y] = cellData;
            }
            
            return matrix;
        }
    }
}