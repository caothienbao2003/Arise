using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridTool
{
    [CreateAssetMenu(fileName = "NewGridData", menuName = "MapTool/Grid Data")]
    public class GridDataSO : ScriptableObject
    {
        [SerializeField] private List<CellData> cellDatas = new();
        [SerializeField] private float cellSize = 1f;

        public List<CellData> CellDatas => cellDatas;
        public float CellSize => cellSize;

        public void SetCellSize(float size)
        {
            cellSize = size;
        }
        
        public CellData GetCellAt(Vector2Int cellPosition)
        {
            return cellDatas?.Find(c => c.CellPosition == cellPosition);
        }

        public Grid<CellData> CreateRuntimeGrid()
        {
            if (cellDatas == null || cellDatas.Count == 0)
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
    }
}