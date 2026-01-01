using System;
using CTB;
using GridUtilities;
using UnityEngine;

namespace GridTool
{
    public class GridManager: MonoBehaviourSingleton<GridManager>
    {
        public LayerMask GridLayerMask;
        public Grid<CellData> CurrentRuntimeGrid { get; private set; }
        public Pathfinding<CellData> CurrentPathfinding { get; private set; }
        
        public void SetRuntimeGrid(Grid<CellData> grid)
        {
            CurrentRuntimeGrid = grid;
        }
        
        public void SetPathfinding(Pathfinding<CellData> pathfinding)
        {
            CurrentPathfinding = pathfinding;
        }
    }
}