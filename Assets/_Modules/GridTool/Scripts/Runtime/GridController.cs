using CTB;
using UnityEngine;
using GridUtilities;
using System.Collections.Generic;
using VContainer;

namespace GridTool
{
    public class GridController : MonoBehaviour
    {
        private GridInitializer gridInitializer;
        
        private GridInitializer GridInitializer
        {
            get
            {
                if (gridInitializer == null)
                {
                    gridInitializer = GetComponent<GridInitializer>();
                }
                return gridInitializer;
            }
        }
        private Pathfinding<CellData> pathfinding => GridInitializer.Pathfinding;
        private GridDataSO gridDataSO => GridInitializer.GridDataSO;
        
        private Camera mainCamera;
        
        private InputReader inputReader => InputReader.Instance;
        
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
            Vector2Int cellXYPos = pathfinding.GetGrid().GetCellXYPos(CameraUtils.GetMouseWorldPosition2D(inputReader.mousePosition));
            CellData cellData = pathfinding.GetGrid().GetCellGridObject(cellXYPos);

            CellData startCell = pathfinding.GetGrid().GetCellGridObject(0,0);
            CellData endCell = pathfinding.GetGrid().GetCellGridObject(cellXYPos);
            
            List<CellData> path = pathfinding.FindPath(startCell, endCell);
        }
    }
}