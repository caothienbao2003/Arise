using System;
using CTB;
using GridUtilities;
using UnityEngine;

namespace GridTool
{
    public class GridManager: MonoBehaviourSingleton<GridManager>
    {
        public GridDataSO GridData;
        
        private Grid<CellData> runtimeGrid;
        public Grid<CellData> RuntimeGrid
        {
            get
            {
                if (runtimeGrid == null)
                {
                    runtimeGrid = GridData.CreateRuntimeGrid();
                }
                return runtimeGrid;
            }
        }

        private Pathfinding<CellData> pathfinding;

        public Pathfinding<CellData> Pathfinding
        {
            get
            {
                if (pathfinding == null)
                {
                    pathfinding = new Pathfinding<CellData>(RuntimeGrid);
                }
                return pathfinding;
            }
        }
    }
}