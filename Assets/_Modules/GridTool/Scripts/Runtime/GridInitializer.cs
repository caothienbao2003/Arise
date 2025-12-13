using GridUtilities;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace GridTool
{
    public class GridInitializer : MonoBehaviour
    {
        public GridDataSO GridDataSO;
        private Grid<CellData> _runtimeGrid;
        public Grid<CellData> RuntimeGrid
        {
            get
            {
                if (_runtimeGrid == null)
                {
                    _runtimeGrid = GridDataSO.CreateRuntimeGrid();
                }
                return _runtimeGrid;
            }
        }

        private Pathfinding<CellData> _pathfinding;

        public Pathfinding<CellData> Pathfinding
        {
            get
            {
                if (_pathfinding == null)
                {
                    _pathfinding = new Pathfinding<CellData>(RuntimeGrid);
                }
                return _pathfinding;
            }
        }
    }
}