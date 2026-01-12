using UnityEngine;
using System.Collections.Generic;
using GridUtilities;

namespace GridTool
{
    public interface IGridService
    {
        Grid<CellData> RuntimeGrid { get; }
        Pathfinding<CellData> Pathfinding { get; }
        GridDataSO GridData { get; }
        
        void Initialize();
        
        CellData GetCellAt(Vector2Int position);
        bool IsWalkable(Vector2Int position);
        bool IsOccupied(Vector2Int position);
        Vector3 GridToWorld(Vector2Int gridPos);
        Vector2Int WorldToGrid(Vector3 worldPos);
        List<Vector2Int> GetNeighbors(Vector2Int position);
        List<Vector2Int> GetWalkableNeighbors(Vector2Int position);
        void SetCellOccupied(Vector2Int position, bool occupied);
    }
}