using UnityEngine;
using GridUtilities;
using System.Collections.Generic;
using VContainer;

namespace GridTool
{
    public class GridController : MonoBehaviour
    {
        private GridInitializer _gridInitializer;
        
        private GridInitializer gridInitializer
        {
            get
            {
                if (_gridInitializer == null)
                {
                    _gridInitializer = GetComponent<GridInitializer>();
                }
                return _gridInitializer;
            }
        }
        private Pathfinding<CellData> pathfinding => gridInitializer.Pathfinding;
        private GridDataSO gridDataSO => gridInitializer.GridDataSO;
        
        private Camera mainCamera;
        
        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
        }
        
        private void HandleClick()
        {
            Vector2Int cellXYPos = pathfinding.GetGrid().GetCellXYPos(GetMouseToWorldPosition());
            CellData cellData = pathfinding.GetGrid().GetCellGridObject(cellXYPos);

            CellData startCell = pathfinding.GetGrid().GetCellGridObject(0,0);
            CellData endCell = pathfinding.GetGrid().GetCellGridObject(cellXYPos);
            
            List<CellData> path = pathfinding.FindPath(startCell, endCell);
            
            if (cellData != null)
            {
                Debug.Log($"Clicked on {cellData.TerrainType.DisplayName}, Cell XY Position: {cellXYPos}");
                Debug.Log($"Cell is walkable: {cellData.IsWalkable}");
            }

            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.Log($"Path from {pathfinding.GetGrid().GetCellCenterWorldPos(path[i].GridPosition)} to {pathfinding.GetGrid().GetCellCenterWorldPos(path[i + 1].GridPosition)}");
                    Debug.DrawLine(pathfinding.GetGrid().GetCellCenterWorldPos(path[i].GridPosition), pathfinding.GetGrid().GetCellCenterWorldPos(path[i + 1].GridPosition), Color.red, 2f);
                }
            }
        }   
        
        private Vector2 GetMouseToWorldPosition()
        {
            if (!mainCamera) return Vector2.zero;

            Vector3 vec = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            vec.z = 0f;
            return vec;
        }
    }
}