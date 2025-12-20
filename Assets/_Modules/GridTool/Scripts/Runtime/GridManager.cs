using CTB_Utils;
using GridUtilities;

namespace GridTool
{
    public class GridManager: MonoBehaviourSingleton<GridManager>
    {
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