using UnityEngine;

namespace GridTool
{
    public class GridVisualizer: MonoBehaviour
    {
        [SerializeField] private GridDataSO gridData;
        [SerializeField] private Color cellColor = Color.green;
        
        private Grid<CellData> grid;
        
        void Start()
        {
            if (gridData != null)
            {
                grid = gridData.CreateRuntimeGrid();
            }
        }
        
        void OnDrawGizmos()
        {
            if (grid == null && gridData != null)
            {
                grid = gridData.CreateRuntimeGrid();
            }
            
            if (grid == null) return;
            
            Gizmos.color = cellColor;
            
            foreach (Vector2Int pos in grid.GetAllCellXYPositions())
            {
                Vector3 center = grid.GetCellCenterWorldPos(pos);
                Gizmos.DrawWireCube(center, new Vector3(gridData.CellSize, gridData.CellSize, 0.01f));
            }
        }
    }
}