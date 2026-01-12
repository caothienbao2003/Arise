using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GridUtilities;
using VContainer;
using VContainer.Unity;

namespace GridTool
{
    public class GridService : IGridService, IInitializable, IDisposable
    {
        [Inject] private GridDataSO gridData;
        private Grid<CellData> runtimeGrid;
        private Pathfinding<CellData> pathfinding;
        
        public Grid<CellData> RuntimeGrid => runtimeGrid;
        public Pathfinding<CellData> Pathfinding => pathfinding;
        public GridDataSO GridData => gridData;
        
        public void Initialize()
        {
            if (gridData == null)
            {
                Debug.LogError("[GridService] GridData is null!");
                return;
            }
            
            runtimeGrid = gridData.CreateRuntimeGrid();
            
            if (runtimeGrid == null || runtimeGrid.GetCellCount() == 0)
            {
                Debug.LogError("[GridService] Failed to create runtime grid or grid is empty!");
                return;
            }
            
            pathfinding = new Pathfinding<CellData>(runtimeGrid);
            
            Debug.Log($"[GridService] Initialized with {runtimeGrid.GetCellCount()} cells");
        }
        
        public void Dispose()
        {
            runtimeGrid = null;
            pathfinding = null;
            Debug.Log("[GridService] Disposed");
        }
        
        public CellData GetCellAt(Vector2Int position)
        {
            return runtimeGrid?.GetCellGridObject(position);
        }
        
        public bool IsWalkable(Vector2Int position)
        {
            var cell = GetCellAt(position);
            return cell != null && cell.IsWalkable;
        }
        
        public bool IsOccupied(Vector2Int position)
        {
            var cell = GetCellAt(position);
            return cell != null && cell.IsOccupied;
        }
        
        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            return runtimeGrid.GetCellCenterWorldPos(gridPos);
        }
        
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            return runtimeGrid.GetCellXYPos(worldPos);
        }
        
        public List<Vector2Int> GetNeighbors(Vector2Int position)
        {
            var neighbors = runtimeGrid.GetNeighbors(position);
            return neighbors?.Select(cell => cell.GridPosition).ToList() ?? new List<Vector2Int>();
        }
        
        public List<Vector2Int> GetWalkableNeighbors(Vector2Int position)
        {
            return GetNeighbors(position)
                .Where(pos => IsWalkable(pos) && !IsOccupied(pos))
                .ToList();
        }
        
        public void SetCellOccupied(Vector2Int position, bool occupied)
        {
            var cell = GetCellAt(position);
            if (cell != null)
            {
                cell.IsOccupied = occupied;
            }
        }
    }
}